using Arda9File.Application.Application.Buckets.Commands.DeleteBucket;
using Arda9File.Domain.Models;
using Arda9File.Domain.Repositories;
using Arda9FileApi.Application.Buckets.Commands.DeleteBucket;
using Amazon.S3;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Moq;

namespace Arda9File.UnitTest.Buckets.Commands;

public class DeleteBucketHandlerTests
{
    private readonly Mock<IAmazonS3> _s3ClientMock;
    private readonly Mock<IBucketRepository> _bucketRepositoryMock;
    private readonly Mock<IValidator<DeleteBucketCommand>> _validatorMock;
    private readonly Mock<ILogger<DeleteBucketHandler>> _loggerMock;
    private readonly DeleteBucketHandler _handler;

    public DeleteBucketHandlerTests()
    {
        _s3ClientMock = new Mock<IAmazonS3>();
        _bucketRepositoryMock = new Mock<IBucketRepository>();
        _validatorMock = new Mock<IValidator<DeleteBucketCommand>>();
        _loggerMock = new Mock<ILogger<DeleteBucketHandler>>();

        _handler = new DeleteBucketHandler(
            _s3ClientMock.Object,
            _bucketRepositoryMock.Object,
            _validatorMock.Object,
            _loggerMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidBucket_ShouldDeleteSuccessfully()
    {
        // Arrange
        var bucketName = "test-bucket";
        var command = new DeleteBucketCommand
        {
            BucketName = bucketName
        };

        var bucket = new BucketModel
        {
            Id = Guid.NewGuid(),
            BucketName = bucketName,
            CompanyId = Guid.NewGuid()
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _bucketRepositoryMock.Setup(r => r.GetByBucketNameAsync(bucketName))
            .ReturnsAsync(bucket);

        _bucketRepositoryMock.Setup(r => r.DeleteAsync(bucket.Id))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _bucketRepositoryMock.Verify(r => r.DeleteAsync(bucket.Id), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidData_ShouldReturnInvalidResult()
    {
        // Arrange
        var command = new DeleteBucketCommand
        {
            BucketName = ""
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
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Invalid);
        _bucketRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenBucketNotFound_ShouldReturnNotFound()
    {
        // Arrange
        var command = new DeleteBucketCommand
        {
            BucketName = "non-existent-bucket"
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
        _bucketRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenExceptionOccurs_ShouldReturnError()
    {
        // Arrange
        var bucketName = "test-bucket";
        var command = new DeleteBucketCommand
        {
            BucketName = bucketName
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _bucketRepositoryMock.Setup(r => r.GetByBucketNameAsync(bucketName))
            .ThrowsAsync(new Exception("Database error"));

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Error);
    }
}
