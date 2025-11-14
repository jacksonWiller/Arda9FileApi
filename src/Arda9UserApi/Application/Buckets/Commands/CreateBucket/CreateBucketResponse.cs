using Arda9UserApi.Application.Buckets.DTOs;

namespace Arda9UserApi.Application.Buckets.Commands.CreateBucket;

public class CreateBucketResponse
{
    public BucketDto? Bucket { get; set; }
    public string Message { get; set; } = string.Empty;
}