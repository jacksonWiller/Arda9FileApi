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
    private readonly IS3Service _s3Service;
    private readonly ILogger<UploadFileCommandHandler> _logger;

    public UploadFileCommandHandler(
        IFileRepository fileRepository,
        IBucketRepository bucketRepository,
        IS3Service s3Service,
        ILogger<UploadFileCommandHandler> logger)
    {
        _fileRepository = fileRepository;
        _bucketRepository = bucketRepository;
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
                _logger.LogWarning("Bucket não encontrado: {BucketName}", request.BucketName);
                return Result.NotFound($"Bucket '{request.BucketName}' não encontrado");
            }

            // Validar se o arquivo foi enviado
            if (request.File == null || request.File.Length == 0)
            {
                _logger.LogWarning("Arquivo não fornecido ou vazio");
                return Result.Invalid(new ValidationError
                {
                    Identifier = nameof(request.File),
                    ErrorMessage = "Arquivo não fornecido ou vazio"
                });
            }

            // Gerar ID único para o arquivo
            var fileId = Guid.NewGuid();
            
            // Construir S3 Key usando o serviço
            var s3Key = _s3Service.BuildS3Key(request.Folder, fileId, request.File.FileName);

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
                _logger.LogError("Falha ao fazer upload do arquivo para S3: {S3Key}", s3Key);
                return Result.Error();
            }

            // Obter URL pública se o arquivo for público
            string? publicUrl = null;
            if (request.IsPublic)
            {
                publicUrl = await _s3Service.GetPublicUrlAsync(request.BucketName, s3Key);
                _logger.LogInformation("URL pública gerada para arquivo: {PublicUrl}", publicUrl);
            }

            // Criar metadados do arquivo
            var fileMetadata = new FileMetadataDto
            {
                FileId = fileId,
                FileName = request.File.FileName,
                BucketName = request.BucketName,
                S3Key = s3Key,
                ContentType = request.File.ContentType,
                Size = request.File.Length,
                UploadedBy = request.UploadedBy,
                IsPublic = request.IsPublic,
                PublicUrl = publicUrl,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _fileRepository.CreateAsync(fileMetadata);

            _logger.LogInformation(
                "Arquivo enviado com sucesso: {FileId} - {FileName} (Público: {IsPublic})", 
                fileId, 
                request.File.FileName, 
                request.IsPublic);

            return Result.Success(new UploadFileResponse
            {
                FileMetadata = fileMetadata,
                Message = "Arquivo enviado com sucesso"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar upload do arquivo: {FileName}", request.File?.FileName);
            return Result.Error();
        }
    }
}