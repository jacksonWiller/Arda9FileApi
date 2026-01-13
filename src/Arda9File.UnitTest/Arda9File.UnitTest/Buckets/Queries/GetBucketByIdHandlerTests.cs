using Arda9File.Application.Application.Buckets.Queries.GetBucketById;
using Arda9File.Domain.Models;
using Arda9File.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Arda9File.UnitTest.Buckets.Queries;

public class GetBucketByIdHandlerTests
{
    private readonly Mock<IBucketRepository> _bucketRepositoryMock;
    private readonly Mock<ILogger<GetBucketByIdHandler>> _loggerMock;
    private readonly GetBucketByIdHandler _handler;

    public GetBucketByIdHandlerTests()
    {
        _bucketRepositoryMock = new Mock<IBucketRepository>();
        _loggerMock = new Mock<ILogger<GetBucketByIdHandler>>();

        _handler = new GetBucketByIdHandler(
            _bucketRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidBucketId_ShouldReturnBucket()
    {
        // Arrange
        var bucketId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var query = new GetBucketByIdQuery
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

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Bucket.Should().Be(bucket);
    }

    [Fact]
    public async Task Handle_WhenBucketNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetBucketByIdQuery
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
        var bucketId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var query = new GetBucketByIdQuery
        {
            BucketId = bucketId,
            TenantId = tenantId
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            BucketName = "test-bucket",
            CompanyId = Guid.NewGuid() // Different tenant
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnError()
    {
        // Arrange
        var query = new GetBucketByIdQuery
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
