using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.ResetPassword;

public class ResetPasswordCommand : IRequest<Result<ResetPasswordResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}