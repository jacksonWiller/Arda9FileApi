using Arda9File.Application.Application.Buckets.Queries.GetAllBuckets;
using Arda9File.Domain.Models;
using Arda9File.Domain.Repositories;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace Arda9File.UnitTest.Buckets.Queries;

public class GetAllBucketsHandlerTests
{
    private readonly Mock<IBucketRepository> _bucketRepositoryMock;
    private readonly Mock<ILogger<GetAllBucketsHandler>> _loggerMock;
    private readonly GetAllBucketsHandler _handler;

    public GetAllBucketsHandlerTests()
    {
        _bucketRepositoryMock = new Mock<IBucketRepository>();
        _loggerMock = new Mock<ILogger<GetAllBucketsHandler>>();

        _handler = new GetAllBucketsHandler(
            _bucketRepositoryMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnAllBucketsForTenant()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetAllBucketsQuery { TenantId = tenantId };

        var buckets = new List<BucketModel>
        {
            new BucketModel { Id = Guid.NewGuid(), BucketName = "bucket1", CompanyId = tenantId },
            new BucketModel { Id = Guid.NewGuid(), BucketName = "bucket2", CompanyId = tenantId }
        };

        _bucketRepositoryMock.Setup(r => r.GetByCompanyIdAsync(tenantId))
            .ReturnsAsync(buckets);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Buckets.Should().HaveCount(2);
        result.Value.Buckets.Should().BeEquivalentTo(buckets);
    }

    [Fact]
    public async Task Handle_WhenNoBucketsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetAllBucketsQuery { TenantId = tenantId };

        _bucketRepositoryMock.Setup(r => r.GetByCompanyIdAsync(tenantId))
            .ReturnsAsync(new List<BucketModel>());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Buckets.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var query = new GetAllBucketsQuery { TenantId = tenantId };

        _bucketRepositoryMock.Setup(r => r.GetByCompanyIdAsync(tenantId))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Error);
    }
}
