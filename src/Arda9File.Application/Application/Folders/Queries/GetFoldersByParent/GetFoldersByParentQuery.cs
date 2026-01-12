using Ardalis.Result;
using MediatR;
using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Folders.Queries.GetFoldersByParent;

public class GetFoldersByParentQuery : IRequest<Result<List<FolderModel>>>
{
    public Guid TenantId { get; set; }
    public Guid ParentFolderId { get; set; }
}