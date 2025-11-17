using Amazon.S3;
using Amazon.S3.Model;

namespace Arda9FileApi.Infrastructure.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Service> _logger;

    public S3Service(IAmazonS3 s3Client, ILogger<S3Service> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<bool> UploadFileAsync(string bucketName, string key, Stream fileStream, string contentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload do arquivo {Key} para o bucket {BucketName}", key, bucketName);
            throw;
        }
    }

    public async Task<Stream?> DownloadFileAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchKey")
        {
            _logger.LogWarning("Arquivo {Key} não encontrado no bucket {BucketName}", key, bucketName);
            return null;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer download do arquivo {Key} do bucket {BucketName}", key, bucketName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.DeleteObjectAsync(request, cancellationToken);
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar arquivo {Key} do bucket {BucketName}", key, bucketName);
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NotFound")
        {
            return false;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do arquivo {Key} no bucket {BucketName}", key, bucketName);
            throw;
        }
    }

    public async Task<bool> DeleteAllObjectsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
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
                    
                    _logger.LogInformation("Deletados {Count} objetos do bucket {BucketName}", listResponse.S3Objects.Count, bucketName);
                }

                listRequest.ContinuationToken = listResponse.NextContinuationToken;

            } while (listResponse.IsTruncated);

            return true;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar objetos do bucket {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<bool> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteBucketRequest
            {
                BucketName = bucketName
            };

            var response = await _s3Client.DeleteBucketAsync(request, cancellationToken);
            _logger.LogInformation("Bucket {BucketName} deletado com sucesso do S3", bucketName);
            
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            _logger.LogWarning("Bucket {BucketName} não existe no S3", bucketName);
            return true; // Considera sucesso se o bucket já não existe
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "BucketNotEmpty")
        {
            _logger.LogWarning("Tentativa de deletar bucket não vazio: {BucketName}", bucketName);
            return false;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar bucket {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.GetBucketLocationAsync(bucketName, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.ErrorCode == "NoSuchBucket")
        {
            return false;
        }
        catch (AmazonS3Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do bucket {BucketName}", bucketName);
            throw;
        }
    }
}