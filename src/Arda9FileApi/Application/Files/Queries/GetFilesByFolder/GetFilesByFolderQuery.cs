using Ardalis.Result;
using Arda9FileApi.Application.DTOs;
using MediatR;

namespace Arda9FileApi.Application.Files.Queries.GetFilesByFolder;

public class GetFilesByFolderQuery : IRequest<Result<List<FileMetadataDto>>>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}