using Ardalis.Result;
using MediatR;
using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Folders.Queries.GetFolderById;

public class GetFolderByIdQuery : IRequest<Result<FolderModel>>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}