namespace Core.Application.Common.Models;

public class PaginationQuery
{
    public int Page { get; set; } = 1;
    public int Limit { get; set; } = 50;

    public int Skip => (Page - 1) * Limit;

    public void Validate()
    {
        if (Page < 1) Page = 1;
        if (Limit < 1) Limit = 1;
        if (Limit > 100) Limit = 100;
    }
}
