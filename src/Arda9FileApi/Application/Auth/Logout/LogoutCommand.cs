using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.Logout;

public class LogoutCommand : IRequest<Result<LogoutResponse>>
{
    public string AccessToken { get; set; } = string.Empty;
}