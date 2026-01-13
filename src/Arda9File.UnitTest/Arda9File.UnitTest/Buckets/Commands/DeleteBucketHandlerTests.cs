using Arda9File.Application.Application.Buckets.Commands.DeleteBucket;
using Arda9File.Application.Services;
using Arda9File.Domain.Models;
using Arda9File.Domain.Repositories;
using Arda9FileApi.Application.Buckets.Commands.DeleteBucket;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace Arda9File.UnitTest.Buckets.Commands;

public class DeleteBucketHandlerTests
{
    private readonly Mock<IS3Service> _s3ServiceMock;
    private readonly Mock<IBucketRepository> _bucketRepositoryMock;
    private readonly Mock<IValidator<DeleteBucketCommand>> _validatorMock;
    private readonly Mock<ILogger<DeleteBucketHandler>> _loggerMock;
    private readonly DeleteBucketHandler _handler;

    public DeleteBucketHandlerTests()
    {
        _s3ServiceMock = new Mock<IS3Service>();
        _bucketRepositoryMock = new Mock<IBucketRepository>();
        _validatorMock = new Mock<IValidator<DeleteBucketCommand>>();
        _loggerMock = new Mock<ILogger<DeleteBucketHandler>>();

        _handler = new DeleteBucketHandler(
            _s3ServiceMock.Object,
            _bucketRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidBucket_ShouldDeleteSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketName = "test-bucket";
        var command = new DeleteBucketCommand
        {
            BucketName = bucketName,
            TenantId = tenantId
        };

        var bucket = new BucketModel
        {
            Id = Guid.NewGuid(),
            BucketName = bucketName,
            CompanyId = tenantId
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _bucketRepositoryMock.Setup(r => r.GetByBucketNameAsync(bucketName))
            .ReturnsAsync(bucket);

        _s3ServiceMock.Setup(s => s.DeleteBucketAsync(bucketName, default))
            .ReturnsAsync(true);

        _bucketRepositoryMock.Setup(r => r.DeleteAsync(bucket.Id))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _s3ServiceMock.Verify(s => s.DeleteBucketAsync(bucketName, default), Times.Once);
        _bucketRepositoryMock.Verify(r => r.DeleteAsync(bucket.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidData_ShouldReturnInvalidResult()
    {
        // Arrange
        var command = new DeleteBucketCommand
        {
            BucketName = "",
            TenantId = Guid.NewGuid()
        };

        var validationResult = new ValidationResult(new[]
        {
            new ValidationFailure("BucketName", "Bucket name is required")
        });

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(validationResult);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _bucketRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenBucketNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = new DeleteBucketCommand
        {
            BucketName = "non-existent-bucket",
            TenantId = Guid.NewGuid()
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _bucketRepositoryMock.Setup(r => r.GetByBucketNameAsync(command.BucketName))
            .ReturnsAsync((BucketModel?)null);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.NotFound);
        _s3ServiceMock.Verify(s => s.DeleteBucketAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenBucketNotBelongsToTenant_ShouldReturnForbidden()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new DeleteBucketCommand
        {
            BucketName = "test-bucket",
            TenantId = tenantId
        };

        var bucket = new BucketModel
        {
            Id = Guid.NewGuid(),
            BucketName = command.BucketName,
            CompanyId = Guid.NewGuid() // Different tenant
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _bucketRepositoryMock.Setup(r => r.GetByBucketNameAsync(command.BucketName))
            .ReturnsAsync(bucket);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Forbidden);
        _s3ServiceMock.Verify(s => s.DeleteBucketAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenS3DeletionFails_ShouldReturnError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var bucketName = "test-bucket";
        var command = new DeleteBucketCommand
        {
            BucketName = bucketName,
            TenantId = tenantId
        };

        var bucket = new BucketModel
        {
            Id = Guid.NewGuid(),
            BucketName = bucketName,
            CompanyId = tenantId
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _bucketRepositoryMock.Setup(r => r.GetByBucketNameAsync(bucketName))
            .ReturnsAsync(bucket);

        _s3ServiceMock.Setup(s => s.DeleteBucketAsync(bucketName, default))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _bucketRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }
}
