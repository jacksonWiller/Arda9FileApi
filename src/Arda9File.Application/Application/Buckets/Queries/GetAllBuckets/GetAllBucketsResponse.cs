using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Buckets.Queries.GetAllBuckets;

public class GetAllBucketsResponse
{
    public List<BucketModel> Buckets { get; set; } = new();
    public string? LastEvaluatedKey { get; set; }
    public int TotalCount { get; set; }
}