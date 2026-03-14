using Arda9File.Application.Application.Buckets.Commands.CreateBucket;
using Arda9File.Application.Services;
using Arda9File.Domain.Models;
using Arda9FileApi.Repositories;
using Ardalis.Result;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arda9File.Application.Application.Files.Commands.DuplicateFile;

public class DuplicateFileCommandHandler : IRequestHandler<DuplicateFileCommand, Result<DuplicateFileResponse>>
{
    private readonly IFileRepository _repository;
    private readonly IFolderRepository _folderRepository;
    private readonly IS3Service _s3Service;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DuplicateFileCommandHandler> _logger;

    public DuplicateFileCommandHandler(
        IFileRepository repository,
        IFolderRepository folderRepository,
        IS3Service s3Service,
        ICurrentUserService currentUserService,
        ILogger<DuplicateFileCommandHandler> logger)
    {
        _repository = repository;
        _folderRepository = folderRepository;
        _s3Service = s3Service;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<DuplicateFileResponse>> Handle(DuplicateFileCommand request, CancellationToken cancellationToken)
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

            var originalFile = await _repository.GetByIdAsync(request.FileId);

            if (originalFile == null || originalFile.IsDeleted)
            {
                _logger.LogWarning("File {FileId} not found", request.FileId);
                return Result.NotFound();
            }

            if (originalFile.TenantId != tenantId)
            {
                _logger.LogWarning("File {FileId} does not belong to tenant {TenantId}", 
                    request.FileId, tenantId);
                return Result<DuplicateFileResponse>.Forbidden();
            }

            // Validate target folder if specified
            var targetFolderId = request.FolderId ?? originalFile.FolderId;
            string? targetFolderPath = null;

            if (targetFolderId.HasValue)
            {
                var folder = await _folderRepository.GetByIdAsync(targetFolderId.Value);
                if (folder == null || folder.IsDeleted)
                {
                    _logger.LogWarning("Target folder {FolderId} not found", targetFolderId);
                    return Result<DuplicateFileResponse>.NotFound("Target folder not found");
                }

                if (folder.TenantId != tenantId)
                {
                    _logger.LogWarning("Target folder {FolderId} does not belong to tenant {TenantId}", 
                        targetFolderId, tenantId);
                    return Result<DuplicateFileResponse>.Forbidden();
                }

                targetFolderPath = string.IsNullOrEmpty(folder.Path) 
                    ? folder.FolderName 
                    : $"{folder.Path}/{folder.FolderName}";
            }

            // Download original file
            var fileStream = await _s3Service.DownloadFileAsync(originalFile.BucketName, originalFile.S3Key, cancellationToken);
            if (fileStream == null)
            {
                _logger.LogError("Failed to download original file from S3: {S3Key}", originalFile.S3Key);
                return Result<DuplicateFileResponse>.Error("Failed to download original file");
            }

            // Generate new file name
            var newFileId = Guid.NewGuid();
            var extension = Path.GetExtension(originalFile.FileName);
            var nameWithoutExt = Path.GetFileNameWithoutExtension(originalFile.FileName);
            var newFileName = string.IsNullOrEmpty(request.Name)
                ? $"{nameWithoutExt} - Copy{extension}"
                : request.Name.EndsWith(extension) ? request.Name : $"{request.Name}{extension}";

            // Build new S3 key
            var newS3Key = _s3Service.BuildS3Key(targetFolderPath, newFileId, newFileName);

            // Upload duplicate
            var uploadSuccess = await _s3Service.UploadFileAsync(
                originalFile.BucketName,
                newS3Key,
                fileStream,
                originalFile.ContentType,
                originalFile.IsPublic,
                cancellationToken);

            if (!uploadSuccess)
            {
                _logger.LogError("Failed to upload duplicate file to S3");
                return Result<DuplicateFileResponse>.Error("Failed to upload duplicate file");
            }

            // Create new file metadata
            var newFile = new FileMetadataModel
            {
                FileId = newFileId,
                FileName = newFileName,
                BucketName = originalFile.BucketName,
                BucketId = originalFile.BucketId,
                S3Key = newS3Key,
                ContentType = originalFile.ContentType,
                Size = originalFile.Size,
                Folder = targetFolderPath,
                FolderId = targetFolderId,
                TenantId = originalFile.TenantId,
                SubCompanyId = originalFile.SubCompanyId,
                UploadedBy = originalFile.UploadedBy,
                IsPublic = originalFile.IsPublic,
                PublicUrl = originalFile.IsPublic 
                    ? await _s3Service.GetPublicUrlAsync(originalFile.BucketName, newS3Key)
                    : null,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repository.CreateAsync(newFile);

            _logger.LogInformation("File {FileId} duplicated successfully as {NewFileId}", 
                request.FileId, newFileId);

            return Result<DuplicateFileResponse>.Success(new DuplicateFileResponse
            {
                Id = newFileId,
                Name = newFileName,
                Url = newFile.PublicUrl ?? string.Empty,
                Size = newFile.Size
            }, "Arquivo duplicado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error duplicating file {FileId}", request.FileId);
            return Result<DuplicateFileResponse>.Error("Failed to duplicate file");
        }
    }
}
