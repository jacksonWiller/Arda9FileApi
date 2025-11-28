using Amazon.S3;
using Amazon.S3.Model;
using System.Text.Json;

namespace Arda9FileApi.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3Service> _logger;

    public S3Service(
        IAmazonS3 s3Client,
        ILogger<S3Service> logger
        )
    {
        _s3Client = s3Client;
        _logger = logger;
    }

    public async Task<bool> CreateBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true
            };

            await _s3Client.PutBucketAsync(putBucketRequest, cancellationToken);
            _logger.LogInformation("Bucket privado {BucketName} criado com sucesso", bucketName);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar bucket: {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<bool> CreatePublicBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            // 1. Criar o bucket
            var putBucketRequest = new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true
            };

            await _s3Client.PutBucketAsync(putBucketRequest, cancellationToken);
            _logger.LogInformation("Bucket {BucketName} criado com sucesso", bucketName);

            // 2. Remover bloqueio de acesso público
            await SetBucketPublicAccessAsync(bucketName, cancellationToken);

            // 3. Configurar política de bucket para leitura pública
            await SetBucketPolicyForPublicReadAsync(bucketName, cancellationToken);

            // 4. Desabilitar ACLs (usa apenas políticas de bucket)
            await DisableBucketOwnershipControlsAsync(bucketName, cancellationToken);

            _logger.LogInformation("Bucket {BucketName} configurado como público", bucketName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar bucket público: {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<bool> SetBucketPublicAccessAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Remover bloqueio de acesso público do bucket
            var publicAccessBlockRequest = new Amazon.S3.Model.DeletePublicAccessBlockRequest
            {
                BucketName = bucketName
            };

            await _s3Client.DeletePublicAccessBlockAsync(publicAccessBlockRequest, cancellationToken);
            
            _logger.LogInformation("Bloqueio de acesso público removido do bucket {BucketName}", bucketName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar acesso público para bucket: {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<bool> SetBucketPolicyForPublicReadAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            // Política que permite leitura pública de todos os objetos
            var bucketPolicy = new
            {
                Version = "2012-10-17",
                Statement = new[]
                {
                    new
                    {
                        Sid = "PublicReadGetObject",
                        Effect = "Allow",
                        Principal = "*",
                        Action = "s3:GetObject",
                        Resource = $"arn:aws:s3:::{bucketName}/*"
                    }
                }
            };

            var policyJson = JsonSerializer.Serialize(bucketPolicy);

            var putBucketPolicyRequest = new Amazon.S3.Model.PutBucketPolicyRequest
            {
                BucketName = bucketName,
                Policy = policyJson
            };

            await _s3Client.PutBucketPolicyAsync(putBucketPolicyRequest, cancellationToken);
            
            _logger.LogInformation("Política de leitura pública aplicada ao bucket {BucketName}", bucketName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir política pública para bucket: {BucketName}", bucketName);
            throw;
        }
    }

    private async Task<bool> DisableBucketOwnershipControlsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            var ownershipRequest = new PutBucketOwnershipControlsRequest
            {
                BucketName = bucketName,
                OwnershipControls = new OwnershipControls
                {
                    Rules = new List<OwnershipControlsRule>
                    {
                        new OwnershipControlsRule
                        {
                            ObjectOwnership = ObjectOwnership.BucketOwnerPreferred
                        }
                    }
                }
            };

            await _s3Client.PutBucketOwnershipControlsAsync(ownershipRequest, cancellationToken);
            _logger.LogInformation("Controles de propriedade configurados para bucket {BucketName}", bucketName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar controles de propriedade: {BucketName}", bucketName);
            throw;
        }
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
            var putRequest = new PutObjectRequest
            {
                BucketName = bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                // Se o bucket já é público, não precisa de ACL individual
                // Mas podemos adicionar para garantir
                CannedACL = isPublic ? S3CannedACL.PublicRead : S3CannedACL.Private
            };

            await _s3Client.PutObjectAsync(putRequest, cancellationToken);
            _logger.LogInformation("Arquivo {Key} enviado para bucket {BucketName} com acesso {Access}", 
                key, bucketName, isPublic ? "público" : "privado");
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer upload do arquivo: {Key}", key);
            throw;
        }
    }

    public async Task<bool> SetObjectAclAsync(string bucketName, string key, bool isPublic, CancellationToken cancellationToken = default)
    {
        try
        {
            var aclRequest = new PutACLRequest
            {
                BucketName = bucketName,
                Key = key,
                CannedACL = isPublic ? S3CannedACL.PublicRead : S3CannedACL.Private
            };

            await _s3Client.PutACLAsync(aclRequest, cancellationToken);
            _logger.LogInformation("ACL do objeto {Key} atualizado para {Access}", key, isPublic ? "público" : "privado");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao definir ACL do objeto: {Key}", key);
            throw;
        }
    }

    public async Task<Stream?> DownloadFileAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var getRequest = new GetObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(getRequest, cancellationToken);
            return response.ResponseStream;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogWarning("Arquivo não encontrado: {Key} no bucket {BucketName}", key, bucketName);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao fazer download do arquivo: {Key}", key);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string bucketName, string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var deleteRequest = new DeleteObjectRequest
            {
                BucketName = bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(deleteRequest, cancellationToken);
            _logger.LogInformation("Arquivo {Key} deletado do bucket {BucketName}", key, bucketName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar arquivo: {Key}", key);
            throw;
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
    }

    public async Task<bool> DeleteAllObjectsAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            var listRequest = new ListObjectsV2Request
            {
                BucketName = bucketName
            };

            ListObjectsV2Response listResponse;
            do
            {
                listResponse = await _s3Client.ListObjectsV2Async(listRequest, cancellationToken);

                if (listResponse.S3Objects.Count > 0)
                {
                    var deleteRequest = new DeleteObjectsRequest
                    {
                        BucketName = bucketName,
                        Objects = listResponse.S3Objects.Select(o => new KeyVersion { Key = o.Key }).ToList()
                    };

                    await _s3Client.DeleteObjectsAsync(deleteRequest, cancellationToken);
                }

                listRequest.ContinuationToken = listResponse.NextContinuationToken;
            } while (listResponse.IsTruncated);

            _logger.LogInformation("Todos os objetos deletados do bucket {BucketName}", bucketName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar objetos do bucket: {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<bool> DeleteBucketAsync(string bucketName, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.DeleteBucketAsync(bucketName, cancellationToken);
            _logger.LogInformation("Bucket {BucketName} deletado", bucketName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar bucket: {BucketName}", bucketName);
            throw;
        }
    }

    public Task<string> GetPublicUrlAsync(string bucketName, string key)
    {
        var region = _s3Client.Config.RegionEndpoint.SystemName;
        var url = $"https://{bucketName}.s3.{region}.amazonaws.com/{key}";
        return Task.FromResult(url);
    }

    public string BuildS3Key(string? folder, Guid fileId, string fileName)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        var key = string.IsNullOrWhiteSpace(folder)
            ? $"{fileId}/{sanitizedFileName}"
            : $"{folder}/{fileId}/{sanitizedFileName}";
        
        return key;
    }

    public string BuildS3Key(string? folder, string fileName)
    {
        var sanitizedFileName = SanitizeFileName(fileName);
        var key = string.IsNullOrWhiteSpace(folder)
            ? sanitizedFileName
            : $"{folder}/{sanitizedFileName}";
        
        return key;
    }

    public string SanitizeFileName(string fileName)
    {
        var invalidChars = Path.GetInvalidFileNameChars();
        return string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
    }
}