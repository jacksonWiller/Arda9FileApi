using Amazon.CognitoIdentityProvider.Model;
using Arda9FileApi.Application.Services;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.ConfirmEmail;

public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommand, Result<ConfirmEmailResponse>>
{
    private readonly IAuthService authService;
    private readonly ILogger<ConfirmEmailCommandHandler> logger;

    public ConfirmEmailCommandHandler(IAuthService authService, ILogger<ConfirmEmailCommandHandler> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    public async Task<Result<ConfirmEmailResponse>> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await authService.ConfirmSignUpAsync(request.Email, request.Code);

            return Result.Success(new ConfirmEmailResponse
            {
                Success = true,
                Message = "Email confirmado com sucesso"
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error confirming email for user {Email}", request.Email);
            return Result.Error("Erro ao confirmar email");
        }
    }
}