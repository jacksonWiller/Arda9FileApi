using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using MediatR;

namespace Arda9FileApi.Application.Folders.Queries.GetFoldersByParent;

public class GetFoldersByParentQuery : IRequest<Result<List<FolderDto>>>
{
    public Guid TenantId { get; set; }
    public Guid ParentFolderId { get; set; }
}