using Amazon.CognitoIdentityProvider.Model;
using Arda9FileApi.Application.Services;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result<ForgotPasswordResponse>>
{
    private readonly IAuthService authService;
    private readonly ILogger<ForgotPasswordCommandHandler> logger;

    public ForgotPasswordCommandHandler(IAuthService authService, ILogger<ForgotPasswordCommandHandler> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    public async Task<Result<ForgotPasswordResponse>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await authService.SendForgotPasswordCodeAsync(request.Email);

            return Result.Success(new ForgotPasswordResponse
            {
                Success = true,
                Message = "Código de recuperação enviado para seu email"
            });
        }
        catch (UserNotFoundException)
        {
            // Por segurança, retornamos sucesso mesmo se o usuário não existir
            return Result.Success(new ForgotPasswordResponse
            {
                Success = true,
                Message = "Se o email existir, você receberá um código de recuperação"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending password reset code to {Email}", request.Email);
            return Result.Error("Erro ao enviar código de recuperação");
        }
    }
}