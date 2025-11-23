using Arda9FileApi.Application.Services;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.ResendCode;

public class ResendCodeCommandHandler : IRequestHandler<ResendCodeCommand, Result<ResendCodeResponse>>
{
    private readonly IAuthService authService;
    private readonly ILogger<ResendCodeCommandHandler> logger;

    public ResendCodeCommandHandler(IAuthService authService, ILogger<ResendCodeCommandHandler> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    public async Task<Result<ResendCodeResponse>> Handle(ResendCodeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await authService.ResendConfirmationCodeAsync(request.Email);

            return Result.Success(new ResendCodeResponse
            {
                Success = true,
                Message = "Código de confirmação reenviado"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error resending confirmation code to {Email}", request.Email);
            return Result.Error("Erro ao reenviar código");
        }
    }
}