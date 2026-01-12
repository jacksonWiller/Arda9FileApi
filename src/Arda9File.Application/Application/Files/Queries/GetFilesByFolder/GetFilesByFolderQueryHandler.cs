using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;
using Microsoft.Extensions.Logging;
using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Files.Queries.GetFilesByFolder;

public class GetFilesByFolderQueryHandler : IRequestHandler<GetFilesByFolderQuery, Result<List<FileMetadataModel>>>
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

    public async Task<Result<List<FileMetadataModel>>> Handle(GetFilesByFolderQuery request, CancellationToken cancellationToken)
    {
        try
        {
            //await _repository.GetByIdAsync(request.FolderId);
            var folder = await _folderRepository.GetByIdAsync(request.FolderId);
            if (folder == null || folder.IsDeleted)
            {
                _logger.LogWarning("Folder {FolderId} not found", request.FolderId);
                return Result<List<FileMetadataModel>>.NotFound();
            }

            if (folder.CompanyId != request.TenantId)
            {
                _logger.LogWarning("Folder {FolderId} does not belong to tenant {TenantId}", 
                    request.FolderId, request.TenantId);
                return Result<List<FileMetadataModel>>.Forbidden();
            }

            // Usar GSI2 para buscar arquivos por FolderId
            var folderFiles = await _fileRepository.GetByFolderIdAsync(request.FolderId);

            return Result<List<FileMetadataModel>>.Success(folderFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving files for folder {FolderId}", request.FolderId);
            return Result<List<FileMetadataModel>>.Error();
        }
    }
}