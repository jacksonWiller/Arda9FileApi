using Amazon.S3;
using Arda9FileApi.Application.Features.Buckets.Commands.CreateBucket;
using Arda9FileApi.Application.Services;
using Arda9FileApi.Models;
using Arda9FileApi.Repositories;
using Ardalis.Result;
using FluentValidation;
using MediatR;

namespace Arda9FileApi.Application.Features.Files.Commands.UploadFile;

public class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, Result<UploadFileResponse>>
{
    private readonly IFileRepository _fileRepository;
    private readonly IBucketRepository _bucketRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IS3Service _s3Service;
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<UploadFileCommandHandler> _logger;
    private readonly IValidator<CreateBucketCommand> _validator;
    private readonly IAuthService _authService;

    public UploadFileCommandHandler(
        IFileRepository fileRepository,
        IBucketRepository bucketRepository,
        IFolderRepository folderRepository,
        IS3Service s3Service,
        IAmazonS3 s3Client,
        ILogger<UploadFileCommandHandler> logger,
        IValidator<CreateBucketCommand> validator,
        IAuthService authService
        )
    {
        _fileRepository = fileRepository;
        _bucketRepository = bucketRepository;
        _folderRepository = folderRepository;
        _s3Service = s3Service;
        _s3Client = s3Client;
        _validator = validator;
        _logger = logger;
        _authService = authService;
    }

    public async Task<Result<UploadFileResponse>> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Obter ID do usuário autenticado
            var userIdResult = _authService.GetCurrentUserId();
            if (!userIdResult.IsSuccess)
            {
                return Result.Unauthorized();
            }

            // Verificar se o bucket existe
            var bucket = await _bucketRepository.GetByIdAsync(request.BucketId);
            if (bucket == null)
            {
                _logger.LogWarning("Bucket {BucketId} not found for company {CompanyId}", request.BucketId, request.TenantId);
                return Result<UploadFileResponse>.Error();
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

            // Se informado ParentFolderId, verificar se existe
            if (request.FolderId.HasValue)
            {
                var parentFolder = await _folderRepository.GetByIdAsync(request.FolderId.Value);
                if (parentFolder == null || parentFolder.IsDeleted)
                {
                    _logger.LogWarning("Parent folder {ParentFolderId} not found", request.FolderId);
                    return Result.Error();
                }

                // Verificar se a pasta pai pertence ao mesmo bucket
                if (parentFolder.BucketId != request.BucketId)
                {
                    _logger.LogWarning("Parent folder {ParentFolderId} does not belong to bucket {BucketId}",
                        request.FolderId, request.BucketId);
                    return Result.Error();
                }
            }

            // Construir o path completo do arquivo
            string folderPath = await BuildFullPath(request.FolderId);

            // Gerar ID único para o arquivo
            var fileId = Guid.NewGuid();

            // Construir S3 Key usando o serviço
            var s3Key = _s3Service.BuildS3Key(folderPath, request.File.FileName);

            // Fazer upload para S3
            var uploadResult = await _s3Service.UploadFileAsync(
                bucket.BucketName,
                s3Key,
                request.File.OpenReadStream(),
                request.File.ContentType,
                request.IsPublic,
                cancellationToken
            );

            if (!uploadResult)
            {
                _logger.LogError("Failed to upload file to S3: {S3Key}", s3Key);
                return Result.Error();
            }

            // Obter URL pública se o arquivo for público
            string? publicUrl = null;
            if (request.IsPublic)
            {
                publicUrl = await _s3Service.GetPublicUrlAsync(bucket.BucketName, s3Key);
                _logger.LogInformation("Public URL generated for file: {PublicUrl}", publicUrl);
            }

            // Criar metadados do arquivo
            var fileMetadata = new FileMetadataModel
            {
                PK = $"FILE#{fileId}",
                SK = "METADATA",
                FileId = fileId,
                FileName = request.File.FileName,
                BucketName = bucket.BucketName,
                S3Key = s3Key,
                ContentType = request.File.ContentType,
                Size = request.File.Length,
                FolderId = request.FolderId,
                Folder = folderPath,
                CompanyId = request.TenantId,
                UploadedBy = userIdResult,
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
                request.FolderId.ToString() ?? "Root"
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

    private async Task<string> BuildFullPath(Guid? parentFolderId)
    {
        if (parentFolderId.HasValue)
        {
            var parentFolder = await _folderRepository.GetByIdAsync(parentFolderId.Value);
            if (parentFolder != null)
            {
                return string.IsNullOrEmpty(parentFolder.Path)
                    ? parentFolder.FolderName
                    : $"{parentFolder.Path}/{parentFolder.FolderName}";
            }
        }

        return string.Empty;
    }
}