using Amazon.CognitoIdentityProvider.Model;
using Arda9FileApi.Application.Services;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<RefreshTokenResponse>>
{
    private readonly IAuthService authService;
    private readonly ILogger<RefreshTokenCommandHandler> logger;

    public RefreshTokenCommandHandler(IAuthService authService, ILogger<RefreshTokenCommandHandler> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    public async Task<Result<RefreshTokenResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.RefreshTokenAsync(request.RefreshToken);

            return Result.Success(new RefreshTokenResponse
            {
                Success = true,
                AccessToken = response.AuthenticationResult.AccessToken,
                IdToken = response.AuthenticationResult.IdToken,
                ExpiresIn = response.AuthenticationResult.ExpiresIn,
                TokenType = response.AuthenticationResult.TokenType
            });
        }
        catch (NotAuthorizedException)
        {
            return Result.Error("Refresh token inválido ou expirado");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error refreshing token");
            return Result.Error("Erro ao renovar token");
        }
    }
}