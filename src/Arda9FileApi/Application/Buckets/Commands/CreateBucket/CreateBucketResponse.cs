using Arda9FileApi.Application.Buckets.DTOs;

namespace Arda9FileApi.Application.Buckets.Commands.CreateBucket;

public class CreateBucketResponse
{
    public BucketDto? Bucket { get; set; }
    public string Message { get; set; } = string.Empty;
}