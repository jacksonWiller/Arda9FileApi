using Ardalis.Result;
using MediatR;
using Arda9File.Application.Services;
using Arda9FileApi.Repositories;
using Microsoft.Extensions.Logging;

namespace Arda9File.Application.Application.Folders.Commands.UpdateFolder;

public class UpdateFolderCommandHandler : IRequestHandler<UpdateFolderCommand, Result<UpdateFolderResponse>>
{
    private readonly IFolderRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<UpdateFolderCommandHandler> _logger;

    public UpdateFolderCommandHandler(
        IFolderRepository repository,
        ICurrentUserService currentUserService,
        ILogger<UpdateFolderCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<UpdateFolderResponse>> Handle(UpdateFolderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Extrair TenantId e UserId do token JWT
            var tenantId = _currentUserService.GetTenantId();
            if (tenantId == Guid.Empty)
            {
                _logger.LogWarning("TenantId not found in token");
                return Result.Error("TenantId not found in token");
            }

            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId not found in token");
                return Result.Error("UserId not found in token");
            }

            var folder = await _repository.GetByIdAsync(request.FolderId);
            if (folder == null || folder.IsDeleted)
            {
                _logger.LogWarning("Folder {FolderId} not found", request.FolderId);
                return Result.NotFound();
            }

            // Verificar se a pasta pertence ao tenant
            if (folder.TenantId != tenantId)
            {
                _logger.LogWarning("Folder {FolderId} does not belong to tenant {TenantId}", 
                    request.FolderId, tenantId);
                return Result.Forbidden();
            }

            // Atualizar apenas os campos fornecidos
            if (!string.IsNullOrEmpty(request.FolderName))
            {
                // Verificar se já existe uma pasta com o mesmo nome no mesmo path
                var existingFolder = await _repository.GetByPathAndNameAsync(
                    folder.BucketId, folder.Path, request.FolderName);

                if (existingFolder != null && existingFolder.Id != request.FolderId)
                {
                    _logger.LogWarning("Folder with name {FolderName} already exists in path {Path}", 
                        request.FolderName, folder.Path);
                    return Result.Error();
                }

                folder.FolderName = request.FolderName;
            }

            if (request.IsPublic.HasValue)
            {
                folder.IsPublic = request.IsPublic.Value;
            }

            folder.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(folder);

            _logger.LogInformation("Folder {FolderId} updated successfully", folder.Id);

            return Result<UpdateFolderResponse>.Success(new UpdateFolderResponse
            {
                Folder = folder,
                Message = "Folder updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating folder {FolderId}", request.FolderId);
            return Result<UpdateFolderResponse>.Error();
        }
    }
}