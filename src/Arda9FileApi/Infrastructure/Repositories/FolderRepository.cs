using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Infrastructure.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<FolderRepository> _logger;
    private readonly IBucketRepository _bucketRepository;

    public FolderRepository(
        IDynamoDBContext context, 
        ILogger<FolderRepository> logger,
        IBucketRepository bucketRepository)
    {
        _context = context;
        _logger = logger;
        _bucketRepository = bucketRepository;
    }

    public async Task<FolderDto?> GetByIdAsync(Guid folderId)
    {
        try
        {
            var folder = await _context.LoadAsync<FolderDto>(folderId);
            return folder?.IsDeleted == false ? folder : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder by id: {FolderId}", folderId);
            throw;
        }
    }

    public async Task<FolderDto?> GetByPathAndNameAsync(Guid bucketId, string path, string folderName)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("BucketId", ScanOperator.Equal, bucketId),
                new ScanCondition("Path", ScanOperator.Equal, path),
                new ScanCondition("FolderName", ScanOperator.Equal, folderName),
                new ScanCondition("IsDeleted", ScanOperator.Equal, false)
            };

            var search = _context.ScanAsync<FolderDto>(conditions);
            var results = await search.GetNextSetAsync();
            return results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder by path and name");
            throw;
        }
    }

    public async Task<List<FolderDto>> GetByBucketIdAsync(Guid bucketId)
    {
        try
        {
            var search = _context.QueryAsync<FolderDto>(
                $"BUCKET#{bucketId}",
                new DynamoDBOperationConfig
                {
                    IndexName = "BucketIndex"
                }
            );

            var results = await search.GetRemainingAsync();
            return results.Where(f => !f.IsDeleted).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folders by bucket id: {BucketId}", bucketId);
            throw;
        }
    }

    public async Task<List<FolderDto>> GetByParentFolderIdAsync(Guid parentFolderId)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("ParentFolderId", ScanOperator.Equal, parentFolderId),
                new ScanCondition("IsDeleted", ScanOperator.Equal, false)
            };

            var search = _context.ScanAsync<FolderDto>(conditions);
            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folders by parent folder id: {ParentFolderId}", parentFolderId);
            throw;
        }
    }

    public async Task<List<FolderDto>> GetByCompanyIdAsync(Guid companyId)
    {
        try
        {
            var search = _context.QueryAsync<FolderDto>(
                $"COMPANY#{companyId}",
                new DynamoDBOperationConfig
                {
                    IndexName = "CompanyIndex"
                }
            );

            var results = await search.GetRemainingAsync();
            return results.Where(f => !f.IsDeleted).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folders by company id: {CompanyId}", companyId);
            throw;
        }
    }

    public async Task CreateAsync(FolderDto folder)
    {
        try
        {
            // Buscar o bucket
            var bucket = await _bucketRepository.GetByIdAsync(folder.BucketId);
            
            if (bucket != null)
            {
                // Adicionar a pasta à lista de pastas do bucket
                bucket.Folders.Add(folder);
                
                // Atualizar o bucket
                await _bucketRepository.UpdateAsync(bucket);
                
                _logger.LogInformation(
                    "Folder {FolderName} created and added to bucket {BucketId}", 
                    folder.FolderName, 
                    folder.BucketId);
            }
            else
            {
                _logger.LogWarning(
                    "Bucket {BucketId} not found when creating folder {FolderName}", 
                    folder.BucketId, 
                    folder.FolderName);
                throw new InvalidOperationException($"Bucket {folder.BucketId} not found");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder: {FolderName}", folder.FolderName);
            throw;
        }
    }

    public async Task UpdateAsync(FolderDto folder)
    {
        try
        {
            // Atualizar data de modificação
            folder.UpdatedAt = DateTime.UtcNow;
            
            // Salvar pasta atualizada no DynamoDB
            await _context.SaveAsync(folder);

            // Buscar o bucket
            var bucket = await _bucketRepository.GetByIdAsync(folder.BucketId);
            
            if (bucket != null)
            {
                // Encontrar e atualizar a pasta na lista do bucket
                var existingFolder = bucket.Folders.FirstOrDefault(f => f.Id == folder.Id);
                
                if (existingFolder != null)
                {
                    // Remover versão antiga
                    bucket.Folders.Remove(existingFolder);
                    // Adicionar versão atualizada
                    bucket.Folders.Add(folder);
                    
                    // Atualizar o bucket
                    await _bucketRepository.UpdateAsync(bucket);
                    
                    _logger.LogInformation(
                        "Folder {FolderId} updated in bucket {BucketId}", 
                        folder.Id, 
                        folder.BucketId);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating folder: {FolderId}", folder.Id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid folderId)
    {
        try
        {
            // Buscar pasta
            var folder = await GetByIdAsync(folderId);
            
            if (folder != null)
            {
                // Marcar como deletado (soft delete)
                folder.IsDeleted = true;
                folder.UpdatedAt = DateTime.UtcNow;
                
                // Salvar no DynamoDB
                await _context.SaveAsync(folder);

                // Buscar o bucket
                var bucket = await _bucketRepository.GetByIdAsync(folder.BucketId);
                
                if (bucket != null)
                {
                    // Remover a pasta da lista do bucket
                    var folderToRemove = bucket.Folders.FirstOrDefault(f => f.Id == folderId);
                    
                    if (folderToRemove != null)
                    {
                        bucket.Folders.Remove(folderToRemove);
                        
                        // Atualizar o bucket
                        await _bucketRepository.UpdateAsync(bucket);
                        
                        _logger.LogInformation(
                            "Folder {FolderId} deleted and removed from bucket {BucketId}", 
                            folderId, 
                            folder.BucketId);
                    }
                }
            }
            else
            {
                _logger.LogWarning("Folder {FolderId} not found to delete", folderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting folder: {FolderId}", folderId);
            throw;
        }
    }
}