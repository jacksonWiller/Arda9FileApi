using Amazon.S3;
using Amazon.S3.Model;
using Arda9File.Domain.Repositories;
using Arda9FileApi.Application.Buckets.Commands.CreateBucket;
using Arda9FileApi.Repositories;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Arda9FileApi.Application.Buckets.Commands.DeleteBucket;

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
}