using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Features.Files.Commands.DuplicateFile;

public class DuplicateFileCommand : IRequest<Result<DuplicateFileResponse>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
    public string? Name { get; set; }
    public Guid? FolderId { get; set; }
}
