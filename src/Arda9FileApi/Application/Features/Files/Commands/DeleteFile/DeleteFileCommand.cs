using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Features.Files.Commands.DeleteFile;

public class DeleteFileCommand : IRequest<Result>
{
    public Guid TenantId { get; set; }
    public Guid FileId { get; set; }
    public bool HardDelete { get; set; } = false;
}