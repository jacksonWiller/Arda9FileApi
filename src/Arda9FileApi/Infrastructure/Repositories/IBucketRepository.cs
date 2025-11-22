using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Infrastructure.Repositories;

public interface IBucketRepository
{
    Task<BucketDto?> GetByIdAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<BucketDto?> GetByBucketNameAsync(string bucketName);
    Task<List<BucketDto>> GetByCompanyIdAsync(Guid companyId);
    Task<List<BucketDto>> GetAllAsync();
    Task CreateAsync(BucketDto bucket);
    Task UpdateAsync(BucketDto bucket);
    Task DeleteAsync(Guid id);
}