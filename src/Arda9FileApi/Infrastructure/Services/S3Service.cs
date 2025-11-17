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
            
            _logger.LogInformation("Arquivo enviado com sucesso para S3: {BucketName}/{Key}", bucketName, key);
            
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload do arquivo para S3: {BucketName}/{Key}", bucketName, key);
            return false;
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
            
            _logger.LogInformation("Arquivo baixado com sucesso do S3: {BucketName}/{Key}", bucketName, key);
            
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Arquivo não encontrado no S3: {BucketName}/{Key}", bucketName, key);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao baixar arquivo do S3: {BucketName}/{Key}", bucketName, key);
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
            
            _logger.LogInformation("Arquivo deletado com sucesso do S3: {BucketName}/{Key}", bucketName, key);
            
            return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar arquivo do S3: {BucketName}/{Key}", bucketName, key);
            return false;
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
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do arquivo no S3: {BucketName}/{Key}", bucketName, key);
            throw;
        }
    }
}