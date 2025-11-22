using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Infrastructure.Repositories;

public interface IFileRepository
{
    Task<FileMetadataDto?> GetByIdAsync(Guid fileId);
    Task<FileMetadataDto?> GetByS3KeyAsync(string s3Key);
    Task<List<FileMetadataDto>> GetByCompanyIdAsync(Guid companyId);
    Task<List<FileMetadataDto>> GetByBucketNameAsync(string bucketName);
    Task<List<FileMetadataDto>> GetByBucketIdAsync(Guid bucketId);
    Task<List<FileMetadataDto>> GetByFolderIdAsync(Guid folderId);
    Task<List<FileMetadataDto>> GetAllAsync();
    Task CreateAsync(FileMetadataDto fileMetadata);
    Task UpdateAsync(FileMetadataDto fileMetadata);
    Task DeleteAsync(Guid fileId);
}