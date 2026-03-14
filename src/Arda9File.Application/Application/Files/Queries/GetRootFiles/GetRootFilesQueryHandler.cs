using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;
using Arda9File.Domain.Repositories;
using Microsoft.Extensions.Logging;
using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Files.Queries.GetRootFiles;

public class GetRootFilesQueryHandler : IRequestHandler<GetRootFilesQuery, Result<List<FileMetadataModel>>>
{
    private readonly IFileRepository _fileRepository;
    private readonly IBucketRepository _bucketRepository;
    private readonly ILogger<GetRootFilesQueryHandler> _logger;

    public GetRootFilesQueryHandler(
        IFileRepository fileRepository,
        IBucketRepository bucketRepository,
        ILogger<GetRootFilesQueryHandler> logger)
    {
        _fileRepository = fileRepository;
        _bucketRepository = bucketRepository;
        _logger = logger;
    }

    public async Task<Result<List<FileMetadataModel>>> Handle(GetRootFilesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validar bucket
            var bucket = await _bucketRepository.GetByIdAsync(request.BucketId);
            if (bucket == null)
            {
                _logger.LogWarning("Bucket {BucketId} not found", request.BucketId);
                return Result<List<FileMetadataModel>>.NotFound("Bucket não encontrado");
            }

            if (bucket.TenantId != request.TenantId)
            {
                _logger.LogWarning("Bucket {BucketId} does not belong to tenant {TenantId}", 
                    request.BucketId, request.TenantId);
                return Result<List<FileMetadataModel>>.Forbidden();
            }

            // Buscar todos os arquivos do bucket
            var allFiles = await _fileRepository.GetByBucketIdAsync(request.BucketId);

            // Filtrar apenas arquivos da pasta raiz (sem FolderId)
            var rootFiles = allFiles
                .Where(f => !f.IsDeleted && !f.FolderId.HasValue)
                .OrderByDescending(f => f.CreatedAt)
                .ToList();

            _logger.LogInformation("Found {Count} root files for bucket {BucketId}", 
                rootFiles.Count, request.BucketId);

            return Result<List<FileMetadataModel>>.Success(rootFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving root files for bucket {BucketId}", request.BucketId);
            return Result<List<FileMetadataModel>>.Error("Failed to retrieve root files");
        }
    }
}
