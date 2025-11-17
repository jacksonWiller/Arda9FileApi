using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Infrastructure.Repositories;

public class BucketRepository : IBucketRepository
{
    private readonly IDynamoDBContext _context;
    private readonly ILogger<BucketRepository> _logger;

    public BucketRepository(IDynamoDBContext context, ILogger<BucketRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<BucketDto?> GetByIdAsync(Guid id)
    {
        try
        {
            return await _context.LoadAsync<BucketDto>(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar bucket por ID: {Id}", id);
            throw;
        }
    }

    public async Task<BucketDto?> GetByBucketNameAsync(string bucketName)
    {
        try
        {
            var conditions = new List<ScanCondition>
            {
                new ScanCondition("BucketName", ScanOperator.Equal, bucketName),
                new ScanCondition("Status", ScanOperator.NotEqual, "deleted")
            };

            var search = _context.ScanAsync<BucketDto>(conditions);
            var results = await search.GetNextSetAsync();
            return results.FirstOrDefault();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar bucket por nome: {BucketName}", bucketName);
            throw;
        }
    }

    public async Task<List<BucketDto>> GetByCompanyIdAsync(Guid companyId)
    {
        try
        {
            var search = _context.QueryAsync<BucketDto>(
                $"COMPANY#{companyId}",
                new DynamoDBOperationConfig
                {
                    IndexName = "CompanyIndex"
                }
            );

            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar buckets por CompanyId: {CompanyId}", companyId);
            throw;
        }
    }

    public async Task<List<BucketDto>> GetAllAsync()
    {
        try
        {
            var search = _context.ScanAsync<BucketDto>(new List<ScanCondition>());
            return await search.GetRemainingAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os buckets");
            throw;
        }
    }

    public async Task CreateAsync(BucketDto bucket)
    {
        try
        {
            await _context.SaveAsync(bucket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar bucket: {BucketName}", bucket.BucketName);
            throw;
        }
    }

    public async Task UpdateAsync(BucketDto bucket)
    {
        try
        {
            bucket.UpdatedAt = DateTime.UtcNow;
            await _context.SaveAsync(bucket);
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
            var bucket = await _context.LoadAsync<BucketDto>(id);
            
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