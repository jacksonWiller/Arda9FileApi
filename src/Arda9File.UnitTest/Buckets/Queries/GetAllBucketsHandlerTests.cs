using Arda9File.Application.Application.Buckets.Queries.GetAllBuckets;
using Arda9File.Domain.Models;
using Arda9File.Domain.Repositories;
using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;

namespace Arda9File.UnitTest.Buckets.Queries;

public class GetAllBucketsHandlerTests
{
    private readonly Mock<IAmazonS3> _s3ClientMock;
    private readonly Mock<IBucketRepository> _bucketRepositoryMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<ILogger<GetAllBucketsHandler>> _loggerMock;
    private readonly GetAllBucketsHandler _handler;

    public GetAllBucketsHandlerTests()
    {
        _s3ClientMock = new Mock<IAmazonS3>();
        _bucketRepositoryMock = new Mock<IBucketRepository>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _loggerMock = new Mock<ILogger<GetAllBucketsHandler>>();

        _handler = new GetAllBucketsHandler(
            _s3ClientMock.Object,
            _bucketRepositoryMock.Object,
            _httpContextAccessorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnAllBucketsFromS3AndDynamoDB()
    {
        // Arrange
        var query = new GetAllBucketsQuery();

        var s3Buckets = new ListBucketsResponse
        {
            Buckets = new List<S3Bucket>
            {
                new S3Bucket { BucketName = "bucket1", CreationDate = DateTime.UtcNow },
                new S3Bucket { BucketName = "bucket2", CreationDate = DateTime.UtcNow }
            }
        };

        var dynamoBuckets = new List<BucketModel>
        {
            new BucketModel { Id = Guid.NewGuid(), BucketName = "bucket1", TenantId = Guid.NewGuid() },
            new BucketModel { Id = Guid.NewGuid(), BucketName = "bucket2", TenantId = Guid.NewGuid() }
        };

        _s3ClientMock.Setup(s => s.ListBucketsAsync(default))
            .ReturnsAsync(s3Buckets);

        _bucketRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(dynamoBuckets);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Buckets.Should().HaveCount(2);
        result.Value.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WhenNoBucketsExist_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllBucketsQuery();

        var s3Buckets = new ListBucketsResponse
        {
            Buckets = new List<S3Bucket>()
        };

        _s3ClientMock.Setup(s => s.ListBucketsAsync(default))
            .ReturnsAsync(s3Buckets);

        _bucketRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(new List<BucketModel>());

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Buckets.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnError()
    {
        // Arrange
        var query = new GetAllBucketsQuery();

        _s3ClientMock.Setup(s => s.ListBucketsAsync(default))
            .ThrowsAsync(new Exception("S3 error"));

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Error);
    }

    [Fact]
    public async Task Handle_ShouldOnlyReturnBucketsInBothS3AndDynamoDB()
    {
        // Arrange
        var query = new GetAllBucketsQuery();

        var s3Buckets = new ListBucketsResponse
        {
            Buckets = new List<S3Bucket>
            {
                new S3Bucket { BucketName = "bucket1", CreationDate = DateTime.UtcNow },
                new S3Bucket { BucketName = "bucket2", CreationDate = DateTime.UtcNow },
                new S3Bucket { BucketName = "bucket-only-in-s3", CreationDate = DateTime.UtcNow }
            }
        };

        var dynamoBuckets = new List<BucketModel>
        {
            new BucketModel { Id = Guid.NewGuid(), BucketName = "bucket1", CompanyId = Guid.NewGuid() },
            new BucketModel { Id = Guid.NewGuid(), BucketName = "bucket2", CompanyId = Guid.NewGuid() },
            new BucketModel { Id = Guid.NewGuid(), BucketName = "bucket-only-in-dynamo", CompanyId = Guid.NewGuid() }
        };

        _s3ClientMock.Setup(s => s.ListBucketsAsync(default))
            .ReturnsAsync(s3Buckets);

        _bucketRepositoryMock.Setup(r => r.GetAllAsync())
            .ReturnsAsync(dynamoBuckets);

        // Act
        var result = await _handler.Handle(query, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Buckets.Should().HaveCount(2);
        result.Value.Buckets.Should().AllSatisfy(b =>
        {
            b.BucketName.Should().BeOneOf("bucket1", "bucket2");
        });
    }
}
