using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.ForgotPassword;

public class ForgotPasswordCommand : IRequest<Result<ForgotPasswordResponse>>
{
    public string Email { get; set; } = string.Empty;
}