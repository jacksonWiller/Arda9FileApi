using Ardalis.Result;
using MediatR;

namespace Arda9File.Application.Application.Folders.Commands.DeleteFolder;

public class DeleteFolderCommand : IRequest<Result>
{
    public Guid FolderId { get; set; }
}