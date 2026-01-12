using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;
using Microsoft.Extensions.Logging;
using Arda9File.Application.Services;

namespace Arda9File.Application.Application.Files.Queries.GetFileDownloadUrl;

public class GetFileDownloadUrlQueryHandler : IRequestHandler<GetFileDownloadUrlQuery, Result<GetFileDownloadUrlResponse>>
{
    private readonly IFileRepository _repository;
    private readonly IS3Service _s3Service;
    private readonly ILogger<GetFileDownloadUrlQueryHandler> _logger;

    public GetFileDownloadUrlQueryHandler(
        IFileRepository repository,
        IS3Service s3Service,
        ILogger<GetFileDownloadUrlQueryHandler> logger)
    {
        _repository = repository;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<Result<GetFileDownloadUrlResponse>> Handle(GetFileDownloadUrlQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _repository.GetByIdAsync(request.FileId);
            
            if (file == null || file.IsDeleted)
            {
                _logger.LogWarning("File {FileId} not found", request.FileId);
                return Result<GetFileDownloadUrlResponse>.NotFound();
            }

            if (file.CompanyId != request.TenantId)
            {
                _logger.LogWarning("File {FileId} does not belong to tenant {TenantId}", 
                    request.FileId, request.TenantId);
                return Result<GetFileDownloadUrlResponse>.Forbidden();
            }

            // For now, return public URL or S3 URL
            // In a production system, you would generate a pre-signed URL
            var url = file.PublicUrl ?? await _s3Service.GetPublicUrlAsync(file.BucketName, file.S3Key);
            
            var response = new GetFileDownloadUrlResponse
            {
                Url = url,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // Signed URLs typically expire after 1 hour
                Filename = file.FileName,
                Size = file.Size
            };

            return Result<GetFileDownloadUrlResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting download URL for file {FileId}", request.FileId);
            return Result<GetFileDownloadUrlResponse>.Error("Failed to get download URL");
        }
    }
}
