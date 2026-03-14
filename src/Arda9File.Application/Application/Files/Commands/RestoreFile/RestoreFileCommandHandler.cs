using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;
using Arda9File.Application.Services;
using Microsoft.Extensions.Logging;

namespace Arda9File.Application.Application.Files.Commands.RestoreFile;

public class RestoreFileCommandHandler : IRequestHandler<RestoreFileCommand, Result<RestoreFileResponse>>
{
    private readonly IFileRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RestoreFileCommandHandler> _logger;

    public RestoreFileCommandHandler(
        IFileRepository repository,
        ICurrentUserService currentUserService,
        ILogger<RestoreFileCommandHandler> logger)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<RestoreFileResponse>> Handle(RestoreFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Extrair TenantId e UserId do token JWT
            var tenantId = _currentUserService.GetTenantId();
            if (tenantId == Guid.Empty)
            {
                _logger.LogWarning("TenantId not found in token");
                return Result<RestoreFileResponse>.Error("TenantId not found in token");
            }

            var userId = _currentUserService.GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogWarning("UserId not found in token");
                return Result<RestoreFileResponse>.Error("UserId not found in token");
            }

            var file = await _repository.GetByIdAsync(request.FileId);

            if (file == null)
            {
                _logger.LogWarning("File {FileId} not found", request.FileId);
                return Result<RestoreFileResponse>.NotFound();
            }

            if (file.TenantId != tenantId)
            {
                _logger.LogWarning("File {FileId} does not belong to tenant {TenantId}", 
                    request.FileId, tenantId);
                return Result<RestoreFileResponse>.Forbidden();
            }

            if (!file.IsDeleted)
            {
                _logger.LogWarning("File {FileId} is not deleted", request.FileId);
                return Result<RestoreFileResponse>.Error("File is not deleted");
            }

            file.IsDeleted = false;
            file.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(file);

            _logger.LogInformation("File {FileId} restored successfully", file.FileId);

            return Result<RestoreFileResponse>.Success(new RestoreFileResponse
            {
                Id = file.FileId,
                Name = file.FileName,
                FolderId = file.FolderId,
                RestoredAt = file.UpdatedAt.Value
            }, "Arquivo restaurado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring file {FileId}", request.FileId);
            return Result<RestoreFileResponse>.Error("Failed to restore file");
        }
    }
}
