using Ardalis.Result;
using Arda9FileApi.Application.Common.Models;
using MediatR;
using Arda9FileApi.Repositories;
using Arda9FileApi.Application.Services;

namespace Arda9FileApi.Application.Features.Files.Queries.GetFiles;

public class GetFilesQueryHandler : IRequestHandler<GetFilesQuery, Result<GetFilesResponse>>
{
    private readonly IFileRepository _fileRepository;
    private readonly IS3Service _s3Service;
    private readonly ILogger<GetFilesQueryHandler> _logger;

    public GetFilesQueryHandler(
        IFileRepository fileRepository,
        IS3Service s3Service,
        ILogger<GetFilesQueryHandler> logger)
    {
        _fileRepository = fileRepository;
        _s3Service = s3Service;
        _logger = logger;
    }

    public async Task<Result<GetFilesResponse>> Handle(GetFilesQuery request, CancellationToken cancellationToken)
    {
        try
        {
            // Get all files for company
            var allFiles = await _fileRepository.GetByCompanyIdAsync(request.TenantId);
            
            // Apply filters
            var filteredFiles = allFiles.Where(f => !f.IsDeleted).AsQueryable();

            // Filter by folder
            if (request.FolderId.HasValue)
            {
                filteredFiles = filteredFiles.Where(f => f.FolderId == request.FolderId.Value);
            }

            // Filter by type
            if (!string.IsNullOrEmpty(request.Type))
            {
                filteredFiles = filteredFiles.Where(f => 
                    GetFileType(f.ContentType, f.FileName) == request.Type.ToLower());
            }

            // Filter by extension
            if (!string.IsNullOrEmpty(request.Extension))
            {
                var ext = request.Extension.StartsWith(".") ? request.Extension : $".{request.Extension}";
                filteredFiles = filteredFiles.Where(f => 
                    Path.GetExtension(f.FileName).Equals(ext, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by search
            if (!string.IsNullOrEmpty(request.Search))
            {
                filteredFiles = filteredFiles.Where(f => 
                    f.FileName.Contains(request.Search, StringComparison.OrdinalIgnoreCase));
            }

            // Filter by size
            if (request.MinSize.HasValue)
            {
                filteredFiles = filteredFiles.Where(f => f.Size >= request.MinSize.Value);
            }

            if (request.MaxSize.HasValue)
            {
                filteredFiles = filteredFiles.Where(f => f.Size <= request.MaxSize.Value);
            }

            // Filter by date range
            if (request.FromDate.HasValue)
            {
                filteredFiles = filteredFiles.Where(f => f.CreatedAt >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                filteredFiles = filteredFiles.Where(f => f.CreatedAt <= request.ToDate.Value);
            }

            // Apply sorting
            var sortBy = request.SortBy?.ToLower() ?? "createdAt";
            var order = request.Order?.ToLower() ?? "desc";

            filteredFiles = sortBy switch
            {
                "name" => order == "asc" 
                    ? filteredFiles.OrderBy(f => f.FileName) 
                    : filteredFiles.OrderByDescending(f => f.FileName),
                "size" => order == "asc" 
                    ? filteredFiles.OrderBy(f => f.Size) 
                    : filteredFiles.OrderByDescending(f => f.Size),
                "type" => order == "asc" 
                    ? filteredFiles.OrderBy(f => f.ContentType) 
                    : filteredFiles.OrderByDescending(f => f.ContentType),
                _ => order == "asc" 
                    ? filteredFiles.OrderBy(f => f.CreatedAt) 
                    : filteredFiles.OrderByDescending(f => f.CreatedAt)
            };

            // Calculate summary
            var filesList = filteredFiles.ToList();
            var summary = new FilesSummary
            {
                TotalSize = filesList.Sum(f => f.Size),
                FilesByType = filesList
                    .GroupBy(f => GetFileType(f.ContentType, f.FileName))
                    .ToDictionary(g => g.Key, g => g.Count())
            };

            // Apply pagination
            var total = filesList.Count;
            var page = request.Page < 1 ? 1 : request.Page;
            var limit = request.Limit < 1 ? 50 : (request.Limit > 100 ? 100 : request.Limit);
            
            var paginatedFiles = filesList
                .Skip((page - 1) * limit)
                .Take(limit)
                .ToList();

            // Map to DTOs
            var fileDtos = paginatedFiles.Select(f => new FileDetailDto
            {
                Id = f.FileId,
                Name = Path.GetFileNameWithoutExtension(f.FileName),
                OriginalName = f.FileName,
                Extension = Path.GetExtension(f.FileName).TrimStart('.'),
                MimeType = f.ContentType,
                Type = GetFileType(f.ContentType, f.FileName),
                Size = f.Size,
                Url = f.PublicUrl ?? _s3Service.GetPublicUrlAsync(f.BucketName, f.S3Key).Result,
                ThumbnailUrl = null,
                PreviewUrl = null,
                FolderId = f.FolderId,
                IsFavorite = false,
                IsShared = false,
                Tags = new List<string>(),
                Description = null,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt,
                CreatedBy = f.UploadedBy.HasValue ? new CreatedByInfo
                {
                    Id = f.UploadedBy.Value,
                    Name = "User",
                    Avatar = null
                } : null
            }).ToList();

            var response = new GetFilesResponse
            {
                Files = new PaginatedResult<FileDetailDto>(fileDtos, page, limit, total),
                Summary = summary
            };

            return Result<GetFilesResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving files for tenant {TenantId}", request.TenantId);
            return Result<GetFilesResponse>.Error("Failed to retrieve files");
        }
    }

    private static string GetFileType(string contentType, string fileName)
    {
        var extension = Path.GetExtension(fileName).ToLower();
        
        if (contentType.StartsWith("image/")) return "image";
        if (contentType.StartsWith("video/")) return "video";
        if (contentType.StartsWith("audio/")) return "audio";
        
        if (contentType == "application/pdf" || 
            contentType.Contains("document") ||
            contentType.Contains("text") ||
            extension is ".doc" or ".docx" or ".xls" or ".xlsx" or ".ppt" or ".pptx" or ".pdf" or ".txt")
            return "document";
        
        if (contentType.Contains("zip") || 
            contentType.Contains("compressed") ||
            extension is ".zip" or ".rar" or ".7z" or ".tar" or ".gz")
            return "archive";
        
        if (extension is ".js" or ".ts" or ".cs" or ".java" or ".py" or ".cpp" or ".html" or ".css" or ".json" or ".xml")
            return "code";
        
        return "other";
    }
}
