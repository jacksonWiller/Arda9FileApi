using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Buckets.Commands.CreateBucket;

public class CreateBucketResponse
{
    public BucketModel? Bucket { get; set; }
    public string Message { get; set; } = string.Empty;
}