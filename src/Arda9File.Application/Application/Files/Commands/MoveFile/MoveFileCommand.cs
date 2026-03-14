using Ardalis.Result;
using MediatR;

namespace Arda9File.Application.Application.Files.Commands.MoveFile;

public class MoveFileCommand : IRequest<Result<MoveFileResponse>>
{
    public Guid FileId { get; set; }
    public Guid? FolderId { get; set; }
}
