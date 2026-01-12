using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Arda9File.Domain.Models;
using Arda9FileApi.Repositories;
using Microsoft.Extensions.Logging;

namespace Arda9File.Infra.Repositories;

public class FolderRepository : IFolderRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<FolderRepository> _logger;

    public FolderRepository(
        IDynamoDBContext context, 
        ILogger<FolderRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<FolderModel?> GetByIdAsync(Guid folderId)
    {
        try
        {
            var pk = $"FOLDER#{folderId}";
            var sk = "METADATA";
            var folder = await _context.LoadAsync<FolderModel>(pk, sk);
            return folder?.IsDeleted == false ? folder : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder by id: {FolderId}", folderId);
            throw;
        }
    }

    public async Task<FolderModel?> GetByPathAndNameAsync(Guid bucketId, string path, string folderName)
    {
        try
        {
            // Use GSI1 to query folders by bucket, then filter in memory
            // This is more efficient than a scan and avoids Guid serialization issues
            var search = _context.QueryAsync<FolderModel>(
                $"BUCKET#{bucketId}",
                new DynamoDBOperationConfig
                {
                    IndexName = "GSI1-Index"
                }
            );

            var allFoldersInBucket = await search.GetRemainingAsync();
            
            // Filter in memory for the specific path and folder name
            return allFoldersInBucket
                .FirstOrDefault(f => 
                    f.EntityType == "FOLDER" &&
                    !f.IsDeleted &&
                    f.Path == path &&
                    f.FolderName == folderName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folder by path and name");
            throw;
        }
    }

    public async Task<List<FolderModel>> GetByBucketIdAsync(Guid bucketId)
    {
        try
        {
            // Usar GSI1 para buscar folders por Bucket
            var search = _context.QueryAsync<FolderModel>(
                $"BUCKET#{bucketId}",
                new DynamoDBOperationConfig
                {
                    IndexName = "GSI1-Index"
                }
            );

            var results = await search.GetRemainingAsync();
            return results.Where(f => !f.IsDeleted && f.EntityType == "FOLDER").ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folders by bucket id: {BucketId}", bucketId);
            throw;
        }
    }

    public async Task<List<FolderModel>> GetByParentFolderIdAsync(Guid parentFolderId)
    {
        try
        {
            // First get the parent folder to find the bucket
            var parentFolder = await GetByIdAsync(parentFolderId);
            if (parentFolder == null)
            {
                _logger.LogWarning("Parent folder {ParentFolderId} not found", parentFolderId);
                return new List<FolderModel>();
            }

            // Use GSI1 to query all folders in the same bucket, then filter in memory
            // This avoids Guid serialization issues with scan conditions
            var search = _context.QueryAsync<FolderModel>(
                $"BUCKET#{parentFolder.BucketId}",
                new DynamoDBOperationConfig
                {
                    IndexName = "GSI1-Index"
                }
            );

            var allFoldersInBucket = await search.GetRemainingAsync();
            
            // Filter in memory for folders with this parent
            return allFoldersInBucket
                .Where(f => 
                    f.EntityType == "FOLDER" &&
                    !f.IsDeleted &&
                    f.ParentFolderId == parentFolderId)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folders by parent folder id: {ParentFolderId}", parentFolderId);
            throw;
        }
    }

    public async Task<List<FolderModel>> GetByCompanyIdAsync(Guid companyId)
    {
        try
        {
            // Usar GSI3 para buscar folders por Company
            var search = _context.QueryAsync<FolderModel>(
                $"COMPANY#{companyId}",
                new DynamoDBOperationConfig
                {
                    IndexName = "GSI3-Index"
                }
            );

            var results = await search.GetRemainingAsync();
            return results.Where(f => !f.IsDeleted && f.EntityType == "FOLDER").ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting folders by company id: {CompanyId}", companyId);
            throw;
        }
    }

    public async Task CreateAsync(FolderModel folder)
    {
        try
        {
            // Definir PK, SK e EntityType
            folder.PK = $"FOLDER#{folder.Id}";
            folder.SK = "METADATA";
            folder.EntityType = "FOLDER";

            // Definir GSIs
            folder.GSI1PK = $"BUCKET#{folder.BucketId}";
            folder.GSI1SK = $"FOLDER#{folder.Id}";
            folder.GSI3PK = $"COMPANY#{folder.CompanyId}";

            // Salvar no DynamoDB
            await _context.SaveAsync(folder);
            
            _logger.LogInformation(
                "Folder {FolderName} created with ID {FolderId}", 
                folder.FolderName, 
                folder.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating folder: {FolderName}", folder.FolderName);
            throw;
        }
    }

    public async Task UpdateAsync(FolderModel folder)
    {
        try
        {
            // Atualizar data de modificação
            folder.UpdatedAt = DateTime.UtcNow;

            // Garantir que PK, SK e EntityType estejam corretos
            folder.PK = $"FOLDER#{folder.Id}";
            folder.SK = "METADATA";
            folder.EntityType = "FOLDER";

            // Atualizar GSIs
            folder.GSI1PK = $"BUCKET#{folder.BucketId}";
            folder.GSI1SK = $"FOLDER#{folder.Id}";
            folder.GSI3PK = $"COMPANY#{folder.CompanyId}";
            
            // Salvar pasta atualizada no DynamoDB
            await _context.SaveAsync(folder);
            
            _logger.LogInformation(
                "Folder {FolderId} updated", 
                folder.Id);
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
                
                _logger.LogInformation("Folder {FolderId} deleted", folderId);
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