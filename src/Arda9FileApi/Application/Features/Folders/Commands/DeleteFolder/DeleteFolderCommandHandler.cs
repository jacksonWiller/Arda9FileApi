using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;

namespace Arda9FileApi.Application.Features.Folders.Commands.DeleteFolder;

public class DeleteFolderCommandHandler : IRequestHandler<DeleteFolderCommand, Result>
{
    private readonly IFolderRepository _repository;
    private readonly ILogger<DeleteFolderCommandHandler> _logger;

    public DeleteFolderCommandHandler(
        IFolderRepository repository,
        ILogger<DeleteFolderCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFolderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var folder = await _repository.GetByIdAsync(request.FolderId);
            if (folder == null || folder.IsDeleted)
            {
                _logger.LogWarning("Folder {FolderId} not found", request.FolderId);
                return Result.NotFound();
            }

            // Verificar se a pasta pertence ao tenant
            if (folder.CompanyId != request.TenantId)
            {
                _logger.LogWarning("Folder {FolderId} does not belong to tenant {TenantId}", 
                    request.FolderId, request.TenantId);
                return Result.Forbidden();
            }

            // Verificar se existem subpastas
            var subFolders = await _repository.GetByParentFolderIdAsync(request.FolderId);
            if (subFolders.Any(f => !f.IsDeleted))
            {
                _logger.LogWarning("Folder {FolderId} has subfolders and cannot be deleted", request.FolderId);
                return Result.Error();
            }

            // Soft delete
            folder.IsDeleted = true;
            folder.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(folder);

            _logger.LogInformation("Folder {FolderId} deleted successfully", request.FolderId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting folder {FolderId}", request.FolderId);
            return Result.Error();
        }
    }
}