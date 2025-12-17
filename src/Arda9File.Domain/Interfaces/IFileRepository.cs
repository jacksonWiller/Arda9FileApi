using Arda9FileApi.Models;

namespace Arda9FileApi.Repositories;

public interface IFileRepository
{
    Task<FileMetadataModel?> GetByIdAsync(Guid fileId);
    Task<FileMetadataModel?> GetByS3KeyAsync(string s3Key);
    Task<List<FileMetadataModel>> GetByCompanyIdAsync(Guid companyId);
    Task<List<FileMetadataModel>> GetByBucketNameAsync(string bucketName);
    Task<List<FileMetadataModel>> GetByBucketIdAsync(Guid bucketId);
    Task<List<FileMetadataModel>> GetByFolderIdAsync(Guid folderId);
    Task<List<FileMetadataModel>> GetAllAsync();
    Task CreateAsync(FileMetadataModel fileMetadata);
    Task UpdateAsync(FileMetadataModel fileMetadata);
    Task DeleteAsync(Guid fileId);
}