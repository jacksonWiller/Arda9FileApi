using Ardalis.Result;
using MediatR;
using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Features.Folders.Queries.GetFolderById;

public class GetFolderByIdQuery : IRequest<Result<FolderModel>>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}