using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Features.Folders.Commands.DeleteFolder;

public class DeleteFolderCommand : IRequest<Result>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}