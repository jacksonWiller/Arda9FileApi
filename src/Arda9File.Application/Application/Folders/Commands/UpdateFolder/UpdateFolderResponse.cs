using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Folders.Commands.UpdateFolder;

public class UpdateFolderResponse
{
    public FolderModel? Folder { get; set; }
    public string Message { get; set; } = string.Empty;
}