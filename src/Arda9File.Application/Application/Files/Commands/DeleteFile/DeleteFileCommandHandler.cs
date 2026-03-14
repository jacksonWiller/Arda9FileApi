using Arda9File.Application.Application.Buckets.Commands.CreateBucket;
using Arda9File.Application.Services;
using Arda9FileApi.Repositories;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Arda9File.Application.Application.Files.Commands.DeleteFile;

public class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Result>
{
    private readonly IFileRepository _repository;
    private readonly IS3Service _s3Service;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteFileCommandHandler> _logger;


    public DeleteFileCommandHandler(
        IFileRepository repository,
        IS3Service s3Service,
        ICurrentUserService currentUserService,
        ILogger<DeleteFileCommandHandler> logger)
    {
        _repository = repository;
        _s3Service = s3Service;
        _currentUserService = currentUserService;
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