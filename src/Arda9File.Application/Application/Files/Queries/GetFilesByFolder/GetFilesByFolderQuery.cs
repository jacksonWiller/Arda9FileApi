using Ardalis.Result;
using MediatR;
using Arda9File.Domain.Models;

namespace Arda9File.Application.Application.Files.Queries.GetFilesByFolder;

public class GetFilesByFolderQuery : IRequest<Result<List<FileMetadataModel>>>
{
    public Guid TenantId { get; set; }
    public Guid FolderId { get; set; }
}