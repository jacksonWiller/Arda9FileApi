using Arda9File.Domain.Models;

namespace Arda9FileApi.Repositories;

public interface IFolderRepository
{
    Task<FolderModel?> GetByIdAsync(Guid folderId);
    Task<FolderModel?> GetByPathAndNameAsync(Guid bucketId, string path, string folderName);
    Task<List<FolderModel>> GetByBucketIdAsync(Guid bucketId);
    Task<List<FolderModel>> GetByParentFolderIdAsync(Guid parentFolderId);
    Task<List<FolderModel>> GetByCompanyIdAsync(Guid companyId);
    Task CreateAsync(FolderModel folder);
    Task UpdateAsync(FolderModel folder);
    Task DeleteAsync(Guid folderId);
}