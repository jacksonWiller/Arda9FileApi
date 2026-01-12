using Core.Application.Common.Models;

namespace Arda9File.Application.Application.Files.Queries.GetFiles;

public class GetFilesResponse
{
    public PaginatedResult<FileDetailDto> Files { get; set; } = new();
    public FilesSummary Summary { get; set; } = new();
}

public class FileDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string MimeType { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public long Size { get; set; }
    public string Url { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? PreviewUrl { get; set; }
    public Guid? FolderId { get; set; }
    public bool IsFavorite { get; set; }
    public bool IsShared { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? CreatedBy { get; set; }
}

public class FilesSummary
{
    public long TotalSize { get; set; }
    public Dictionary<string, int> FilesByType { get; set; } = new();
}
