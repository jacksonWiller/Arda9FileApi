using Arda9File.Application.Application.Buckets.Commands.CreateBucket;
using Arda9File.Application.Application.Folders.Commands.CreateFolder;
using Arda9File.Application.Services;
using Arda9File.Domain.Models;
using Arda9File.Domain.Repositories;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;

namespace Arda9File.UnitTest.Buckets.Commands;

public class CreateBucketHandlerTests
{
    private readonly Mock<IS3Service> _s3ServiceMock;
    private readonly Mock<IBucketRepository> _bucketRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IValidator<CreateBucketCommand>> _validatorMock;
    private readonly Mock<ILogger<CreateBucketHandler>> _loggerMock;
    private readonly Mock<IMediator> _mediatorMock;
    private readonly CreateBucketHandler _handler;

    public CreateBucketHandlerTests()
    {
        _s3ServiceMock = new Mock<IS3Service>();
        _bucketRepositoryMock = new Mock<IBucketRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _validatorMock = new Mock<IValidator<CreateBucketCommand>>();
        _loggerMock = new Mock<ILogger<CreateBucketHandler>>();
        _mediatorMock = new Mock<IMediator>();

        _handler = new CreateBucketHandler(
            _s3ServiceMock.Object,
            _bucketRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _validatorMock.Object,
            _loggerMock.Object,
            _mediatorMock.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldCreateBucketSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var command = new CreateBucketCommand
        {
            BucketName = "test-bucket",
            TenantId = tenantId,
            IsPublic = false
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(userId);

        _bucketRepositoryMock.Setup(r => r.GetByBucketNameAsync(command.BucketName))
            .ReturnsAsync((BucketModel?)null);

        _s3ServiceMock.Setup(s => s.BucketExistsAsync(command.BucketName, default))
            .ReturnsAsync(false);

        _s3ServiceMock.Setup(s => s.CreateBucketAsync(command.BucketName, default))
            .ReturnsAsync(true);

        _bucketRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<BucketModel>()))
            .Returns(Task.CompletedTask);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFolderCommand>(), default))
            .ReturnsAsync(Ardalis.Result.Result<CreateFolderResponse>.Success(new CreateFolderResponse()));

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Bucket.Should().NotBeNull();
        result.Value.Bucket.BucketName.Should().Be(command.BucketName);
        result.Value.Bucket.CompanyId.Should().Be(tenantId);

        _bucketRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<BucketModel>()), Times.Once);
        _s3ServiceMock.Verify(s => s.CreateBucketAsync(command.BucketName, default), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidData_ShouldReturnInvalidResult()
    {
        // Arrange
        var command = new CreateBucketCommand
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
        result.Status.Should().Be(Ardalis.Result.ResultStatus.Invalid);
    }

    [Fact]
    public async Task Handle_WithEmptyTenantId_ShouldReturnError()
    {
        // Arrange
        var command = new CreateBucketCommand
        {
            BucketName = "test-bucket",
            TenantId = Guid.Empty
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WithEmptyUserId_ShouldReturnError()
    {
        // Arrange
        var command = new CreateBucketCommand
        {
            BucketName = "test-bucket",
            TenantId = Guid.NewGuid()
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(string.Empty);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenBucketExistsInDatabase_ShouldReturnError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var command = new CreateBucketCommand
        {
            BucketName = "existing-bucket",
            TenantId = tenantId
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(Guid.NewGuid().ToString());

        _bucketRepositoryMock.Setup(r => r.GetByBucketNameAsync(command.BucketName))
            .ReturnsAsync(new BucketModel { BucketName = command.BucketName });

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _s3ServiceMock.Verify(s => s.CreateBucketAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenBucketExistsInS3_ShouldReturnError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var command = new CreateBucketCommand
        {
            BucketName = "existing-s3-bucket",
            TenantId = tenantId
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(userId);

        _bucketRepositoryMock.Setup(r => r.GetByBucketNameAsync(command.BucketName))
            .ReturnsAsync((BucketModel?)null);

        _s3ServiceMock.Setup(s => s.BucketExistsAsync(command.BucketName, default))
            .ReturnsAsync(true);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _s3ServiceMock.Verify(s => s.CreateBucketAsync(It.IsAny<string>(), default), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenS3CreationFails_ShouldReturnError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var command = new CreateBucketCommand
        {
            BucketName = "test-bucket",
            TenantId = tenantId
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(userId);

        _bucketRepositoryMock.Setup(r => r.GetByBucketNameAsync(command.BucketName))
            .ReturnsAsync((BucketModel?)null);

        _s3ServiceMock.Setup(s => s.BucketExistsAsync(command.BucketName, default))
            .ReturnsAsync(false);

        _s3ServiceMock.Setup(s => s.CreateBucketAsync(command.BucketName, default))
            .ReturnsAsync(false);

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        _bucketRepositoryMock.Verify(r => r.CreateAsync(It.IsAny<BucketModel>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithPublicBucket_ShouldCreatePublicBucketSuccessfully()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        var command = new CreateBucketCommand
        {
            BucketName = "public-bucket",
            TenantId = tenantId,
            IsPublic = true
        };

        _validatorMock.Setup(v => v.ValidateAsync(command, default))
            .ReturnsAsync(new ValidationResult());

        _currentUserServiceMock.Setup(c => c.GetUserId())
            .Returns(userId);

        _bucketRepositoryMock.Setup(r => r.GetByBucketNameAsync(command.BucketName))
            .ReturnsAsync((BucketModel?)null);

        _s3ServiceMock.Setup(s => s.BucketExistsAsync(command.BucketName, default))
            .ReturnsAsync(false);

        _s3ServiceMock.Setup(s => s.CreatePublicBucketAsync(command.BucketName, default))
            .ReturnsAsync(true);

        _bucketRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<BucketModel>()))
            .Returns(Task.CompletedTask);

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateFolderCommand>(), default))
            .ReturnsAsync(Ardalis.Result.Result<CreateFolderResponse>.Success(new CreateFolderResponse()));

        // Act
        var result = await _handler.Handle(command, default);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        _s3ServiceMock.Verify(s => s.CreatePublicBucketAsync(command.BucketName, default), Times.Once);
        _s3ServiceMock.Verify(s => s.CreateBucketAsync(It.IsAny<string>(), default), Times.Never);
    }
}
