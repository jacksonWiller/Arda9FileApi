using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Buckets.Queries.GetAllBuckets;

public class GetAllBucketsQuery : IRequest<Result<GetAllBucketsResponse>>
{
    public Guid? CompanyId { get; set; }
    public int PageSize { get; set; } = 50;
    public string? LastEvaluatedKey { get; set; }
}