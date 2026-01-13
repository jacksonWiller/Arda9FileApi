using Arda9File.Application.Application.Files.Commands.UploadFile;
using Arda9File.Application.Services;
using Arda9File.Domain.Models;
using Arda9File.Domain.Repositories;
using Arda9FileApi.Repositories;
using Amazon.S3;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Arda9File.UnitTest.Files.Commands;

public class UploadFileCommandHandlerTests
{
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IBucketRepository> _bucketRepositoryMock;
    private readonly Mock<IFolderRepository> _folderRepositoryMock;
    private readonly Mock<IS3Service> _s3ServiceMock;
    private readonly Mock<IAmazonS3> _s3ClientMock;
    private readonly Mock<ILogger<UploadFileCommandHandler>> _loggerMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UploadFileCommandHandler _handler;

    public UploadFileCommandHandlerTests()
    {
        _fileRepositoryMock = new Mock<IFileRepository>();
        _bucketRepositoryMock = new Mock<IBucketRepository>();
        _folderRepositoryMock = new Mock<IFolderRepository>();
        _s3ServiceMock = new Mock<IS3Service>();
        _s3ClientMock = new Mock<IAmazonS3>();
        _loggerMock = new Mock<ILogger<UploadFileCommandHandler>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();

        _handler = new UploadFileCommandHandler(
            _fileRepositoryMock.Object,
            _bucketRepositoryMock.Object,
            _folderRepositoryMock.Object,
            _s3ServiceMock.Object,
            _s3ClientMock.Object,
            _loggerMock.Object,
            null!, // validator not used in handler
            _currentUserServiceMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidFile_ShouldUploadSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var fileMock = new Mock<IFormFile>();
        var content = "Hello World from a Fake File";
        var fileName = "test.txt";
        var ms = new MemoryStream();
        var writer = new StreamWriter(ms);
        writer.Write(content);
        writer.Flush();
        ms.Position = 0;

        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.ContentType).Returns("text/plain");
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);

        var command = new UploadFileCommand
        {
            File = fileMock.Object,
            BucketId = bucketId,
            TenantId = tenantId,
            FolderId = null,
            IsPublic = false
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            BucketName = "test-bucket",
            CompanyId = tenantId
        };

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(userId);

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _s3ServiceMock.Setup(s => s.BuildS3Key(It.IsAny<string>(), fileName))
            .Returns($"files/{fileName}");

        _s3ServiceMock.Setup(s => s.UploadFileAsync(
            bucket.BucketName,
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            false,
            default))
            .ReturnsAsync(true);

        _fileRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<FileMetadataModel>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.FileMetadata.Should().NotBeNull();
        result.Value.FileMetadata.FileName.Should().Be(fileName);
        result.Value.FileMetadata.BucketName.Should().Be(bucket.BucketName);

        _fileRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<FileMetadataModel>()), Times.Once);
        _s3ServiceMock.Verify(s => s.UploadFileAsync(
            bucket.BucketName,
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            false,
            default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithPublicFile_ShouldGeneratePublicUrl()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var fileMock = new Mock<IFormFile>();
        var fileName = "public-file.jpg";
        var ms = new MemoryStream(new byte[] { 1, 2, 3 });

        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);

        var command = new UploadFileCommand
        {
            File = fileMock.Object,
            BucketId = bucketId,
            TenantId = tenantId,
            IsPublic = true
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            BucketName = "test-bucket",
            CompanyId = tenantId
        };

        var publicUrl = $"https://test-bucket.s3.amazonaws.com/files/{fileName}";

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(userId);

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _s3ServiceMock.Setup(s => s.BuildS3Key(It.IsAny<string>(), fileName))
            .Returns($"files/{fileName}");

        _s3ServiceMock.Setup(s => s.UploadFileAsync(
            bucket.BucketName,
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            true,
            default))
            .ReturnsAsync(true);

        _s3ServiceMock.Setup(s => s.GetPublicUrlAsync(bucket.BucketName, It.IsAny<string>()))
            .ReturnsAsync(publicUrl);

        _fileRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<FileMetadataModel>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.FileMetadata.IsPublic.Should().BeTrue();
        result.Value.FileMetadata.PublicUrl.Should().Be(publicUrl);

        _s3ServiceMock.Verify(s => s.GetPublicUrlAsync(bucket.BucketName, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserIdNotFound_ShouldReturnForbidden()
    {
        // Arrange
        var command = new UploadFileCommand
        {
            File = new Mock<IFormFile>().Object,
            BucketId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(string.Empty);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenBucketNotFound_ShouldReturnError()
    {
        // Arrange
        var command = new UploadFileCommand
        {
            File = new Mock<IFormFile>().Object,
            BucketId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(Guid.NewGuid().ToString());

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(command.BucketId))
            .ReturnsAsync((BucketModel?)null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenFileIsNull_ShouldReturnInvalid()
    {
        // Arrange
        var command = new UploadFileCommand
        {
            File = null!,
            BucketId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(Guid.NewGuid().ToString());

        var bucket = new BucketModel { Id = command.BucketId };
        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(command.BucketId))
            .ReturnsAsync(bucket);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WhenFileIsEmpty_ShouldReturnInvalid()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);

        var command = new UploadFileCommand
        {
            File = fileMock.Object,
            BucketId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(Guid.NewGuid().ToString());

        var bucket = new BucketModel { Id = command.BucketId };
        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(command.BucketId))
            .ReturnsAsync(bucket);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WhenFolderNotFound_ShouldReturnError()
    {
        // Arrange
        var folderId = Guid.NewGuid();
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(100);

        var command = new UploadFileCommand
        {
            File = fileMock.Object,
            BucketId = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            FolderId = folderId
        };

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(Guid.NewGuid().ToString());

        var bucket = new BucketModel { Id = command.BucketId };
        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(command.BucketId))
            .ReturnsAsync(bucket);

        _folderRepositoryMock.Setup(r => r.GetByIdAsync(folderId))
            .ReturnsAsync((FolderModel?)null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenS3UploadFails_ShouldReturnError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();

        var fileMock = new Mock<IFormFile>();
        var ms = new MemoryStream(new byte[] { 1, 2, 3 });
        fileMock.Setup(f => f.FileName).Returns("test.txt");
        fileMock.Setup(f => f.Length).Returns(ms.Length);
        fileMock.Setup(f => f.ContentType).Returns("text/plain");
        fileMock.Setup(f => f.OpenReadStream()).Returns(ms);

        var command = new UploadFileCommand
        {
            File = fileMock.Object,
            BucketId = bucketId,
            TenantId = tenantId
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            BucketName = "test-bucket",
            CompanyId = tenantId
        };

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(userId);

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _s3ServiceMock.Setup(s => s.BuildS3Key(It.IsAny<string>(), It.IsAny<string>()))
            .Returns("files/test.txt");

        _s3ServiceMock.Setup(s => s.UploadFileAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            default))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _fileRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<FileMetadataModel>()), Times.Never);
    }
}
