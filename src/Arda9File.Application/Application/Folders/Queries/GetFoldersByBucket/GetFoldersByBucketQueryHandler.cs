using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;
using Arda9File.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Folders.Queries.GetFoldersByBucket;

public class GetFoldersByBucketQueryHandler : IRequestHandler<GetFoldersByBucketQuery, Result<List<FolderModel>>>
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

    public async Task<Result<List<FolderModel>>> Handle(GetFoldersByBucketQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var bucket = await _bucketRepository.GetByIdAsync(request.BucketId);
            if (bucket == null)
            {
                _logger.LogWarning("Bucket {BucketId} not found", request.BucketId);
                return Result<List<FolderModel>>.NotFound();
            }

            if (bucket.TenantId != request.TenantId)
            {
                _logger.LogWarning("Bucket {BucketId} does not belong to tenant {TenantId}", 
                    request.BucketId, request.TenantId);
                return Result<List<FolderModel>>.Forbidden();
            }

            var folders = await _repository.GetByBucketIdAsync(request.BucketId);
            var activeFolders = folders.Where(f => !f.IsDeleted).ToList();

            return Result<List<FolderModel>>.Success(activeFolders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folders for bucket {BucketId}", request.BucketId);
            return Result<List<FolderModel>>.Error();
        }
    }
}