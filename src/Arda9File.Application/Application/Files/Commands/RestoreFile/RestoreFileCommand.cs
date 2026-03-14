using Ardalis.Result;
using MediatR;

namespace Arda9File.Application.Application.Files.Commands.RestoreFile;

public class RestoreFileCommand : IRequest<Result<RestoreFileResponse>>
{
    public Guid FileId { get; set; }
}
