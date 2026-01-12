using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Folders.Commands.CreateFolder;

public class CreateFolderResponse
{
    public FolderModel? Folder { get; set; }
    public string Message { get; set; } = string.Empty;
}