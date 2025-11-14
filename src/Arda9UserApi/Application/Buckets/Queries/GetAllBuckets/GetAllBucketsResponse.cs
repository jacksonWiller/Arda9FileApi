using Arda9UserApi.Application.Buckets.DTOs;

namespace Arda9UserApi.Application.Buckets.Queries.GetAllBuckets;

public class GetAllBucketsResponse
{
    public List<BucketDto> Buckets { get; set; } = new();
    public string? LastEvaluatedKey { get; set; }
    public int TotalCount { get; set; }
}