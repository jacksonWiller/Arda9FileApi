using Amazon.S3;
using Ardalis.Result;
using Arda9FileApi.Application.Buckets.DTOs;
using Arda9FileApi.Infrastructure.Repositories;
using MediatR;

namespace Arda9FileApi.Application.Buckets.Queries.GetAllBuckets;

public class GetAllBucketsHandler : IRequestHandler<GetAllBucketsQuery, Result<GetAllBucketsResponse>>
{
    private readonly IAmazonS3 _s3Client;
    private readonly IBucketRepository _bucketRepository;
    private readonly ILogger<GetAllBucketsHandler> _logger;

    public GetAllBucketsHandler(
        IAmazonS3 s3Client,
        IBucketRepository bucketRepository,
        ILogger<GetAllBucketsHandler> logger)
    {
        _s3Client = s3Client;
        _bucketRepository = bucketRepository;
        _logger = logger;
    }

    public async Task<Result<GetAllBucketsResponse>> Handle(GetAllBucketsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Buscar todos os buckets do S3
            var s3Response = await _s3Client.ListBucketsAsync(cancellationToken);

            // Buscar metadados do DynamoDB
            //var dynamoBuckets = request.CompanyId.HasValue
            //    ? await _bucketRepository.GetByCompanyIdAsync(request.CompanyId.Value)
            //    : await _bucketRepository.GetAllAsync();

            // Criar um dicionário para lookup rápido
            //var dynamoBucketsDict = dynamoBuckets.ToDictionary(b => b.BucketName, b => b);

            // Combinar dados do S3 com metadados do DynamoDB
            //var buckets = s3Response.Buckets.Select(s3Bucket =>
            //{
            //    if (dynamoBucketsDict.TryGetValue(s3Bucket.BucketName, out var dynamoBucket))
            //    {
            //        // Se existe no DynamoDB, usar os dados de lá com informações do S3
            //        dynamoBucket.CreatedAt = s3Bucket.CreationDate;
            //        return dynamoBucket;
            //    }
            //    else
            //    {
            //        // Se não existe no DynamoDB, criar um DTO básico com dados do S3
            //        return new BucketDto
            //        {
            //            BucketName = s3Bucket.BucketName,
            //            CreatedAt = s3Bucket.CreationDate,
            //            Status = "Active",
            //            Region = "us-east-1", // Região padrão, pode ser melhorado
            //            Id = Guid.Empty, // Indica que não tem registro no DynamoDB
            //            CompanyId = Guid.Empty,
            //            UpdatedAt = s3Bucket.CreationDate,
            //            GSI1PK = string.Empty,
            //            GSI2PK = $"BUCKET#{s3Bucket.BucketName}"
            //        };
            //    }
            //}).ToList();

            // Filtrar por CompanyId se fornecido
            //if (request.CompanyId.HasValue)
            //{
            //    buckets = buckets.Where(b => b.CompanyId == request.CompanyId.Value).ToList();
            //}

            //_logger.LogInformation("Retrieved {Count} buckets from S3", buckets.Count);

            var buckets = s3Response.Buckets.Select(s3Bucket => new BucketDto
            {
                BucketName = s3Bucket.BucketName,
                CreatedAt = s3Bucket.CreationDate,
                Status = "Active",
                Region = "us-east-1",
                Id = Guid.Empty,
                CompanyId = Guid.Empty,
                UpdatedAt = s3Bucket.CreationDate,
                GSI1PK = string.Empty,
                GSI2PK = $"BUCKET#{s3Bucket.BucketName}"
            }).ToList();

            return Result<GetAllBucketsResponse>.Success(new GetAllBucketsResponse
            {
                Buckets = buckets,
                TotalCount = buckets.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar buckets do S3");
            return Result<GetAllBucketsResponse>.Error();
        }
    }
}