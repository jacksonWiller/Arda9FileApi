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
        var query = new GetBucketByIdQuery
        {
            Id = bucketId
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            BucketName = "test-bucket",
            CompanyId = Guid.NewGuid()
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
        result.Value.Bucket.BucketName.Should().Be("test-bucket");
    }

    [Fact]
    public async Task Handle_WhenBucketNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var query = new GetBucketByIdQuery
        {
            Id = Guid.NewGuid()
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(query.Id))
            .ReturnsAsync((BucketModel?)null);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnError()
    {
        // Arrange
        var query = new GetBucketByIdQuery
        {
            Id = Guid.NewGuid()
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(query.Id))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_WithMultipleCalls_ShouldCallRepositoryOnce()
    {
        // Arrange
        var bucketId = Guid.NewGuid();
        var query = new GetBucketByIdQuery
        {
            Id = bucketId
        };

        var bucket = new BucketModel
        {
            Id = bucketId,
            BucketName = "test-bucket",
            CompanyId = Guid.NewGuid()
        };

        _bucketRepositoryMock.Setup(r => r.GetByIdAsync(bucketId))
            .ReturnsAsync(bucket);

        // Act
        await _handler.Handle(query, default);

        // Assert
        _bucketRepositoryMock.Verify(r => r.GetByIdAsync(bucketId), Times.Once);
    }
}
