using Arda9FileApi.Application.Services;
using Ardalis.Result;
using MediatR;

namespace Arda9FileApi.Application.Auth.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result<LogoutResponse>>
{
    private readonly IAuthService authService;
    private readonly ILogger<LogoutCommandHandler> logger;

    public LogoutCommandHandler(IAuthService authService, ILogger<LogoutCommandHandler> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    public async Task<Result<LogoutResponse>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await authService.GlobalSignOutAsync(request.AccessToken);

            return Result.Success(new LogoutResponse
            {
                Success = true,
                Message = "Logout realizado com sucesso"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during logout");
            return Result.Error();
        }
    }
}