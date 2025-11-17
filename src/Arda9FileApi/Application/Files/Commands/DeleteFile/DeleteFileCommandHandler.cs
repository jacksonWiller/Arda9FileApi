using Ardalis.Result;
using Arda9FileApi.Infrastructure.Repositories;
using Arda9FileApi.Infrastructure.Services;
using MediatR;

namespace Arda9FileApi.Application.Files.Commands.DeleteFile;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result>
{
    private readonly IFileRepository _repository;
    private readonly IS3Service _s3Service;
    private readonly ILogger<DeleteFileCommandHandler> _logger;

    public DeleteFileCommandHandler(
        IFileRepository repository,
        IS3Service s3Service,
        ILogger<DeleteFileCommandHandler> logger)
    {
        _repository = repository;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _repository.GetByIdAsync(request.FileId);
            if (file == null || file.IsDeleted)
            {
                _logger.LogWarning("File {FileId} not found", request.FileId);
                return Result.NotFound();
            }

            if (file.CompanyId != request.TenantId)
            {
                _logger.LogWarning("File {FileId} does not belong to tenant {TenantId}", 
                    request.FileId, request.TenantId);
                return Result.Forbidden();
            }

            if (request.HardDelete)
            {
                // Deletar arquivo do S3
                var deleteSuccess = await _s3Service.DeleteFileAsync(file.BucketName, file.S3Key, cancellationToken);
                if (!deleteSuccess)
                {
                    _logger.LogWarning("Failed to delete file from S3: {S3Key}", file.S3Key);
                }

                // Deletar metadata do banco
                await _repository.DeleteAsync(request.FileId);

                _logger.LogInformation("File {FileId} permanently deleted", request.FileId);
            }
            else
            {
                // Soft delete
                file.IsDeleted = true;
                file.UpdatedAt = DateTime.UtcNow;

                await _repository.UpdateAsync(file);

                _logger.LogInformation("File {FileId} soft deleted", request.FileId);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileId}", request.FileId);
            return Result.Error();
        }
    }
}