using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Features.Buckets.Queries.GetBucketById;

public class GetBucketByIdQuery : IRequest<Result<GetBucketByIdResponse>>
{
    public Guid Id { get; set; }
}