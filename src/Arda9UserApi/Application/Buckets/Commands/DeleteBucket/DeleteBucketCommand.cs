using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Buckets.Commands.DeleteBucket;

public class DeleteBucketCommand : IRequest<Result>
{
    public string BucketName { get; set; } = string.Empty;
    public bool ForceDelete { get; set; } = false;
}