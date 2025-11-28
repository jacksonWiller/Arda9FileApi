using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;

namespace Arda9FileApi.Application.Features.Files.Commands.RestoreFile;

public class RestoreFileCommandHandler : IRequestHandler<RestoreFileCommand, Result<RestoreFileResponse>>
{
    private readonly IFileRepository _repository;
    private readonly ILogger<RestoreFileCommandHandler> _logger;

    public RestoreFileCommandHandler(
        IFileRepository repository,
        ILogger<RestoreFileCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<RestoreFileResponse>> Handle(RestoreFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _repository.GetByIdAsync(request.FileId);
            
            if (file == null)
            {
                _logger.LogWarning("File {FileId} not found", request.FileId);
                return Result<RestoreFileResponse>.NotFound();
            }

            if (file.CompanyId != request.TenantId)
            {
                _logger.LogWarning("File {FileId} does not belong to tenant {TenantId}", 
                    request.FileId, request.TenantId);
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
