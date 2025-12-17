using Ardalis.Result;
using MediatR;
using System.Text.Json.Serialization;

namespace Arda9FileApi.Application.Buckets.Commands.CreateBucket;

public class CreateBucketCommand : IRequest<Result<CreateBucketResponse>>
{
    public string BucketName { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = false;

    [JsonIgnore]
    public Guid TenantId { get; set; } = new Guid();
}