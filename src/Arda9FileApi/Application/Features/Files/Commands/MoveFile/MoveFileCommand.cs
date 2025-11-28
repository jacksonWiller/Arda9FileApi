using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Features.Files.Commands.MoveFile;

public class MoveFileCommand : IRequest<Result<MoveFileResponse>>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
    public Guid? FolderId { get; set; }
}
