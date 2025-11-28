using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Features.Folders.Commands.CreateFolder;

public class CreateFolderResponse
{
    public FolderModel? Folder { get; set; }
    public string Message { get; set; } = string.Empty;
}