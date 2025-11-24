using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Infrastructure.Repositories;
using MediatR;

namespace Arda9FileApi.Application.Files.Queries.GetFilesByBucket;

public class GetFilesByBucketQuery : IRequest<Result<List<FileMetadataDto>>>
{
    public Guid TenantId { get; set; }
    public Guid BucketId { get; set; }
}

public class GetFilesByBucketQueryHandler : IRequestHandler<GetFilesByBucketQuery, Result<List<FileMetadataDto>>>
{
    private readonly IFileRepository _fileRepository;
    private readonly IBucketRepository _bucketRepository;
    private readonly ILogger<GetFilesByBucketQueryHandler> _logger;

    public GetFilesByBucketQueryHandler(
        IFileRepository fileRepository,
        IBucketRepository bucketRepository,
        ILogger<GetFilesByBucketQueryHandler> logger)
    {
        _fileRepository = fileRepository;
        _bucketRepository = bucketRepository;
        _logger = logger;
    }

    public async Task<Result<List<FileMetadataDto>>> Handle(GetFilesByBucketQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var bucket = await _bucketRepository.GetByIdAsync(request.BucketId);
            if (bucket == null)
            {
                _logger.LogWarning("Bucket {BucketName} not found", request.BucketId);
                return Result<List<FileMetadataDto>>.NotFound();
            }

            if (bucket.CompanyId != request.TenantId)
            {
                _logger.LogWarning("Bucket {BucketName} does not belong to tenant {TenantId}", 
                    request.BucketId, request.TenantId);
                return Result<List<FileMetadataDto>>.Forbidden();
            }

            // Usar GSI1 para buscar arquivos por BucketId
            var files = await _fileRepository.GetByBucketIdAsync(bucket.Id);

            return Result<List<FileMetadataDto>>.Success(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving files for bucket {BucketName}", request.BucketId);
            return Result<List<FileMetadataDto>>.Error();
        }
    }
}