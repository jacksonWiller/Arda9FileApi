using Amazon.CognitoIdentityProvider.Model;
using Arda9FileApi.Application.Services;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result<ResetPasswordResponse>>
{
    private readonly IAuthService authService;
    private readonly ILogger<ResetPasswordCommandHandler> logger;

    public ResetPasswordCommandHandler(IAuthService authService, ILogger<ResetPasswordCommandHandler> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    public async Task<Result<ResetPasswordResponse>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await authService.ConfirmForgotPasswordAsync(request.Email, request.Code, request.NewPassword);

            return Result.Success(new ResetPasswordResponse
            {
                Success = true,
                Message = "Senha redefinida com sucesso"
            });
        }
        catch (CodeMismatchException)
        {
            return Result.Error("Código de confirmação inválido");
        }
        catch (ExpiredCodeException)
        {
            return Result.Error("Código de confirmação expirado");
        }
        catch (InvalidPasswordException ex)
        {
            return Result.Error($"Senha inválida: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resetting password for {Email}", request.Email);
            return Result.Error("Erro ao redefinir senha");
        }
    }
}