using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using Arda9FileApi.Infrastructure.Repositories;
using MediatR;

namespace Arda9FileApi.Application.Folders.Queries.GetFolderById;

public class GetFolderByIdQueryHandler : IRequestHandler<GetFolderByIdQuery, Result<FolderDto>>
{
    private readonly IFolderRepository _repository;
    private readonly ILogger<GetFolderByIdQueryHandler> _logger;

    public GetFolderByIdQueryHandler(
        IFolderRepository repository,
        ILogger<GetFolderByIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<FolderDto>> Handle(GetFolderByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var folder = await _repository.GetByIdAsync(request.FolderId);
            
            if (folder == null || folder.IsDeleted)
            {
                _logger.LogWarning("Folder {FolderId} not found", request.FolderId);
                return Result<FolderDto>.NotFound();
            }

            if (folder.CompanyId != request.TenantId)
            {
                _logger.LogWarning("Folder {FolderId} does not belong to tenant {TenantId}", 
                    request.FolderId, request.TenantId);
                return Result<FolderDto>.Forbidden();
            }

            return Result<FolderDto>.Success(folder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folder {FolderId}", request.FolderId);
            return Result<FolderDto>.Error();
        }
    }
}