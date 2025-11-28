using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Features.Folders.Commands.MoveFolder;

public class MoveFolderCommand : IRequest<Result<MoveFolderResponse>>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
    public Guid? ParentId { get; set; }
}
