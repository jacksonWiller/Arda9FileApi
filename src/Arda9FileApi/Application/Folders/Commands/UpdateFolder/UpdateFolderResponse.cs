using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Folders.Commands.UpdateFolder;

public class UpdateFolderResponse
{
    public FolderModel? Folder { get; set; }
    public string Message { get; set; } = string.Empty;
}