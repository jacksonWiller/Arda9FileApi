using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Arda9FileApi.Models;
using Microsoft.Extensions.Logging;

namespace Arda9FileApi.Repositories;

public class FileRepository : IFileRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<FileRepository> _logger;

    public FileRepository(
        IDynamoDBContext context, 
        ILogger<FileRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FileMetadataModel?> GetByIdAsync(Guid fileId)
    {
        try
        {
            var pk = $"FILE#{fileId}";
            var sk = "METADATA";
            return await _context.LoadAsync<FileMetadataModel>(pk, sk);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivo por ID: {FileId}", fileId);
            throw;
        }
    }

    public async Task<FileMetadataModel?> GetByS3KeyAsync(string s3Key)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("S3Key", ScanOperator.Equal, s3Key),
                new ScanCondition("IsDeleted", ScanOperator.Equal, false),
                new ScanCondition("EntityType", ScanOperator.Equal, "FILE")
            };

            var search = _context.ScanAsync<FileMetadataModel>(conditions);
            var results = await search.GetNextSetAsync();
            return results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivo por S3Key: {S3Key}", s3Key);
            throw;
        }
    }

    public async Task<List<FileMetadataModel>> GetByCompanyIdAsync(Guid companyId)
    {
        try
        {
            // Usar GSI3 para buscar por Company
            var search = _context.QueryAsync<FileMetadataModel>(
                $"COMPANY#{companyId}",
                new DynamoDBOperationConfig
                {
                    IndexName = "GSI3-Index"
                }
            );

            var results = await search.GetRemainingAsync();
            return results.Where(f => !f.IsDeleted).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivos por CompanyId: {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<List<FileMetadataModel>> GetByBucketNameAsync(string bucketName)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("BucketName", ScanOperator.Equal, bucketName),
                new ScanCondition("IsDeleted", ScanOperator.Equal, false),
                new ScanCondition("EntityType", ScanOperator.Equal, "FILE")
            };

            var search = _context.ScanAsync<FileMetadataModel>(conditions);
            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivos por BucketName: {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<List<FileMetadataModel>> GetByBucketIdAsync(Guid bucketId)
    {
        try
        {
            // Usar GSI1 para buscar arquivos por Bucket
            var search = _context.QueryAsync<FileMetadataModel>(
                $"BUCKET#{bucketId}",
                new DynamoDBOperationConfig
                {
                    IndexName = "GSI1-Index"
                }
            );

            var results = await search.GetRemainingAsync();
            return results.Where(f => !f.IsDeleted).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivos por BucketId: {BucketId}", bucketId);
            throw;
        }
    }

    public async Task<List<FileMetadataModel>> GetByFolderIdAsync(Guid folderId)
    {
        try
        {
            // Usar GSI2 para buscar arquivos por Folder
            var search = _context.QueryAsync<FileMetadataModel>(
                $"FOLDER#{folderId}",
                new DynamoDBOperationConfig
                {
                    IndexName = "GSI2-Index"
                }
            );

            var results = await search.GetRemainingAsync();
            return results.Where(f => !f.IsDeleted).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivos por FolderId: {FolderId}", folderId);
            throw;
        }
    }

    public async Task<List<FileMetadataModel>> GetAllAsync()
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("IsDeleted", ScanOperator.Equal, false),
                new ScanCondition("EntityType", ScanOperator.Equal, "FILE")
            };

            var search = _context.ScanAsync<FileMetadataModel>(conditions);
            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os arquivos");
            throw;
        }
    }

    public async Task CreateAsync(FileMetadataModel fileMetadata)
    {
        try
        {
            // Definir PK, SK e EntityType
            fileMetadata.PK = $"FILE#{fileMetadata.FileId}";
            fileMetadata.SK = "METADATA";
            fileMetadata.EntityType = "FILE";

            // Definir GSIs
            fileMetadata.GSI1PK = $"BUCKET#{fileMetadata.BucketId}";
            fileMetadata.GSI1SK = $"FILE#{fileMetadata.FileId}";
            
            if (fileMetadata.FolderId.HasValue)
            {
                fileMetadata.GSI2PK = $"FOLDER#{fileMetadata.FolderId.Value}";
                fileMetadata.GSI2SK = $"FILE#{fileMetadata.FileId}";
            }

            fileMetadata.GSI3PK = $"COMPANY#{fileMetadata.CompanyId}";

            // Salvar o arquivo no DynamoDB
            await _context.SaveAsync(fileMetadata);
            
            _logger.LogInformation(
                "Arquivo {FileName} criado com ID {FileId}", 
                fileMetadata.FileName, 
                fileMetadata.FileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar arquivo: {FileName}", fileMetadata.FileName);
            throw;
        }
    }

    public async Task UpdateAsync(FileMetadataModel fileMetadata)
    {
        try
        {
            // Atualizar data de modificação
            fileMetadata.UpdatedAt = DateTime.UtcNow;

            // Garantir que PK, SK e EntityType estejam corretos
            fileMetadata.PK = $"FILE#{fileMetadata.FileId}";
            fileMetadata.SK = "METADATA";
            fileMetadata.EntityType = "FILE";

            // Atualizar GSIs
            fileMetadata.GSI1PK = $"BUCKET#{fileMetadata.BucketId}";
            fileMetadata.GSI1SK = $"FILE#{fileMetadata.FileId}";
            
            if (fileMetadata.FolderId.HasValue)
            {
                fileMetadata.GSI2PK = $"FOLDER#{fileMetadata.FolderId.Value}";
                fileMetadata.GSI2SK = $"FILE#{fileMetadata.FileId}";
            }
            else
            {
                fileMetadata.GSI2PK = string.Empty;
                fileMetadata.GSI2SK = string.Empty;
            }

            fileMetadata.GSI3PK = $"COMPANY#{fileMetadata.CompanyId}";
            
            // Salvar arquivo atualizado no DynamoDB
            await _context.SaveAsync(fileMetadata);
            
            _logger.LogInformation(
                "Arquivo {FileId} atualizado", 
                fileMetadata.FileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar arquivo: {FileId}", fileMetadata.FileId);
            throw;
        }
    }

    public async Task DeleteAsync(Guid fileId)
    {
        try
        {
            // Buscar arquivo
            var file = await GetByIdAsync(fileId);
            
            if (file != null)
            {
                // Marcar como deletado (soft delete)
                file.IsDeleted = true;
                file.UpdatedAt = DateTime.UtcNow;
                
                // Salvar no DynamoDB
                await _context.SaveAsync(file);
                
                _logger.LogInformation("Arquivo {FileId} deletado", fileId);
            }
            else
            {
                _logger.LogWarning("Arquivo {FileId} não encontrado para deletar", fileId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar arquivo: {FileId}", fileId);
            throw;
        }
    }
}