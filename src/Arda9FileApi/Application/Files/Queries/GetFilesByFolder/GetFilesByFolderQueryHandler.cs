using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Infrastructure.Repositories;
using MediatR;

namespace Arda9FileApi.Application.Files.Queries.GetFilesByFolder;

public class GetFilesByFolderQueryHandler : IRequestHandler<GetFilesByFolderQuery, Result<List<FileMetadataDto>>>
{
    private readonly IFileRepository _fileRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly ILogger<GetFilesByFolderQueryHandler> _logger;

    public GetFilesByFolderQueryHandler(
        IFileRepository fileRepository,
        IFolderRepository folderRepository,
        ILogger<GetFilesByFolderQueryHandler> logger)
    {
        _fileRepository = fileRepository;
        _folderRepository = folderRepository;
        _logger = logger;
    }

    public async Task<Result<List<FileMetadataDto>>> Handle(GetFilesByFolderQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var folder = await _folderRepository.GetByIdAsync(request.FolderId);
            if (folder == null || folder.IsDeleted)
            {
                _logger.LogWarning("Folder {FolderId} not found", request.FolderId);
                return Result<List<FileMetadataDto>>.NotFound();
            }

            if (folder.CompanyId != request.TenantId)
            {
                _logger.LogWarning("Folder {FolderId} does not belong to tenant {TenantId}", 
                    request.FolderId, request.TenantId);
                return Result<List<FileMetadataDto>>.Forbidden();
            }

            var folderPath = string.IsNullOrEmpty(folder.Path) 
                ? folder.FolderName 
                : $"{folder.Path}/{folder.FolderName}";

            var allFiles = await _fileRepository.GetByCompanyIdAsync(request.TenantId);
            
            var folderFiles = allFiles
                .Where(f => !f.IsDeleted && f.Folder == folderPath)
                .ToList();

            return Result<List<FileMetadataDto>>.Success(folderFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving files for folder {FolderId}", request.FolderId);
            return Result<List<FileMetadataDto>>.Error();
        }
    }
}