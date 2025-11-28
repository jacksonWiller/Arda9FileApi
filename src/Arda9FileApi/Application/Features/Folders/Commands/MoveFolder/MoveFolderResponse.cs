namespace Arda9FileApi.Application.Features.Folders.Commands.MoveFolder;

public class MoveFolderResponse
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public List<string> Path { get; set; } = new();
}
