using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;
using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Folders.Queries.GetFoldersByParent;

public class GetFoldersByParentQueryHandler : IRequestHandler<GetFoldersByParentQuery, Result<List<FolderModel>>>
{
    private readonly IFolderRepository _repository;
    private readonly ILogger<GetFoldersByParentQueryHandler> _logger;

    public GetFoldersByParentQueryHandler(
        IFolderRepository repository,
        ILogger<GetFoldersByParentQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<List<FolderModel>>> Handle(GetFoldersByParentQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var parentFolder = await _repository.GetByIdAsync(request.ParentFolderId);
            if (parentFolder == null || parentFolder.IsDeleted)
            {
                _logger.LogWarning("Parent folder {ParentFolderId} not found", request.ParentFolderId);
                return Result<List<FolderModel>>.NotFound();
            }

            if (parentFolder.CompanyId != request.TenantId)
            {
                _logger.LogWarning("Parent folder {ParentFolderId} does not belong to tenant {TenantId}", 
                    request.ParentFolderId, request.TenantId);
                return Result<List<FolderModel>>.Forbidden();
            }

            var folders = await _repository.GetByParentFolderIdAsync(request.ParentFolderId);
            var activeFolders = folders.Where(f => !f.IsDeleted).ToList();

            return Result<List<FolderModel>>.Success(activeFolders);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving subfolders for parent {ParentFolderId}", request.ParentFolderId);
            return Result<List<FolderModel>>.Error();
        }
    }
}