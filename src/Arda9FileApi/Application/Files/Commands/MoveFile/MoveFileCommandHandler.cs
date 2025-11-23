using Ardalis.Result;
using Arda9FileApi.Infrastructure.Repositories;
using Arda9FileApi.Infrastructure.Services;
using MediatR;

namespace Arda9FileApi.Application.Files.Commands.MoveFile;

public class MoveFileCommandHandler : IRequestHandler<MoveFileCommand, Result<MoveFileResponse>>
{
    private readonly IFileRepository _repository;
    private readonly IFolderRepository _folderRepository;
    private readonly IS3Service _s3Service;
    private readonly ILogger<MoveFileCommandHandler> _logger;

    public MoveFileCommandHandler(
        IFileRepository repository,
        IFolderRepository folderRepository,
        IS3Service s3Service,
        ILogger<MoveFileCommandHandler> logger)
    {
        _repository = repository;
        _folderRepository = folderRepository;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<Result<MoveFileResponse>> Handle(MoveFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _repository.GetByIdAsync(request.FileId);
            
            if (file == null || file.IsDeleted)
            {
                _logger.LogWarning("File {FileId} not found", request.FileId);
                return Result<MoveFileResponse>.NotFound();
            }

            if (file.CompanyId != request.TenantId)
            {
                _logger.LogWarning("File {FileId} does not belong to tenant {TenantId}", 
                    request.FileId, request.TenantId);
                return Result<MoveFileResponse>.Forbidden();
            }

            // If moving to same folder, no action needed
            if (file.FolderId == request.FolderId)
            {
                return Result<MoveFileResponse>.Success(new MoveFileResponse
                {
                    Id = file.FileId,
                    FolderId = file.FolderId
                }, "File is already in the target folder");
            }

            var oldS3Key = file.S3Key;
            string? newFolderPath = null;

            // Validate target folder if specified
            if (request.FolderId.HasValue)
            {
                var folder = await _folderRepository.GetByIdAsync(request.FolderId.Value);
                if (folder == null || folder.IsDeleted)
                {
                    _logger.LogWarning("Target folder {FolderId} not found", request.FolderId);
                    return Result<MoveFileResponse>.NotFound("Target folder not found");
                }

                if (folder.CompanyId != request.TenantId)
                {
                    _logger.LogWarning("Target folder {FolderId} does not belong to tenant {TenantId}", 
                        request.FolderId, request.TenantId);
                    return Result<MoveFileResponse>.Forbidden();
                }

                newFolderPath = string.IsNullOrEmpty(folder.Path) 
                    ? folder.FolderName 
                    : $"{folder.Path}/{folder.FolderName}";
            }

            // Build new S3 key
            var newS3Key = _s3Service.BuildS3Key(newFolderPath, file.FileId, file.FileName);

            if (newS3Key != oldS3Key)
            {
                // Download file
                var fileStream = await _s3Service.DownloadFileAsync(file.BucketName, oldS3Key, cancellationToken);
                if (fileStream == null)
                {
                    _logger.LogError("Failed to download file from S3: {S3Key}", oldS3Key);
                    return Result<MoveFileResponse>.Error("Failed to download file");
                }

                // Upload to new location
                var uploadSuccess = await _s3Service.UploadFileAsync(
                    file.BucketName,
                    newS3Key,
                    fileStream,
                    file.ContentType,
                    file.IsPublic,
                    cancellationToken);

                if (!uploadSuccess)
                {
                    _logger.LogError("Failed to upload file to new location: {S3Key}", newS3Key);
                    return Result<MoveFileResponse>.Error("Failed to upload file to new location");
                }

                // Delete old file
                await _s3Service.DeleteFileAsync(file.BucketName, oldS3Key, cancellationToken);

                file.S3Key = newS3Key;

                if (file.IsPublic)
                {
                    file.PublicUrl = await _s3Service.GetPublicUrlAsync(file.BucketName, newS3Key);
                }
            }

            file.Folder = newFolderPath;
            file.FolderId = request.FolderId;
            file.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(file);

            _logger.LogInformation("File {FileId} moved successfully to folder {FolderId}", 
                file.FileId, request.FolderId);

            return Result<MoveFileResponse>.Success(new MoveFileResponse
            {
                Id = file.FileId,
                FolderId = file.FolderId
            }, "Arquivo movido com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moving file {FileId}", request.FileId);
            return Result<MoveFileResponse>.Error("Failed to move file");
        }
    }
}
