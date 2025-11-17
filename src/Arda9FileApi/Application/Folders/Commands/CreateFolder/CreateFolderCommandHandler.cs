using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Infrastructure.Repositories;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Folders.Commands.CreateFolder;

public class CreateFolderCommandHandler : IRequestHandler<CreateFolderCommand, Result<CreateFolderResponse>>
{
    private readonly IFolderRepository _repository;
    private readonly IBucketRepository _bucketRepository;
    private readonly ILogger<CreateFolderCommandHandler> _logger;

    public CreateFolderCommandHandler(
        IFolderRepository repository,
        IBucketRepository bucketRepository,
        ILogger<CreateFolderCommandHandler> logger)
    {
        _repository = repository;
        _bucketRepository = bucketRepository;
        _logger = logger;
    }

    public async Task<Result<CreateFolderResponse>> Handle(CreateFolderCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Verificar se o bucket existe
            var bucket = await _bucketRepository.GetByIdAsync(request.BucketId);
            if (bucket == null)
            {
                _logger.LogWarning("Bucket {BucketId} not found for company {CompanyId}", request.BucketId, request.TenantId);
                return Result<CreateFolderResponse>.Error();
            }

            // Se informado ParentFolderId, verificar se existe
            if (request.ParentFolderId.HasValue)
            {
                var parentFolder = bucket.Folders.FirstOrDefault(x => x.Id == request.ParentFolderId.Value);
                if (parentFolder == null || parentFolder.IsDeleted)
                {
                    _logger.LogWarning("Parent folder {ParentFolderId} not found", request.ParentFolderId);
                    return Result<CreateFolderResponse>.Error();
                }

                // Verificar se a pasta pai pertence ao mesmo bucket
                if (parentFolder.BucketId != request.BucketId)
                {
                    _logger.LogWarning("Parent folder {ParentFolderId} does not belong to bucket {BucketId}", 
                        request.ParentFolderId, request.BucketId);
                    return Result<CreateFolderResponse>.Error();
                }
            }

            // Construir o path completo
            string fullPath = await BuildFullPath(request.ParentFolderId);

            // Verificar se já existe uma pasta com o mesmo nome no mesmo path
            var existingFolder = await _repository.GetByPathAndNameAsync(request.BucketId, fullPath, request.FolderName);
            if (existingFolder != null)
            {
                _logger.LogWarning("Folder {FolderName} already exists in path {Path}", request.FolderName, fullPath);
                return Result<CreateFolderResponse>.Error();
            }

            // Criar a pasta
            var folderId = Guid.NewGuid();
            var folder = new FolderDto
            {
                Id = folderId,
                FolderName = request.FolderName,
                BucketId = request.BucketId,
                Path = fullPath,
                ParentFolderId = request.ParentFolderId,
                CompanyId = request.CompanyId,
                CreatedBy = request.CreatedBy,
                IsPublic = request.IsPublic,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            await _repository.CreateAsync(folder);

            _logger.LogInformation("Folder {FolderId} created successfully in bucket {BucketId}", folder.Id, request.BucketId);

            return Result<CreateFolderResponse>.Success(new CreateFolderResponse
            {
                Folder = folder,
                Message = "Folder created successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder {FolderName}", request.FolderName);
            return Result<CreateFolderResponse>.Error();
        }
    }

    private async Task<string> BuildFullPath(string? requestPath, Guid? parentFolderId)
    {
        if (!string.IsNullOrEmpty(requestPath))
        {
            return requestPath.TrimEnd('/');
        }

        if (parentFolderId.HasValue)
        {
            var parentFolder = await _repository.GetByIdAsync(parentFolderId.Value);
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