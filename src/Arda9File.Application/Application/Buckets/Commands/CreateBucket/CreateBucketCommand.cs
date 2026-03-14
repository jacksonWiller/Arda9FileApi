using Ardalis.Result;
using MediatR;
using System.Text.Json.Serialization;

namespace Arda9File.Application.Application.Buckets.Commands.CreateBucket;

public class CreateBucketCommand : IRequest<Result<CreateBucketResponse>>
{
    public string BucketName { get; set; } = string.Empty;

    public bool IsPublic { get; set; } = true;
}