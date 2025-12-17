using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;
using Arda9FileApi.Services;
using Microsoft.Extensions.Logging;

namespace Arda9FileApi.Application.Files.Commands.UpdateFile;

public class UpdateFileCommandHandler : IRequestHandler<UpdateFileCommand, Result<UpdateFileResponse>>
{
    private readonly IFileRepository _repository;
    private readonly IFolderRepository _folderRepository;
    private readonly IS3Service _s3Service;
    private readonly ILogger<UpdateFileCommandHandler> _logger;

    public UpdateFileCommandHandler(
        IFileRepository repository,
        IFolderRepository folderRepository,
        IS3Service s3Service,
        ILogger<UpdateFileCommandHandler> logger)
    {
        _repository = repository;
        _folderRepository = folderRepository;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<Result<UpdateFileResponse>> Handle(UpdateFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var file = await _repository.GetByIdAsync(request.FileId);
            if (file == null || file.IsDeleted)
            {
                _logger.LogWarning("File {FileId} not found", request.FileId);
                return Result<UpdateFileResponse>.NotFound();
            }

            if (file.CompanyId != request.TenantId)
            {
                _logger.LogWarning("File {FileId} does not belong to tenant {TenantId}", 
                    request.FileId, request.TenantId);
                return Result<UpdateFileResponse>.Forbidden();
            }

            var needsS3Update = false;
            var oldS3Key = file.S3Key;

            // Atualizar nome do arquivo se fornecido
            if (!string.IsNullOrEmpty(request.FileName))
            {
                var sanitizedFileName = _s3Service.SanitizeFileName(request.FileName);
                file.FileName = sanitizedFileName;
                needsS3Update = true;
            }

            // Atualizar pasta se fornecido
            if (request.FolderId.HasValue)
            {
                var folder = await _folderRepository.GetByIdAsync(request.FolderId.Value);
                if (folder == null || folder.IsDeleted)
                {
                    _logger.LogWarning("Folder {FolderId} not found", request.FolderId);
                    return Result<UpdateFileResponse>.Error();
                }

                if (folder.CompanyId != request.TenantId)
                {
                    _logger.LogWarning("Folder {FolderId} does not belong to tenant {TenantId}", 
                        request.FolderId, request.TenantId);
                    return Result<UpdateFileResponse>.Forbidden();
                }

                var newFolderPath = string.IsNullOrEmpty(folder.Path) 
                    ? folder.FolderName 
                    : $"{folder.Path}/{folder.FolderName}";

                file.Folder = newFolderPath;
                needsS3Update = true;
            }

            // Atualizar visibilidade se fornecido
            if (request.IsPublic.HasValue && file.IsPublic != request.IsPublic.Value)
            {
                file.IsPublic = request.IsPublic.Value;
                
                // Atualizar ACL no S3
                await _s3Service.SetObjectAclAsync(file.BucketName, file.S3Key, file.IsPublic, cancellationToken);

                if (file.IsPublic)
                {
                    file.PublicUrl = await _s3Service.GetPublicUrlAsync(file.BucketName, file.S3Key);
                }
                else
                {
                    file.PublicUrl = null;
                }
            }

            // Se mudou nome ou pasta, mover arquivo no S3
            if (needsS3Update)
            {
                var newS3Key = _s3Service.BuildS3Key(file.Folder, file.FileId, file.FileName);
                
                if (newS3Key != oldS3Key)
                {
                    // Download do arquivo antigo
                    var fileStream = await _s3Service.DownloadFileAsync(file.BucketName, oldS3Key, cancellationToken);
                    if (fileStream == null)
                    {
                        _logger.LogError("Failed to download file from S3: {S3Key}", oldS3Key);
                        return Result<UpdateFileResponse>.Error();
                    }

                    // Upload com novo nome/caminho
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
                        return Result<UpdateFileResponse>.Error();
                    }

                    // Deletar arquivo antigo
                    await _s3Service.DeleteFileAsync(file.BucketName, oldS3Key, cancellationToken);

                    file.S3Key = newS3Key;

                    if (file.IsPublic)
                    {
                        file.PublicUrl = await _s3Service.GetPublicUrlAsync(file.BucketName, newS3Key);
                    }
                }
            }

            file.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(file);

            _logger.LogInformation("File {FileId} updated successfully", file.FileId);

            return Result<UpdateFileResponse>.Success(new UpdateFileResponse
            {
                File = file,
                Message = "File updated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating file {FileId}", request.FileId);
            return Result<UpdateFileResponse>.Error();
        }
    }
}