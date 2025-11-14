using Amazon.CognitoIdentityProvider.Model;
using Arda9UserApi.Application.Extensions;
using Arda9UserApi.Application.Services;
using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Auth.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IAuthService authService;
    private readonly ILogger<LoginCommandHandler> logger;
    private readonly IHttpContextAccessor httpContextAccessor;

    public LoginCommandHandler(
        IAuthService authService, 
        ILogger<LoginCommandHandler> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        this.authService = authService;
        this.logger = logger;
        this.httpContextAccessor = httpContextAccessor;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var authResponse = await authService.LoginAsync(request.Email, request.Password);

            return Result.Success(new LoginResponse
            {
                Success = true,
                AccessToken = authResponse.AuthenticationResult.AccessToken,
                IdToken = authResponse.AuthenticationResult.IdToken,
                RefreshToken = authResponse.AuthenticationResult.RefreshToken,
                ExpiresIn = authResponse.AuthenticationResult.ExpiresIn,
                TokenType = authResponse.AuthenticationResult.TokenType
            });
        }
        catch (NotAuthorizedException)
        {
            return ResultError.Error("Email ou senha incorretos");
        }
        catch (UserNotConfirmedException)
        {
            return ResultError.Error("Usuário não confirmado. Verifique seu email.");
        }
        catch (InvalidParameterException ex)
        {
            return ResultError.Error($"Parâmetros inválidos: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error logging in user {Email}", request.Email);
            return ResultError.Error("Erro ao realizar login");
        }
    }
}