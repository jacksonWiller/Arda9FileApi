using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Auth.Login;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}