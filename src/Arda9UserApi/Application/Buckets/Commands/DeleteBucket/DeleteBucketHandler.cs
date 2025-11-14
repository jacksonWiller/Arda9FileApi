using Amazon.S3;
using Amazon.S3.Model;
using Arda9UserApi.Application.Buckets.Commands.CreateBucket;
using Arda9UserApi.Infrastructure.Repositories;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;

namespace Arda9UserApi.Application.Buckets.Commands.DeleteBucket;

public class DeleteBucketHandler : IRequestHandler<DeleteBucketCommand, Result>
{
    private readonly IAmazonS3 _s3Client;
    private readonly IBucketRepository _bucketRepository;
    private readonly IValidator<DeleteBucketCommand> _validator;
    private readonly ILogger<DeleteBucketHandler> _logger;

    public DeleteBucketHandler(
        IAmazonS3 s3Client,
        IBucketRepository bucketRepository,
        IValidator<DeleteBucketCommand> validator,
        ILogger<DeleteBucketHandler> logger)
    {
        _s3Client = s3Client;
        _bucketRepository = bucketRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteBucketCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validação
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Invalid(
                    validationResult.AsErrors()
                );

            }

            // Verificar se bucket existe no DynamoDB
            var bucket = await _bucketRepository.GetByBucketNameAsync(request.BucketName);
            if (bucket == null)
            {
                return Result.NotFound("Bucket não encontrado");
            }

            // Se ForceDelete, deletar todos os objetos primeiro
            if (request.ForceDelete)
            {
                await DeleteAllObjectsAsync(request.BucketName, cancellationToken);
            }

            // Deletar bucket do S3
            await _s3Client.DeleteBucketAsync(request.BucketName, cancellationToken);

            // Deletar registro do DynamoDB
            await _bucketRepository.DeleteAsync(bucket.Id);

            _logger.LogInformation("Bucket {BucketName} deletado com sucesso", request.BucketName);

            return Result.Success();
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            // Se o bucket não existe no S3, apenas remove do DynamoDB
            var bucket = await _bucketRepository.GetByBucketNameAsync(request.BucketName);
            if (bucket != null)
            {
                await _bucketRepository.DeleteAsync(bucket.Id);
            }
            return Result.Success();
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketNotEmpty")
        {
            _logger.LogWarning("Tentativa de deletar bucket não vazio: {BucketName}", request.BucketName);
            return Result.Error();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar bucket: {BucketName}", request.BucketName);
            return Result.Error();
        }
    }

    private async Task DeleteAllObjectsAsync(string bucketName, CancellationToken cancellationToken)
    {
        var listRequest = new ListObjectsV2Request { BucketName = bucketName };
        ListObjectsV2Response listResponse;

        do
        {
            listResponse = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);

            if (listResponse.S3Objects.Count > 0)
            {
                var deleteRequest = new DeleteObjectsRequest { BucketName = bucketName };
                deleteRequest.Objects.AddRange(
                    listResponse.S3Objects.Select(obj => new KeyVersion { Key = obj.Key })
                );

                await _s3Client.DeleteObjectsAsync(deleteRequest, cancellationToken);
            }

            listRequest.ContinuationToken = listResponse.NextContinuationToken;

        } while (listResponse.IsTruncated);
    }
}