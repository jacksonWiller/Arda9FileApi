using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Features.Buckets.Commands.CreateBucket;

public class CreateBucketResponse
{
    public BucketModel? Bucket { get; set; }
    public string Message { get; set; } = string.Empty;
}