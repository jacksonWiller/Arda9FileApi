using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Buckets.Queries.GetBucketById;

public class GetBucketByIdQuery : IRequest<Result<GetBucketByIdResponse>>
{
    public Guid Id { get; set; }
}