using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Buckets.Queries.GetBucketById;

public class GetBucketByIdResponse
{
    public BucketModel? Bucket { get; set; }
}