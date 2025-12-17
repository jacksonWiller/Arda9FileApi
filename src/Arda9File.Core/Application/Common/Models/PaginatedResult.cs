namespace Core.Application.Common.Models;

public class PaginatedResult<T>
{
    public List<T> Items { get; set; } = new();
    public PaginationMetadata Pagination { get; set; } = new();

    public PaginatedResult()
    {
    }

    public PaginatedResult(List<T> items, int page, int limit, int total)
    {
        Items = items;
        Pagination = new PaginationMetadata
        {
            Page = page,
            Limit = limit,
            Total = total,
            TotalPages = (int)Math.Ceiling(total / (double)limit),
            HasNext = page * limit < total,
            HasPrev = page > 1
        };
    }
}

public class PaginationMetadata
{
    public int Page { get; set; }
    public int Limit { get; set; }
    public int Total { get; set; }
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrev { get; set; }
}
