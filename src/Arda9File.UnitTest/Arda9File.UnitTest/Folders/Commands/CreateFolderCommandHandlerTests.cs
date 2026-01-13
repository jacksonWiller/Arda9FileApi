using Arda9File.Application.Application.Folders.Commands.CreateFolder;
using Arda9File.Domain.Models;
using Arda9File.Domain.Repositories;
using Arda9FileApi.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Arda9File.UnitTest.Folders.Commands;

public class CreateFolderCommandHandlerTests
{
    private readonly Mock<IFolderRepository> _folderRepositoryMock;
    private readonly Mock<IBucketRepository> _bucketRepositoryMock;
    private readonly Mock<ILogger<CreateFolderCommandHandler>> _loggerMock;
    private readonly CreateFolderCommandHandler _handler;

    public CreateFolderCommandHandlerTests()
    {
        _folderRepositoryMock = new Mock<IFolderRepository>();
        _bucketRepositoryMock = new Mock<IBucketRepository>();
        _loggerMock = new Mock<ILogger<CreateFolderCommandHandler>>();

        _handler = new CreateFolderCommandHandler(
            _folderRepositoryMock.Object,
            _bucketRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateFolderSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketId = Guid.NewGuid();
        var command = new CreateFolderCommand
        {
            FolderName = "test-folder",
            BucketId = bucketId,
            TenantId = tenantId,
            ParentFolderId = null,
            IsPublic = false
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            BucketName = "test-bucket",
            CompanyId = tenantId
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _folderRepositoryMock.Setup(r => r.GetByPathAndNameAsync(bucketId, string.Empty, command.FolderName))
            .ReturnsAsync((FolderModel?)null);

        _folderRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<FolderModel>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Folder.Should().NotBeNull();
        result.Value.Folder.FolderName.Should().Be(command.FolderName);
        result.Value.Folder.BucketId.Should().Be(bucketId);

        _folderRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<FolderModel>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithParentFolder_ShouldCreateFolderWithPath()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketId = Guid.NewGuid();
        var parentFolderId = Guid.NewGuid();
        var command = new CreateFolderCommand
        {
            FolderName = "subfolder",
            BucketId = bucketId,
            TenantId = tenantId,
            ParentFolderId = parentFolderId,
            IsPublic = false
        };

        var bucket = new BucketModel { Id = bucketId, CompanyId = tenantId };
        var parentFolder = new FolderModel
        {
            Id = parentFolderId,
            FolderName = "parent",
            BucketId = bucketId,
            Path = "",
            IsDeleted = false
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _folderRepositoryMock.Setup(r => r.GetByIdAsync(parentFolderId))
            .ReturnsAsync(parentFolder);

        _folderRepositoryMock.Setup(r => r.GetByPathAndNameAsync(bucketId, "parent", command.FolderName))
            .ReturnsAsync((FolderModel?)null);

        _folderRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<FolderModel>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Folder.Path.Should().Be("parent");
        result.Value.Folder.ParentFolderId.Should().Be(parentFolderId);
    }

    [Fact]
    public async Task Handle_WhenBucketNotFound_ShouldReturnError()
    {
        // Arrange
        var command = new CreateFolderCommand
        {
            FolderName = "test-folder",
            BucketId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(command.BucketId))
            .ReturnsAsync((BucketModel?)null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _folderRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<FolderModel>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenParentFolderNotFound_ShouldReturnError()
    {
        // Arrange
        var bucketId = Guid.NewGuid();
        var command = new CreateFolderCommand
        {
            FolderName = "test-folder",
            BucketId = bucketId,
            TenantId = Guid.NewGuid(),
            ParentFolderId = Guid.NewGuid()
        };

        var bucket = new BucketModel { Id = bucketId };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _folderRepositoryMock.Setup(r => r.GetByIdAsync(command.ParentFolderId!.Value))
            .ReturnsAsync((FolderModel?)null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _folderRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<FolderModel>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenParentFolderIsDeleted_ShouldReturnError()
    {
        // Arrange
        var bucketId = Guid.NewGuid();
        var parentFolderId = Guid.NewGuid();
        var command = new CreateFolderCommand
        {
            FolderName = "test-folder",
            BucketId = bucketId,
            TenantId = Guid.NewGuid(),
            ParentFolderId = parentFolderId
        };

        var bucket = new BucketModel { Id = bucketId };
        var parentFolder = new FolderModel
        {
            Id = parentFolderId,
            BucketId = bucketId,
            IsDeleted = true
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _folderRepositoryMock.Setup(r => r.GetByIdAsync(parentFolderId))
            .ReturnsAsync(parentFolder);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenParentFolderNotInSameBucket_ShouldReturnError()
    {
        // Arrange
        var bucketId = Guid.NewGuid();
        var parentFolderId = Guid.NewGuid();
        var command = new CreateFolderCommand
        {
            FolderName = "test-folder",
            BucketId = bucketId,
            TenantId = Guid.NewGuid(),
            ParentFolderId = parentFolderId
        };

        var bucket = new BucketModel { Id = bucketId };
        var parentFolder = new FolderModel
        {
            Id = parentFolderId,
            BucketId = Guid.NewGuid(), // Different bucket
            IsDeleted = false
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _folderRepositoryMock.Setup(r => r.GetByIdAsync(parentFolderId))
            .ReturnsAsync(parentFolder);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenFolderAlreadyExists_ShouldReturnError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketId = Guid.NewGuid();
        var command = new CreateFolderCommand
        {
            FolderName = "existing-folder",
            BucketId = bucketId,
            TenantId = tenantId
        };

        var bucket = new BucketModel { Id = bucketId };
        var existingFolder = new FolderModel
        {
            Id = Guid.NewGuid(),
            FolderName = command.FolderName,
            BucketId = bucketId
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _folderRepositoryMock.Setup(r => r.GetByPathAndNameAsync(bucketId, string.Empty, command.FolderName))
            .ReturnsAsync(existingFolder);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _folderRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<FolderModel>()), Times.Never);
    }
}
