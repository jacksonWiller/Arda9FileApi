using Arda9UserApi.Application.Buckets.DTOs;

namespace Arda9UserApi.Infrastructure.Repositories;

public interface IBucketRepository
{
    Task<BucketDto?> GetByIdAsync(Guid id);
    Task<BucketDto?> GetByBucketNameAsync(string bucketName);
    Task<List<BucketDto>> GetByCompanyIdAsync(Guid companyId);
    Task<List<BucketDto>> GetAllAsync();
    Task CreateAsync(BucketDto bucket);
    Task UpdateAsync(BucketDto bucket);
    Task DeleteAsync(Guid id);
}