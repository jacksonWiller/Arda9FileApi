using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Buckets.Commands.DeleteBucketById;

public class DeleteBucketByIdCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public bool ForceDelete { get; set; } = false;
}