using Arda9File.Application.Services;
using Arda9File.Domain.Repositories;
using Arda9FileApi.Models;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arda9File.Application.Application.Buckets.Commands.CreateBucket;

public class CreateBucketHandler : IRequestHandler<CreateBucketCommand, Result<CreateBucketResponse>>
{
    private readonly IS3Service _s3Service;
    private readonly IBucketRepository _bucketRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IValidator<CreateBucketCommand> _validator;
    private readonly ILogger<CreateBucketHandler> _logger;

    public CreateBucketHandler(
        IS3Service s3Service,
        IBucketRepository bucketRepository,
        ICurrentUserService currentUserService,
        IValidator<CreateBucketCommand> validator,
        ILogger<CreateBucketHandler> logger)
    {
        _s3Service = s3Service;
        _bucketRepository = bucketRepository;
        _currentUserService = currentUserService;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<CreateBucketResponse>> Handle(CreateBucketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validação
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result<CreateBucketResponse>.Invalid(validationResult.AsErrors());
            }

            // Extrair TenantId e UserId do token JWT
            //var tenantId = _currentUserService.GetTenantId();
            var tenantId = request.TenantId;
            if (tenantId == Guid.Empty)
            {
                _logger.LogWarning("TenantId not found in token");
                return Result<CreateBucketResponse>.Error("TenantId not found in token");
            }

            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId not found in token");
                return Result<CreateBucketResponse>.Error("UserId not found in token");
            }

            // Verificar se bucket já existe
            var existingBucket = await _bucketRepository.GetByBucketNameAsync(request.BucketName);
            if (existingBucket != null)
            {
                _logger.LogWarning("Bucket {BucketName} already exists in database", request.BucketName);
                return Result<CreateBucketResponse>.Error("Bucket {BucketName} already exists in database");
            }

            // Verificar se bucket já existe no S3
            var bucketExists = await _s3Service.BucketExistsAsync(request.BucketName, cancellationToken);
            if (bucketExists)
            {
                _logger.LogWarning("Bucket {BucketName} already exists in S3", request.BucketName);
                return Result<CreateBucketResponse>.Error("Bucket {BucketName} already exists in S3");
            }

            // Criar bucket no S3 usando o S3Service
            bool bucketCreated;
            if (request.IsPublic)
            {
                bucketCreated = await _s3Service.CreatePublicBucketAsync(request.BucketName, cancellationToken);
            }
            else
            {
                bucketCreated = await _s3Service.CreateBucketAsync(request.BucketName, cancellationToken);
            }

            if (!bucketCreated)
            {
                _logger.LogError("Failed to create bucket {BucketName} in S3", request.BucketName);
                return Result<CreateBucketResponse>.Error("Failed to create bucket {BucketName} in S3");
            }

            // Criar registro no DynamoDB
            var bucketDto = new BucketModel
            {
                Id = Guid.NewGuid(),
                BucketName = request.BucketName,
                CompanyId = tenantId,
                Region = "us-east-1",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            await _bucketRepository.CreateAsync(bucketDto);

            _logger.LogInformation("Bucket {BucketName} criado com sucesso pelo usuário {UserId} (Público: {IsPublic})", 
                request.BucketName, userId, request.IsPublic);

            return Result<CreateBucketResponse>.Success(new CreateBucketResponse
            {
                Bucket = bucketDto,
                Message = "Bucket criado com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar bucket: {BucketName}", request.BucketName);
            return Result<CreateBucketResponse>.Error();
        }
    }
}