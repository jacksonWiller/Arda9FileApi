using Amazon.S3;
using Amazon.S3.Model;
using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Application.Services;
using Arda9FileApi.Infrastructure.Repositories;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;

namespace Arda9FileApi.Application.Buckets.Commands.CreateBucket;

public class CreateBucketHandler : IRequestHandler<CreateBucketCommand, Result<CreateBucketResponse>>
{
    private readonly IAmazonS3 _s3Client;
    private readonly IBucketRepository _bucketRepository;
    private readonly IValidator<CreateBucketCommand> _validator;
    private readonly ILogger<CreateBucketHandler> _logger;
    private readonly IAuthService _authService;

    public CreateBucketHandler(
        IAmazonS3 s3Client,
        IBucketRepository bucketRepository,
        IValidator<CreateBucketCommand> validator,
        IAuthService authService,
        ILogger<CreateBucketHandler> logger)
    {
        _s3Client = s3Client;
        _bucketRepository = bucketRepository;
        _validator = validator;
        _authService = authService;
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

            // Obter ID do usuário autenticado
            var userIdResult = _authService.GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return Result<CreateBucketResponse>.Unauthorized();
            }

            // Verificar se bucket já existe
            var existingBucket = await _bucketRepository.GetByBucketNameAsync(request.BucketName);
            if (existingBucket != null)
            {
                return Result<CreateBucketResponse>.Error();
            }

            // Criar bucket no S3
            var bucketRequest = new PutBucketRequest
            {
                BucketName = request.BucketName,
                UseClientRegion = true
            };

            await _s3Client.PutBucketAsync(bucketRequest, cancellationToken);

            // Criar registro no DynamoDB
            var bucketDto = new BucketDto
            {
                Id = Guid.NewGuid(),
                BucketName = request.BucketName,
                CompanyId = request.TenantId,
                Region = "us-east-1",
                Status = "Active",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                CreatedBy = userIdResult.Value
            };

            await _bucketRepository.CreateAsync(bucketDto);

            _logger.LogInformation("Bucket {BucketName} criado com sucesso pelo usuário {UserId}", 
                request.BucketName, userIdResult.Value);

            return Result<CreateBucketResponse>.Success(new CreateBucketResponse
            {
                Bucket = bucketDto,
                Message = "Bucket criado com sucesso"
            });
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar bucket no S3: {BucketName}", request.BucketName);
            return Result<CreateBucketResponse>.Error();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar bucket: {BucketName}", request.BucketName);
            return Result<CreateBucketResponse>.Error();
        }
    }
}