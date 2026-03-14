using Ardalis.Result;
using MediatR;

namespace Arda9File.Application.Application.Files.Commands.DeleteFile;

public class DeleteFileCommand : IRequest<Result>
{
    public Guid FileId { get; set; }
    public bool HardDelete { get; set; } = false;
}