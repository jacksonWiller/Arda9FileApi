using Ardalis.Result;
using MediatR;
using Arda9FileApi.Repositories;

namespace Arda9FileApi.Application.Buckets.Queries.GetBucketById;

public class GetBucketByIdHandler : IRequestHandler<GetBucketByIdQuery, Result<GetBucketByIdResponse>>
{
    private readonly IBucketRepository _bucketRepository;
    private readonly ILogger<GetBucketByIdHandler> _logger;

    public GetBucketByIdHandler(
        IBucketRepository bucketRepository,
        ILogger<GetBucketByIdHandler> logger)
    {
        _bucketRepository = bucketRepository;
        _logger = logger;
    }

    public async Task<Result<GetBucketByIdResponse>> Handle(GetBucketByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var bucket = await _bucketRepository.GetByIdAsync(request.Id);

            if (bucket == null)
            {
                return Result<GetBucketByIdResponse>.NotFound("Bucket não encontrado");
            }

            return Result<GetBucketByIdResponse>.Success(new GetBucketByIdResponse
            {
                Bucket = bucket
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar bucket: {Id}", request.Id);
            return Result<GetBucketByIdResponse>.Error();
        }
    }
}