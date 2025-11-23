using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Application.Services;
using Arda9FileApi.Infrastructure.Repositories;
using Arda9FileApi.Infrastructure.Services;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;

namespace Arda9FileApi.Application.Buckets.Commands.CreateBucket;

public class CreateBucketHandler : IRequestHandler<CreateBucketCommand, Result<CreateBucketResponse>>
{
    private readonly IS3Service _s3Service;
    private readonly IBucketRepository _bucketRepository;
    private readonly IValidator<CreateBucketCommand> _validator;
    private readonly ILogger<CreateBucketHandler> _logger;
    private readonly IAuthService _authService;

    public CreateBucketHandler(
        IS3Service s3Service,
        IBucketRepository bucketRepository,
        IValidator<CreateBucketCommand> validator,
        IAuthService authService,
        ILogger<CreateBucketHandler> logger)
    {
        _s3Service = s3Service;
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

            // Verificar se bucket já existe no S3
            var bucketExists = await _s3Service.BucketExistsAsync(request.BucketName, cancellationToken);
            if (bucketExists)
            {
                return Result<CreateBucketResponse>.Error();
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
                return Result<CreateBucketResponse>.Error();
            }

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

            _logger.LogInformation("Bucket {BucketName} criado com sucesso pelo usuário {UserId} (Público: {IsPublic})", 
                request.BucketName, userIdResult.Value, request.IsPublic);

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