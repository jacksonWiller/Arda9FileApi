using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Auth.ForgotPassword;

public class ForgotPasswordCommand : IRequest<Result<ForgotPasswordResponse>>
{
    public string Email { get; set; } = string.Empty;
}