using Ardalis.Result;
using MediatR;
using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Features.Folders.Queries.GetFoldersByParent;

public class GetFoldersByParentQuery : IRequest<Result<List<FolderModel>>>
{
    public Guid TenantId { get; set; }
    public Guid ParentFolderId { get; set; }
}