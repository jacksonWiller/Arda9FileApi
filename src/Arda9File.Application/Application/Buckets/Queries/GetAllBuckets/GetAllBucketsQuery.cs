using Ardalis.Result;
using MediatR;

namespace Arda9File.Application.Application.Buckets.Queries.GetAllBuckets;

public class GetAllBucketsQuery : IRequest<Result<GetAllBucketsResponse>>
{
    public int PageSize { get; set; } = 50;
    public string? LastEvaluatedKey { get; set; }
}