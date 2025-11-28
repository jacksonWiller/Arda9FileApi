using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;
using Arda9FileApi.Application.Services;

namespace Arda9FileApi.Application.Features.Files.Queries.DownloadFile;

public class DownloadFileQueryHandler : IRequestHandler<DownloadFileQuery, Result<DownloadFileResponse>>
{
    private readonly IFileRepository _repository;
    private readonly IS3Service _s3Service;
    private readonly ILogger<DownloadFileQueryHandler> _logger;

    public DownloadFileQueryHandler(
        IFileRepository repository,
        IS3Service s3Service,
        ILogger<DownloadFileQueryHandler> logger)
    {
        _repository = repository;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<Result<DownloadFileResponse>> Handle(DownloadFileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _repository.GetByIdAsync(request.FileId);
            
            if (file == null || file.IsDeleted)
            {
                _logger.LogWarning("File {FileId} not found", request.FileId);
                return Result<DownloadFileResponse>.NotFound();
            }

            if (file.CompanyId != request.TenantId)
            {
                _logger.LogWarning("File {FileId} does not belong to tenant {TenantId}", 
                    request.FileId, request.TenantId);
                return Result<DownloadFileResponse>.Forbidden();
            }

            var fileStream = await _s3Service.DownloadFileAsync(file.BucketName, file.S3Key, cancellationToken);
            
            if (fileStream == null)
            {
                _logger.LogError("Failed to download file from S3: {S3Key}", file.S3Key);
                return Result<DownloadFileResponse>.Error();
            }

            return Result<DownloadFileResponse>.Success(new DownloadFileResponse
            {
                FileStream = fileStream,
                FileName = file.FileName,
                ContentType = file.ContentType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file {FileId}", request.FileId);
            return Result<DownloadFileResponse>.Error();
        }
    }
}