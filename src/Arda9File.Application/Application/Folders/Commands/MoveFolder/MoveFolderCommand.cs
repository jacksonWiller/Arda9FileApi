using Ardalis.Result;
using MediatR;

namespace Arda9File.Application.Application.Folders.Commands.MoveFolder;

public class MoveFolderCommand : IRequest<Result<MoveFolderResponse>>
{
    public Guid FolderId { get; set; }
    public Guid? ParentId { get; set; }
}
