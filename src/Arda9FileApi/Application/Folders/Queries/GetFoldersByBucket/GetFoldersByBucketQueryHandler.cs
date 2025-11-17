using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Infrastructure.Repositories;
using MediatR;

namespace Arda9FileApi.Application.Folders.Queries.GetFoldersByBucket;

public class GetFoldersByBucketQueryHandler : IRequestHandler<GetFoldersByBucketQuery, Result<List<FolderDto>>>
{
    private readonly IFolderRepository _repository;
    private readonly IBucketRepository _bucketRepository;
    private readonly ILogger<GetFoldersByBucketQueryHandler> _logger;

    public GetFoldersByBucketQueryHandler(
        IFolderRepository repository,
        IBucketRepository bucketRepository,
        ILogger<GetFoldersByBucketQueryHandler> logger)
    {
        _repository = repository;
        _bucketRepository = bucketRepository;
        _logger = logger;
    }

    public async Task<Result<List<FolderDto>>> Handle(GetFoldersByBucketQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var bucket = await _bucketRepository.GetByIdAsync(request.BucketId);
            if (bucket == null)
            {
                _logger.LogWarning("Bucket {BucketId} not found", request.BucketId);
                return Result<List<FolderDto>>.NotFound();
            }

            if (bucket.CompanyId != request.TenantId)
            {
                _logger.LogWarning("Bucket {BucketId} does not belong to tenant {TenantId}", 
                    request.BucketId, request.TenantId);
                return Result<List<FolderDto>>.Forbidden();
            }

            var folders = await _repository.GetByBucketIdAsync(request.BucketId);
            var activeFolders = folders.Where(f => !f.IsDeleted).ToList();

            return Result<List<FolderDto>>.Success(activeFolders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folders for bucket {BucketId}", request.BucketId);
            return Result<List<FolderDto>>.Error();
        }
    }
}