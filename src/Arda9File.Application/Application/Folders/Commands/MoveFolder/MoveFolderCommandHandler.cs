using Ardalis.Result;
using MediatR;
using Arda9File.Application.Services;
using Arda9FileApi.Repositories;
using Microsoft.Extensions.Logging;

namespace Arda9File.Application.Application.Folders.Commands.MoveFolder;

public class MoveFolderCommandHandler : IRequestHandler<MoveFolderCommand, Result<MoveFolderResponse>>
{
    private readonly IFolderRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<MoveFolderCommandHandler> _logger;

    public MoveFolderCommandHandler(
        IFolderRepository repository,
        ICurrentUserService currentUserService,
        ILogger<MoveFolderCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<MoveFolderResponse>> Handle(MoveFolderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Extrair TenantId e UserId do token JWT
            var tenantId = _currentUserService.GetTenantId();
            if (tenantId == Guid.Empty)
            {
                _logger.LogWarning("TenantId not found in token");
                return Result<MoveFolderResponse>.Error("TenantId not found in token");
            }

            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId not found in token");
                return Result<MoveFolderResponse>.Error("UserId not found in token");
            }

            var folder = await _repository.GetByIdAsync(request.FolderId);

            if (folder == null || folder.IsDeleted)
            {
                _logger.LogWarning("Folder {FolderId} not found", request.FolderId);
                return Result<MoveFolderResponse>.NotFound();
            }

            if (folder.TenantId != tenantId)
            {
                _logger.LogWarning("Folder {FolderId} does not belong to tenant {TenantId}", 
                    request.FolderId, tenantId);
                return Result<MoveFolderResponse>.Forbidden();
            }

            // Check if already in target location
            if (folder.ParentFolderId == request.ParentId)
            {
                return Result<MoveFolderResponse>.Success(new MoveFolderResponse
                {
                    Id = folder.Id,
                    ParentId = folder.ParentFolderId,
                    Path = BuildPathList(folder.Path, folder.FolderName)
                }, "Folder is already in target location");
            }

            string? newPath = null;

            // Validate target parent folder if specified
            if (request.ParentId.HasValue)
            {
                var parentFolder = await _repository.GetByIdAsync(request.ParentId.Value);

                if (parentFolder == null || parentFolder.IsDeleted)
                {
                    _logger.LogWarning("Parent folder {ParentId} not found", request.ParentId);
                    return Result<MoveFolderResponse>.NotFound("Parent folder not found");
                }

                if (parentFolder.TenantId != tenantId)
                {
                    _logger.LogWarning("Parent folder {ParentId} does not belong to tenant {TenantId}", 
                        request.ParentId, tenantId);
                    return Result<MoveFolderResponse>.Forbidden();
                }

                // Prevent circular reference - can't move folder into itself or its descendants
                if (request.ParentId == request.FolderId)
                {
                    return Result<MoveFolderResponse>.Error("Cannot move folder into itself");
                }

                // Build new path
                newPath = string.IsNullOrEmpty(parentFolder.Path)
                    ? parentFolder.FolderName
                    : $"{parentFolder.Path}/{parentFolder.FolderName}";
            }

            folder.ParentFolderId = request.ParentId;
            folder.Path = newPath ?? string.Empty;
            folder.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(folder);

            _logger.LogInformation("Folder {FolderId} moved successfully", folder.Id);

            return Result<MoveFolderResponse>.Success(new MoveFolderResponse
            {
                Id = folder.Id,
                ParentId = folder.ParentFolderId,
                Path = BuildPathList(folder.Path, folder.FolderName)
            }, "Pasta movida com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving folder {FolderId}", request.FolderId);
            return Result<MoveFolderResponse>.Error("Failed to move folder");
        }
    }

    private List<string> BuildPathList(string? path, string folderName)
    {
        var pathList = new List<string>();
        
        if (!string.IsNullOrEmpty(path))
        {
            pathList.AddRange(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
        }
        
        pathList.Add(folderName);
        
        return pathList;
    }
}
