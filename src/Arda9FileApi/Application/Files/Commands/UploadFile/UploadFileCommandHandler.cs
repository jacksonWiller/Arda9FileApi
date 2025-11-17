using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Infrastructure.Repositories;
using Arda9FileApi.Infrastructure.Services;
using MediatR;

namespace Arda9FileApi.Application.Files.Commands.UploadFile;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<UploadFileResponse>>
{
    private readonly IFileRepository _fileRepository;
    private readonly IBucketRepository _bucketRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IS3Service _s3Service;
    private readonly ILogger<UploadFileCommandHandler> _logger;

    public UploadFileCommandHandler(
        IFileRepository fileRepository,
        IBucketRepository bucketRepository,
        IFolderRepository folderRepository,
        IS3Service s3Service,
        ILogger<UploadFileCommandHandler> logger)
    {
        _fileRepository = fileRepository;
        _bucketRepository = bucketRepository;
        _folderRepository = folderRepository;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<Result<UploadFileResponse>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validar se o bucket existe
            var bucket = await _bucketRepository.GetByBucketNameAsync(request.BucketName);
            if (bucket == null)
            {
                _logger.LogWarning("Bucket not found: {BucketName}", request.BucketName);
                return Result.NotFound($"Bucket '{request.BucketName}' not found");
            }

            // Validar se o arquivo foi enviado
            if (request.File == null || request.File.Length == 0)
            {
                _logger.LogWarning("File not provided or empty");
                return Result.Invalid(new ValidationError
                {
                    Identifier = nameof(request.File),
                    ErrorMessage = "File not provided or empty"
                });
            }

            // Se informado FolderId, verificar se a pasta existe
            FolderDto? folder = null;
            if (request.ParentFolder.HasValue)
            {
                folder = bucket.Folders
                    .FirstOrDefault(f => f.Id == request.ParentFolder.Value && !f.IsDeleted);
                if (folder == null || folder.IsDeleted)
                {
                    _logger.LogWarning("Folder {FolderId} not found", request.ParentFolder);
                    return Result.Error();
                }

                // Verificar se a pasta pertence ao mesmo bucket
                if (folder.BucketId != bucket.Id)
                {
                    _logger.LogWarning("Folder {FolderId} does not belong to bucket {BucketId}", 
                        request.ParentFolder.Value, bucket.Id);
                    return Result.Error();
                }

                // Validar CompanyId
                //if (folder.CompanyId != request.CompanyId)
                //{
                //    _logger.LogWarning("Folder {FolderId} does not belong to company {CompanyId}",
                //        request.ParentFolder.Value, request.CompanyId);
                //    return Result.Error();
                //}
            }

            // Construir o path completo do arquivo
            string folderPath = BuildFolderPath(folder);

            // Gerar ID único para o arquivo
            var fileId = Guid.NewGuid();

            // Construir S3 Key usando o serviço
            var s3Key = _s3Service.BuildS3Key(folderPath, request.File.FileName);

            // Fazer upload para S3
            var uploadResult = await _s3Service.UploadFileAsync(
                request.BucketName,
                s3Key,
                request.File.OpenReadStream(),
                request.File.ContentType,
                request.IsPublic,
                cancellationToken);

            if (!uploadResult)
            {
                _logger.LogError("Failed to upload file to S3: {S3Key}", s3Key);
                return Result.Error();
            }

            // Obter URL pública se o arquivo for público
            string? publicUrl = null;
            if (request.IsPublic)
            {
                publicUrl = await _s3Service.GetPublicUrlAsync(request.BucketName, s3Key);
                _logger.LogInformation("Public URL generated for file: {PublicUrl}", publicUrl);
            }

            // Criar metadados do arquivo
            var fileMetadata = new FileMetadataDto
            {
                PK = $"FILE#{fileId}",
                SK = "METADATA",
                FileId = fileId,
                FileName = request.File.FileName,
                BucketName = request.BucketName,
                S3Key = s3Key,
                ContentType = request.File.ContentType,
                Size = request.File.Length,
                Folder = folderPath,
                CompanyId = request.TenantId,
                SubCompanyId = request.SubCompanyId,
                UploadedBy = request.UploadedBy,
                IsPublic = request.IsPublic,
                PublicUrl = publicUrl,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _fileRepository.CreateAsync(fileMetadata);

            _logger.LogInformation(
                "File uploaded successfully: {FileId} - {FileName} (Public: {IsPublic}, Folder: {FolderId})",
                fileId,
                request.File.FileName,
                request.IsPublic,
                request.ParentFolder.HasValue
                );



            return Result.Success(new UploadFileResponse
            {
                FileMetadata = fileMetadata,
                Message = "File uploaded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing file upload: {FileName}", request.File?.FileName);
            return Result.Error();
        }
    }

    private string BuildFolderPath(FolderDto? folder)
    {
        if (folder == null)
        {
            return string.Empty;
        }

        return string.IsNullOrEmpty(folder.Path)
            ? folder.FolderName
            : $"{folder.Path}/{folder.FolderName}";
    }
}