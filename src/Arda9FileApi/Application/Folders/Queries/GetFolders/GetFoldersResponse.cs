namespace Arda9FileApi.Application.Folders.Queries.GetFolders;

public class GetFoldersResponse
{
    public List<FolderDetailDto> Folders { get; set; } = new();
}

public class FolderDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public Guid? ParentId { get; set; }
    public List<string> Path { get; set; } = new();
    public int Depth { get; set; }
    public int FileCount { get; set; }
    public int FolderCount { get; set; }
    public long TotalSize { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IsShared { get; set; }
    public FolderPermissions Permissions { get; set; } = new();
    public List<FolderDetailDto> Children { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public FolderCreatedBy? CreatedBy { get; set; }
}

public class FolderPermissions
{
    public bool CanView { get; set; } = true;
    public bool CanEdit { get; set; } = true;
    public bool CanDelete { get; set; } = true;
    public bool CanShare { get; set; } = true;
}

public class FolderCreatedBy
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}
