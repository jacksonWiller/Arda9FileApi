using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;
using Microsoft.Extensions.Logging;
using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Folders.Queries.GetFolders;

public class GetFoldersQueryHandler : IRequestHandler<GetFoldersQuery, Result<GetFoldersResponse>>
{
    private readonly IFolderRepository _folderRepository;
    private readonly IFileRepository _fileRepository;
    private readonly ILogger<GetFoldersQueryHandler> _logger;

    public GetFoldersQueryHandler(
        IFolderRepository folderRepository,
        IFileRepository fileRepository,
        ILogger<GetFoldersQueryHandler> logger)
    {
        _folderRepository = folderRepository;
        _fileRepository = fileRepository;
        _logger = logger;
    }

    public async Task<Result<GetFoldersResponse>> Handle(GetFoldersQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Validate depth
            var depth = request.Depth < 1 ? 1 : request.Depth > 5 ? 5 : request.Depth;

            List<FolderDetailDto> folders;

            if (request.ParentId.HasValue)
            {
                // Get folders by parent
                var parentFolders = await _folderRepository.GetByParentFolderIdAsync(request.ParentId.Value);
                folders = await BuildFolderTree(
                    parentFolders.Where(f => !f.IsDeleted && f.TenantId == request.TenantId).ToList(),
                    depth,
                    request.IncludeEmpty);
            }
            else
            {
                // Get root folders (folders with no parent)
                var allFolders = await _folderRepository.GetByTenantIdAsync(request.TenantId);
                var rootFolders = allFolders.Where(f => !f.IsDeleted && !f.ParentFolderId.HasValue).ToList();
                folders = await BuildFolderTree(rootFolders, depth, request.IncludeEmpty);
            }

            var response = new GetFoldersResponse
            {
                Folders = folders
            };

            return Result<GetFoldersResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving folders for tenant {TenantId}", request.TenantId);
            return Result<GetFoldersResponse>.Error("Failed to retrieve folders");
        }
    }

    private async Task<List<FolderDetailDto>> BuildFolderTree(
        List<FolderModel> folders,
        int depth,
        bool includeEmpty)
    {
        var folderDtos = new List<FolderDetailDto>();

        foreach (var folder in folders)
        {
            var files = await _fileRepository.GetByFolderIdAsync(folder.Id);
            var fileCount = files.Count(f => !f.IsDeleted);
            
            var childFolders = await _folderRepository.GetByParentFolderIdAsync(folder.Id);
            var validChildren = childFolders.Where(f => !f.IsDeleted).ToList();
            var folderCount = validChildren.Count;

            // Skip empty folders if requested
            if (!includeEmpty && fileCount == 0 && folderCount == 0)
                continue;

            var totalSize = files.Where(f => !f.IsDeleted).Sum(f => f.Size);

            var folderDto = new FolderDetailDto
            {
                Id = folder.Id,
                Name = folder.FolderName,
                ParentId = folder.ParentFolderId,
                Path = BuildPath(folder.Path, folder.FolderName),
                Depth = CalculateDepth(folder.Path),
                FileCount = fileCount,
                FolderCount = folderCount,
                TotalSize = totalSize,
                Color = null,
                Icon = null,
                IsShared = false,
                Permissions = new FolderPermissions
                {
                    CanView = true,
                    CanEdit = true,
                    CanDelete = true,
                    CanShare = true
                },
                Children = new List<FolderDetailDto>(),
                CreatedAt = folder.CreatedAt,
                UpdatedAt = folder.UpdatedAt,
                CreatedBy = folder.CreatedBy.HasValue ? new FolderCreatedBy
                {
                    Id = folder.CreatedBy.Value,
                    Name = "User"
                } : null
            };

            // Recursively build children if depth allows
            if (depth > 1 && validChildren.Any())
            {
                folderDto.Children = await BuildFolderTree(validChildren, depth - 1, includeEmpty);
            }

            folderDtos.Add(folderDto);
        }

        return folderDtos;
    }

    private List<string> BuildPath(string? path, string folderName)
    {
        var pathList = new List<string>();
        
        if (!string.IsNullOrEmpty(path))
        {
            pathList.AddRange(path.Split('/', StringSplitOptions.RemoveEmptyEntries));
        }
        
        pathList.Add(folderName);
        
        return pathList;
    }

    private int CalculateDepth(string? path)
    {
        if (string.IsNullOrEmpty(path))
            return 0;
        
        return path.Split('/', StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
