using Ardalis.Result;
using MediatR;
using Arda9FileApi.Models;

namespace Arda9FileApi.Application.Features.Files.Queries.GetFilesByFolder;

public class GetFilesByFolderQuery : IRequest<Result<List<FileMetadataModel>>>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}