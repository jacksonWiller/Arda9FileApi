using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Features.Buckets.Queries.GetAllBuckets;

public class GetAllBucketsResponse
{
    public List<BucketModel> Buckets { get; set; } = new();
    public string? LastEvaluatedKey { get; set; }
    public int TotalCount { get; set; }
}