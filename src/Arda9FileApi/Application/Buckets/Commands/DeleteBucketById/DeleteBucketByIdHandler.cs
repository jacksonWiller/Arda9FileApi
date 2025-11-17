using Amazon.S3;
using Amazon.S3.Model;
using Arda9FileApi.Infrastructure.Repositories;
using Arda9FileApi.Infrastructure.Services;
using Ardalis.Result;
using Ardalis.Result.FluentValidation;
using FluentValidation;
using MediatR;

namespace Arda9FileApi.Application.Buckets.Commands.DeleteBucketById;

public class DeleteBucketByIdHandler : IRequestHandler<DeleteBucketByIdCommand, Result>
{
    private readonly IS3Service _s3Service;
    private readonly IBucketRepository _bucketRepository;
    private readonly IValidator<DeleteBucketByIdCommand> _validator;
    private readonly ILogger<DeleteBucketByIdHandler> _logger;

    public DeleteBucketByIdHandler(
        IS3Service s3Service,
        IBucketRepository bucketRepository,
        IValidator<DeleteBucketByIdCommand> validator,
        ILogger<DeleteBucketByIdHandler> logger)
    {
        _s3Service = s3Service;
        _bucketRepository = bucketRepository;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result> Handle(DeleteBucketByIdCommand request, CancellationToken cancellationToken)
    {
        try
        {
            // Validação
            var validationResult = await _validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                return Result.Invalid(validationResult.AsErrors());
            }

            // Verificar se bucket existe no DynamoDB
            var bucket = await _bucketRepository.GetByIdAsync(request.Id);
            if (bucket == null)
            {
                return Result.NotFound("Bucket não encontrado");
            }

            // Verificar se já está deletado
            if (bucket.Status == "deleted")
            {
                return Result.Error();
            }

            // Verificar se bucket existe no S3
            var bucketExists = await _s3Service.BucketExistsAsync(bucket.BucketName, cancellationToken);

            if (bucketExists)
            {
                // Se ForceDelete está ativo, deletar todos os objetos primeiro
                if (request.ForceDelete)
                {
                    _logger.LogInformation("Deletando todos os objetos do bucket {BucketName} (ID: {Id})", bucket.BucketName, bucket.Id);
                    await _s3Service.DeleteAllObjectsAsync(bucket.BucketName, cancellationToken);
                }

                // Deletar bucket do S3
                var s3DeleteSuccess = await _s3Service.DeleteBucketAsync(bucket.BucketName, cancellationToken);
                
                if (!s3DeleteSuccess)
                {
                    _logger.LogWarning("Não foi possível deletar o bucket {BucketName} (ID: {Id}) do S3. Bucket pode conter objetos.", bucket.BucketName, bucket.Id);
                    return Result.Error();
                }
            }
            else
            {
                _logger.LogWarning("Bucket {BucketName} (ID: {Id}) não existe no S3, removendo apenas do DynamoDB", bucket.BucketName, bucket.Id);
            }

            // Deletar registro do DynamoDB (soft delete)
            await _bucketRepository.DeleteAsync(bucket.Id);

            _logger.LogInformation("Bucket {BucketName} (ID: {Id}) deletado com sucesso", bucket.BucketName, bucket.Id);

            return Result.Success();
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketNotEmpty")
        {
            _logger.LogWarning("Tentativa de deletar bucket não vazio (ID: {Id})", request.Id);
            return Result.Error();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar bucket (ID: {Id})", request.Id);
            return Result.Error();
        }
    }
}