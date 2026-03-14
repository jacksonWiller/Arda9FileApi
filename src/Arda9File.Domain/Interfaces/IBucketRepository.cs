using Arda9File.Domain.Models;

namespace Arda9File.Domain.Repositories;

public interface IBucketRepository
{
    Task<BucketModel?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<BucketModel?> GetByBucketNameAsync(string bucketName);
    Task<List<BucketModel>> GetByTenantIdAsync(Guid companyId);
    Task<List<BucketModel>> GetAllAsync();
    Task CreateAsync(BucketModel bucket);
    Task UpdateAsync(BucketModel bucket);
    Task DeleteAsync(Guid id);
}