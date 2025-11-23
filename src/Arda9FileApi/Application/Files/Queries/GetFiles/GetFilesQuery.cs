using Ardalis.Result;
using MediatR;
using Arda9FileApi.Application.Common.Models;

namespace Arda9FileApi.Application.Files.Queries.GetFiles;

public class GetFilesQuery : IRequest<Result<GetFilesResponse>>
{
    public Guid TenantId { get; set; }
    
    // Pagination
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 50;
    
    // Sorting
    public string? SortBy { get; set; } = "createdAt";
    public string? Order { get; set; } = "desc";
    
    // Filters
    public Guid? FolderId { get; set; }
    public string? Type { get; set; }
    public string? Extension { get; set; }
    public bool? IsFavorite { get; set; }
    public bool? IsShared { get; set; }
    public string? Tags { get; set; }
    public string? Search { get; set; }
    public long? MinSize { get; set; }
    public long? MaxSize { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}
