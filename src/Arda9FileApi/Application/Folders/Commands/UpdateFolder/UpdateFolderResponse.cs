using Arda9FileApi.Application.DTOs;

namespace Arda9FileApi.Application.Folders.Commands.UpdateFolder;

public class UpdateFolderResponse
{
    public FolderDto? Folder { get; set; }
    public string Message { get; set; } = string.Empty;
}