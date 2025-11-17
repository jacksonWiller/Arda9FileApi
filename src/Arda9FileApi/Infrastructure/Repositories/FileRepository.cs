using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<FileRepository> _logger;
    private readonly IBucketRepository _bucketRepository;

    public FileRepository(
        IDynamoDBContext context, 
        ILogger<FileRepository> logger,
        IBucketRepository bucketRepository)
    {
        _context = context;
        _logger = logger;
        _bucketRepository = bucketRepository;
    }

    public async Task<FileMetadataDto?> GetByIdAsync(Guid fileId)
    {
        try
        {
            return await _context.LoadAsync<FileMetadataDto>($"FILE#{fileId}", "METADATA");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivo por ID: {FileId}", fileId);
            throw;
        }
    }

    public async Task<FileMetadataDto?> GetByS3KeyAsync(string s3Key)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("S3Key", ScanOperator.Equal, s3Key),
                new ScanCondition("IsDeleted", ScanOperator.Equal, false)
            };

            var search = _context.ScanAsync<FileMetadataDto>(conditions);
            var results = await search.GetNextSetAsync();
            return results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivo por S3Key: {S3Key}", s3Key);
            throw;
        }
    }

    public async Task<List<FileMetadataDto>> GetByCompanyIdAsync(Guid companyId)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("CompanyId", ScanOperator.Equal, companyId),
                new ScanCondition("IsDeleted", ScanOperator.Equal, false)
            };

            var search = _context.ScanAsync<FileMetadataDto>(conditions);
            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivos por CompanyId: {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<List<FileMetadataDto>> GetByBucketNameAsync(string bucketName)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("BucketName", ScanOperator.Equal, bucketName),
                new ScanCondition("IsDeleted", ScanOperator.Equal, false)
            };

            var search = _context.ScanAsync<FileMetadataDto>(conditions);
            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar arquivos por BucketName: {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<List<FileMetadataDto>> GetAllAsync()
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("IsDeleted", ScanOperator.Equal, false)
            };

            var search = _context.ScanAsync<FileMetadataDto>(conditions);
            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os arquivos");
            throw;
        }
    }

    public async Task CreateAsync(FileMetadataDto fileMetadata)
    {
        try
        {
            // Salvar o arquivo no DynamoDB
            //await _context.SaveAsync(fileMetadata);

            // Buscar o bucket
            var bucket = await _bucketRepository.GetByBucketNameAsync(fileMetadata.BucketName);
            
            if (bucket != null)
            {
                // Adicionar o arquivo à lista de arquivos do bucket
                bucket.Files.Add(fileMetadata);
                
                // Atualizar o bucket
                await _bucketRepository.UpdateAsync(bucket);
                
                _logger.LogInformation(
                    "Arquivo {FileName} criado e adicionado ao bucket {BucketName}", 
                    fileMetadata.FileName, 
                    fileMetadata.BucketName);
            }
            else
            {
                _logger.LogWarning(
                    "Bucket {BucketName} não encontrado ao criar arquivo {FileName}", 
                    fileMetadata.BucketName, 
                    fileMetadata.FileName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar arquivo: {FileName}", fileMetadata.FileName);
            throw;
        }
    }

    public async Task UpdateAsync(FileMetadataDto fileMetadata)
    {
        try
        {
            // Atualizar data de modificação
            fileMetadata.UpdatedAt = DateTime.UtcNow;
            
            // Salvar arquivo atualizado no DynamoDB
            await _context.SaveAsync(fileMetadata);

            // Buscar o bucket
            var bucket = await _bucketRepository.GetByBucketNameAsync(fileMetadata.BucketName);
            
            if (bucket != null)
            {
                // Encontrar e atualizar o arquivo na lista do bucket
                var existingFile = bucket.Files.FirstOrDefault(f => f.FileId == fileMetadata.FileId);
                
                if (existingFile != null)
                {
                    // Remover versão antiga
                    bucket.Files.Remove(existingFile);
                    // Adicionar versão atualizada
                    bucket.Files.Add(fileMetadata);
                    
                    // Atualizar o bucket
                    await _bucketRepository.UpdateAsync(bucket);
                    
                    _logger.LogInformation(
                        "Arquivo {FileId} atualizado no bucket {BucketName}", 
                        fileMetadata.FileId, 
                        fileMetadata.BucketName);
                }
                else
                {
                    _logger.LogWarning(
                        "Arquivo {FileId} não encontrado na lista do bucket {BucketName}", 
                        fileMetadata.FileId, 
                        fileMetadata.BucketName);
                }
            }
            else
            {
                _logger.LogWarning(
                    "Bucket {BucketName} não encontrado ao atualizar arquivo {FileId}", 
                    fileMetadata.BucketName, 
                    fileMetadata.FileId);
            }
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

                // Buscar o bucket
                var bucket = await _bucketRepository.GetByBucketNameAsync(file.BucketName);
                
                if (bucket != null)
                {
                    // Remover o arquivo da lista do bucket
                    var fileToRemove = bucket.Files.FirstOrDefault(f => f.FileId == fileId);
                    
                    if (fileToRemove != null)
                    {
                        bucket.Files.Remove(fileToRemove);
                        
                        // Atualizar o bucket
                        await _bucketRepository.UpdateAsync(bucket);
                        
                        _logger.LogInformation(
                            "Arquivo {FileId} deletado e removido do bucket {BucketName}", 
                            fileId, 
                            file.BucketName);
                    }
                    else
                    {
                        _logger.LogWarning(
                            "Arquivo {FileId} não encontrado na lista do bucket {BucketName}", 
                            fileId, 
                            file.BucketName);
                    }
                }
                else
                {
                    _logger.LogWarning(
                        "Bucket {BucketName} não encontrado ao deletar arquivo {FileId}", 
                        file.BucketName, 
                        fileId);
                }
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