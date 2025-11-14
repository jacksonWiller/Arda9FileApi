using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Application.Buckets.Queries.GetAllBuckets;

public class GetAllBucketsResponse
{
    public List<BucketDto> Buckets { get; set; } = new();
    public string? LastEvaluatedKey { get; set; }
    public int TotalCount { get; set; }
}