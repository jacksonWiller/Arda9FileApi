using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Auth.ConfirmEmail;

public class ConfirmEmailCommand : IRequest<Result<ConfirmEmailResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}