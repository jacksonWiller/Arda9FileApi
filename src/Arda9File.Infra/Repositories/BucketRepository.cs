using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Arda9File.Domain.Models;
using Arda9File.Domain.Repositories;
using Microsoft.Extensions.Logging;

namespace Arda9File.Infra.Repositories;

public class BucketRepository : IBucketRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<BucketRepository> _logger;

    public BucketRepository(IDynamoDBContext context, ILogger<BucketRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BucketModel?> GetByIdAsync(Guid id)
    {
        try
        {
            var pk = $"BUCKET#{id}";
            var sk = "METADATA";
            return await _context.LoadAsync<BucketModel>(pk, sk);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar bucket por ID: {Id}", id);
            throw;
        }
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        try
        {
            var result = await GetByIdAsync(id);
            return result != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do bucket: {Id}", id);
            throw;
        }
    }

    public async Task<BucketModel?> GetByBucketNameAsync(string bucketName)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("BucketName", ScanOperator.Equal, bucketName),
                new ScanCondition("Status", ScanOperator.NotEqual, "deleted"),
                new ScanCondition("EntityType", ScanOperator.Equal, "BUCKET")
            };

            var search = _context.ScanAsync<BucketModel>(conditions);
            var results = await search.GetNextSetAsync();
            return results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar bucket por nome: {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<List<BucketModel>> GetByCompanyIdAsync(Guid companyId)
    {
        try
        {
            // Usar GSI3 para buscar buckets por Company
            var search = _context.QueryAsync<BucketModel>(
                $"COMPANY#{companyId}",
                new DynamoDBOperationConfig
                {
                    IndexName = "GSI3-Index"
                }
            );

            var results = await search.GetRemainingAsync();
            return results.Where(b => b.EntityType == "BUCKET").ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar buckets por CompanyId: {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<List<BucketModel>> GetAllAsync()
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("EntityType", ScanOperator.Equal, "BUCKET")
            };

            var search = _context.ScanAsync<BucketModel>(conditions);
            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os buckets");
            throw;
        }
    }

    public async Task CreateAsync(BucketModel bucket)
    {
        try
        {
            bucket.PK = $"BUCKET#{bucket.Id}";
            bucket.SK = "METADATA";
            bucket.EntityType = "BUCKET";
            bucket.GSI3PK = $"COMPANY#{bucket.CompanyId}";
            
            await _context.SaveAsync(bucket);
            
            _logger.LogInformation("Bucket {BucketName} criado com ID {BucketId}", 
                bucket.BucketName, bucket.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar bucket: {BucketName}", bucket.BucketName);
            throw;
        }
    }

    public async Task UpdateAsync(BucketModel bucket)
    {
        try
        {
            bucket.UpdatedAt = DateTime.UtcNow;
            bucket.PK = $"BUCKET#{bucket.Id}";
            bucket.SK = "METADATA";
            bucket.EntityType = "BUCKET";
            bucket.GSI3PK = $"COMPANY#{bucket.CompanyId}";
            
            await _context.SaveAsync(bucket);
            
            _logger.LogInformation("Bucket {BucketId} atualizado", bucket.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar bucket: {BucketName}", bucket.BucketName);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        try
        {
            var bucket = await GetByIdAsync(id);
            
            if (bucket == null)
            {
                _logger.LogWarning("Bucket não encontrado para soft delete: {Id}", id);
                throw new KeyNotFoundException($"Bucket com ID {id} não encontrado");
            }

            bucket.Status = "deleted";
            bucket.UpdatedAt = DateTime.UtcNow;
            
            await _context.SaveAsync(bucket);
            
            _logger.LogInformation("Bucket marcado como deletado: {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao deletar bucket: {Id}", id);
            throw;
        }
    }
}