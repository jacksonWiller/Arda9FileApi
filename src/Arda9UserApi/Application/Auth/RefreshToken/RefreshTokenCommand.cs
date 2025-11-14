using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Auth.RefreshToken;

public class RefreshTokenCommand : IRequest<Result<RefreshTokenResponse>>
{
    public string RefreshToken { get; set; } = string.Empty;
}