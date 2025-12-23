using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;
using Arda9FileApi.Models;
using Microsoft.Extensions.Logging;

namespace Arda9FileApi.Application.Folders.Queries.GetFolderById;

public class GetFolderByIdQueryHandler : IRequestHandler<GetFolderByIdQuery, Result<FolderModel>>
{
    private readonly IFolderRepository _repository;
    private readonly ILogger<GetFolderByIdQueryHandler> _logger;

    public GetFolderByIdQueryHandler(
        IFolderRepository repository,
        ILogger<GetFolderByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<FolderModel>> Handle(GetFolderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var folder = await _repository.GetByIdAsync(request.FolderId);
            
            if (folder == null || folder.IsDeleted)
            {
                _logger.LogWarning("Folder {FolderId} not found", request.FolderId);
                return Result<FolderModel>.NotFound();
            }

            if (folder.CompanyId != request.TenantId)
            {
                _logger.LogWarning("Folder {FolderId} does not belong to tenant {TenantId}", 
                    request.FolderId, request.TenantId);
                return Result<FolderModel>.Forbidden();
            }

            return Result<FolderModel>.Success(folder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folder {FolderId}", request.FolderId);
            return Result<FolderModel>.Error();
        }
    }
}