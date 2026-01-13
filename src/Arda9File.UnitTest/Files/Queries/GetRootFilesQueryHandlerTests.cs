using Arda9File.Application.Application.Files.Queries.GetRootFiles;
using Arda9File.Domain.Models;
using Arda9File.Domain.Repositories;
using Arda9FileApi.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Arda9File.UnitTest.Files.Queries;

public class GetRootFilesQueryHandlerTests
{
    private readonly Mock<IFileRepository> _fileRepositoryMock;
    private readonly Mock<IBucketRepository> _bucketRepositoryMock;
    private readonly Mock<ILogger<GetRootFilesQueryHandler>> _loggerMock;
    private readonly GetRootFilesQueryHandler _handler;

    public GetRootFilesQueryHandlerTests()
    {
        _fileRepositoryMock = new Mock<IFileRepository>();
        _bucketRepositoryMock = new Mock<IBucketRepository>();
        _loggerMock = new Mock<ILogger<GetRootFilesQueryHandler>>();

        _handler = new GetRootFilesQueryHandler(
            _fileRepositoryMock.Object,
            _bucketRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnRootFilesSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketId = Guid.NewGuid();
        var query = new GetRootFilesQuery
        {
            BucketId = bucketId,
            TenantId = tenantId
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            BucketName = "test-bucket",
            CompanyId = tenantId
        };

        var allFiles = new List<FileMetadataModel>
        {
            new FileMetadataModel
            {
                FileId = Guid.NewGuid(),
                FileName = "root-file1.txt",
                FolderId = null, // Root file
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new FileMetadataModel
            {
                FileId = Guid.NewGuid(),
                FileName = "root-file2.txt",
                FolderId = null, // Root file
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow.AddMinutes(-5)
            },
            new FileMetadataModel
            {
                FileId = Guid.NewGuid(),
                FileName = "folder-file.txt",
                FolderId = Guid.NewGuid(), // Not a root file
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            }
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _fileRepositoryMock.Setup(r => r.GetByBucketIdAsync(bucketId))
            .ReturnsAsync(allFiles);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);
        result.Value.Should().AllSatisfy(f => f.FolderId.Should().BeNull());
        result.Value.First().FileName.Should().Be("root-file1.txt"); // Ordered by CreatedAt descending
    }

    [Fact]
    public async Task Handle_WhenBucketNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetRootFilesQuery
        {
            BucketId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(query.BucketId))
            .ReturnsAsync((BucketModel?)null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenBucketNotBelongsToTenant_ShouldReturnForbidden()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetRootFilesQuery
        {
            BucketId = Guid.NewGuid(),
            TenantId = tenantId
        };

        var bucket = new BucketModel
        {
            Id = query.BucketId,
            CompanyId = Guid.NewGuid() // Different tenant
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(query.BucketId))
            .ReturnsAsync(bucket);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenNoRootFiles_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketId = Guid.NewGuid();
        var query = new GetRootFilesQuery
        {
            BucketId = bucketId,
            TenantId = tenantId
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            CompanyId = tenantId
        };

        var allFiles = new List<FileMetadataModel>
        {
            new FileMetadataModel
            {
                FileId = Guid.NewGuid(),
                FileName = "folder-file.txt",
                FolderId = Guid.NewGuid(), // Not a root file
                IsDeleted = false
            }
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _fileRepositoryMock.Setup(r => r.GetByBucketIdAsync(bucketId))
            .ReturnsAsync(allFiles);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_ShouldExcludeDeletedFiles()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketId = Guid.NewGuid();
        var query = new GetRootFilesQuery
        {
            BucketId = bucketId,
            TenantId = tenantId
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            CompanyId = tenantId
        };

        var allFiles = new List<FileMetadataModel>
        {
            new FileMetadataModel
            {
                FileId = Guid.NewGuid(),
                FileName = "active-file.txt",
                FolderId = null,
                IsDeleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new FileMetadataModel
            {
                FileId = Guid.NewGuid(),
                FileName = "deleted-file.txt",
                FolderId = null,
                IsDeleted = true, // Deleted file
                CreatedAt = DateTime.UtcNow
            }
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _fileRepositoryMock.Setup(r => r.GetByBucketIdAsync(bucketId))
            .ReturnsAsync(allFiles);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().FileName.Should().Be("active-file.txt");
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnError()
    {
        // Arrange
        var query = new GetRootFilesQuery
        {
            BucketId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(query.BucketId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Error);
    }
}
