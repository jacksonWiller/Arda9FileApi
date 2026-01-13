using Arda9File.Application.Application.Folders.Queries.GetFoldersByBucket;
using Arda9File.Domain.Models;
using Arda9File.Domain.Repositories;
using Arda9FileApi.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Arda9File.UnitTest.Folders.Queries;

public class GetFoldersByBucketQueryHandlerTests
{
    private readonly Mock<IFolderRepository> _folderRepositoryMock;
    private readonly Mock<IBucketRepository> _bucketRepositoryMock;
    private readonly Mock<ILogger<GetFoldersByBucketQueryHandler>> _loggerMock;
    private readonly GetFoldersByBucketQueryHandler _handler;

    public GetFoldersByBucketQueryHandlerTests()
    {
        _folderRepositoryMock = new Mock<IFolderRepository>();
        _bucketRepositoryMock = new Mock<IBucketRepository>();
        _loggerMock = new Mock<ILogger<GetFoldersByBucketQueryHandler>>();

        _handler = new GetFoldersByBucketQueryHandler(
            _folderRepositoryMock.Object,
            _bucketRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnFoldersForBucket()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketId = Guid.NewGuid();
        var query = new GetFoldersByBucketQuery
        {
            BucketId = bucketId,
            TenantId = tenantId
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            CompanyId = tenantId
        };

        var folders = new List<FolderModel>
        {
            new FolderModel { Id = Guid.NewGuid(), FolderName = "folder1", BucketId = bucketId },
            new FolderModel { Id = Guid.NewGuid(), FolderName = "folder2", BucketId = bucketId }
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _folderRepositoryMock.Setup(r => r.GetByBucketIdAsync(bucketId))
            .ReturnsAsync(folders);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Should().BeEquivalentTo(folders);
    }

    [Fact]
    public async Task Handle_WhenBucketNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetFoldersByBucketQuery
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
        var query = new GetFoldersByBucketQuery
        {
            BucketId = Guid.NewGuid(),
            TenantId = Guid.NewGuid()
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
    public async Task Handle_WhenNoFolders_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketId = Guid.NewGuid();
        var query = new GetFoldersByBucketQuery
        {
            BucketId = bucketId,
            TenantId = tenantId
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            CompanyId = tenantId
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        _folderRepositoryMock.Setup(r => r.GetByBucketIdAsync(bucketId))
            .ReturnsAsync(new List<FolderModel>());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnError()
    {
        // Arrange
        var query = new GetFoldersByBucketQuery
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
