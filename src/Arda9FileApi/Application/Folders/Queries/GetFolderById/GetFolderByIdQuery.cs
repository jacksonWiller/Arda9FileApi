using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using MediatR;

namespace Arda9FileApi.Application.Folders.Queries.GetFolderById;

public class GetFolderByIdQuery : IRequest<Result<FolderDto>>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}