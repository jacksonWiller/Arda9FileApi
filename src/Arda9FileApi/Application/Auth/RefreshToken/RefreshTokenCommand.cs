using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.RefreshToken;

public class RefreshTokenCommand : IRequest<Result<RefreshTokenResponse>>
{
    public string RefreshToken { get; set; } = string.Empty;
}