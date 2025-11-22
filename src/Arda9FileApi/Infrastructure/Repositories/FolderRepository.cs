using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Infrastructure.Repositories;

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

    public async Task<FolderDto?> GetByIdAsync(Guid folderId)
    {
        try
        {
            var pk = $"FOLDER#{folderId}";
            var sk = "METADATA";
            var folder = await _context.LoadAsync<FolderDto>(pk, sk);
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
                new ScanCondition("IsDeleted", ScanOperator.Equal, false),
                new ScanCondition("EntityType", ScanOperator.Equal, "FOLDER")
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
            // Usar GSI1 para buscar folders por Bucket
            var search = _context.QueryAsync<FolderDto>(
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

    public async Task<List<FolderDto>> GetByParentFolderIdAsync(Guid parentFolderId)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("ParentFolderId", ScanOperator.Equal, parentFolderId),
                new ScanCondition("IsDeleted", ScanOperator.Equal, false),
                new ScanCondition("EntityType", ScanOperator.Equal, "FOLDER")
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
            // Usar GSI3 para buscar folders por Company
            var search = _context.QueryAsync<FolderDto>(
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

    public async Task CreateAsync(FolderDto folder)
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

    public async Task UpdateAsync(FolderDto folder)
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