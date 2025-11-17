using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Infrastructure.Repositories;

public interface IFolderRepository
{
    Task<FolderDto?> GetByIdAsync(Guid folderId);
    Task<FolderDto?> GetByPathAndNameAsync(Guid bucketId, string path, string folderName);
    Task<List<FolderDto>> GetByBucketIdAsync(Guid bucketId);
    Task<List<FolderDto>> GetByParentFolderIdAsync(Guid parentFolderId);
    Task<List<FolderDto>> GetByCompanyIdAsync(Guid companyId);
    Task CreateAsync(FolderDto folder);
    Task UpdateAsync(FolderDto folder);
    Task DeleteAsync(Guid folderId);
}