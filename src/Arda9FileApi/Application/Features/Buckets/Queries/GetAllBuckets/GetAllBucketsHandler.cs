using Amazon.S3;
using Ardalis.Result;
using MediatR;
using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Repositories;

namespace Arda9FileApi.Application.Features.Buckets.Queries.GetAllBuckets;

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
            var dynamoBuckets = await _bucketRepository.GetAllAsync();

            // Criar um dicionário para lookup rápido
            var dynamoBucketsDict = dynamoBuckets.ToDictionary(b => b.BucketName, b => b);

            // Combinar dados - APENAS buckets que existem em AMBOS (S3 e DynamoDB)
            var buckets = s3Response.Buckets
                .Where(s3Bucket => dynamoBucketsDict.ContainsKey(s3Bucket.BucketName))
                .Select(s3Bucket =>
                {
                    var dynamoBucket = dynamoBucketsDict[s3Bucket.BucketName];
                    // Atualizar com a data de criação do S3
                    dynamoBucket.CreatedAt = s3Bucket.CreationDate;
                    return dynamoBucket;
                })
                .ToList();

            _logger.LogInformation("Retrieved {Count} buckets matching both S3 and DynamoDB", buckets.Count);

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