using Arda9UserApi.Application.Extensions;
using Arda9UserApi.Application.Services;
using Ardalis.Result;
using MediatR;

namespace Arda9UserApi.Application.Auth.GetUserInfo;

public class GetUserInfoQueryHandler : IRequestHandler<GetUserInfoQuery, Result<UserInfoResponse>>
{
    private readonly IAuthService authService;
    private readonly ILogger<GetUserInfoQueryHandler> logger;

    public GetUserInfoQueryHandler(IAuthService authService, ILogger<GetUserInfoQueryHandler> logger)
    {
        this.authService = authService;
        this.logger = logger;
    }

    public async Task<Result<UserInfoResponse>> Handle(GetUserInfoQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await authService.GetUserAsync(request.AccessToken);

            var userInfo = new UserInfoResponse
            {
                Username = response.Username,
                Email = response.UserAttributes.FirstOrDefault(a => a.Name == "email")?.Value ?? string.Empty,
                Name = response.UserAttributes.FirstOrDefault(a => a.Name == "name")?.Value ?? string.Empty,
                PhoneNumber = response.UserAttributes.FirstOrDefault(a => a.Name == "phone_number")?.Value,
                EmailVerified = bool.Parse(response.UserAttributes.FirstOrDefault(a => a.Name == "email_verified")?.Value ?? "false"),
                Sub = response.UserAttributes.FirstOrDefault(a => a.Name == "sub")?.Value ?? string.Empty
            };

            return Result.Success(userInfo);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user info");
            return ResultError.Error("Erro ao obter informações do usuário");
        }
    }
}