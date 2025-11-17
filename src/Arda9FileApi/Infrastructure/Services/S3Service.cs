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

    public async Task<bool> UploadFileAsync(
        string bucketName, 
        string key, 
        Stream fileStream, 
        string contentType, 
        bool isPublic = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType
                // Removido: CannedACL - não usar ACLs se o bucket não permite
            };

            // Se o arquivo precisa ser público, adicionar metadados
            if (isPublic)
            {
                request.Metadata.Add("x-amz-meta-public", "true");
            }

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);
            
            _logger.LogInformation(
                "Arquivo {Key} enviado para o bucket {BucketName} com visibilidade {Visibility}",
                key, bucketName, isPublic ? "pública" : "privada");
            
            return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload do arquivo {Key} para o bucket {BucketName}", key, bucketName);
            return false;
        }
    }

    public async Task<Stream?> DownloadFileAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _s3Client.GetObjectAsync(bucketName, key, cancellationToken);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Arquivo {Key} não encontrado no bucket {BucketName}", key, bucketName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer download do arquivo {Key} do bucket {BucketName}", key, bucketName);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.DeleteObjectAsync(bucketName, key, cancellationToken);
            _logger.LogInformation("Arquivo {Key} deletado do bucket {BucketName}", key, bucketName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar arquivo {Key} do bucket {BucketName}", key, bucketName);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.GetObjectMetadataAsync(bucketName, key, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
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
                        listResponse.S3Objects.Select(obj => new KeyVersion { Key = obj.Key }));

                    await _s3Client.DeleteObjectsAsync(deleteRequest, cancellationToken);
                }

                listRequest.ContinuationToken = listResponse.NextContinuationToken;
            } while (listResponse.IsTruncated);

            _logger.LogInformation("Todos os objetos deletados do bucket {BucketName}", bucketName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar todos os objetos do bucket {BucketName}", bucketName);
            return false;
        }
    }

    public async Task<bool> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.DeleteBucketAsync(bucketName, cancellationToken);
            _logger.LogInformation("Bucket {BucketName} deletado com sucesso", bucketName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar bucket {BucketName}", bucketName);
            return false;
        }
    }

    public async Task<bool> BucketExistsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.GetBucketLocationAsync(bucketName, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do bucket {BucketName}", bucketName);
            throw;
        }
    }

    public Task<string> GetPublicUrlAsync(string bucketName, string key)
    {
        var region = _s3Client.Config.RegionEndpoint.SystemName;
        var url = $"https://{bucketName}.s3.{region}.amazonaws.com/{key}";
        return Task.FromResult(url);
    }

    public async Task<bool> SetObjectAclAsync(
        string bucketName, 
        string key, 
        bool isPublic, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PutACLRequest
            {
                BucketName = bucketName,
                Key = key,
                CannedACL = isPublic ? S3CannedACL.PublicRead : S3CannedACL.Private
            };

            await _s3Client.PutACLAsync(request, cancellationToken);
            
            _logger.LogInformation(
                "ACL do arquivo {Key} no bucket {BucketName} alterado para {Visibility}",
                key, bucketName, isPublic ? "público" : "privado");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar ACL do arquivo {Key} no bucket {BucketName}", key, bucketName);
            return false;
        }
    }

    public string BuildS3Key(string? folder, string fileName)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        var pathParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(folder))
        {
            pathParts.Add(folder.Trim('/'));
        }

        pathParts.Add($"{sanitizedFileName}");

        return string.Join("/", pathParts);
    }

    public string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }

    public string BuildS3Key(string? folder, Guid fileId, string fileName)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        var pathParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(folder))
        {
            pathParts.Add(folder.Trim('/'));
        }

        // Adiciona o fileId para garantir unicidade
        pathParts.Add($"{fileId}_{sanitizedFileName}");

        return string.Join("/", pathParts);
    }
}