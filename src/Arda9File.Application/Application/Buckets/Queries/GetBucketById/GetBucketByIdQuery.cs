using Ardalis.Result;
using MediatR;

namespace Arda9File.Application.Application.Buckets.Queries.GetBucketById;

public class GetBucketByIdQuery : IRequest<Result<GetBucketByIdResponse>>
{
    public Guid Id { get; set; }
}