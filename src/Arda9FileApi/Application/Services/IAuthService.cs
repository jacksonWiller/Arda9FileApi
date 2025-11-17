using Amazon.CognitoIdentityProvider.Model;
using Arda9FileApi.Application.Auth.GetUserInfo;
using Ardalis.Result;

namespace Arda9FileApi.Application.Services;

public interface IAuthService
{
    // Cognito Authentication Methods
    string ComputeSecretHash(string username);
    Task<SignUpResponse> RegisterUserAsync(string email, string password, string name, string? phoneNumber);
    Task ConfirmSignUpAsync(string email, string code);
    Task ResendConfirmationCodeAsync(string email);
    Task<InitiateAuthResponse> LoginAsync(string email, string password);
    Task<InitiateAuthResponse> RefreshTokenAsync(string refreshToken);
    Task SendForgotPasswordCodeAsync(string email);
    Task ConfirmForgotPasswordAsync(string email, string code, string newPassword);
    Task ChangePasswordAsync(string accessToken, string oldPassword, string newPassword);
    Task GlobalSignOutAsync(string accessToken);
    Task<GetUserResponse> GetUserAsync(string accessToken);
    Task<UserInfoResponse> GetUserInfoAsync(string accessToken);

    // User Context Methods
    Result<Guid> GetCurrentUserId();
    Result<string> GetCurrentUserEmail();
    Result<string> GetCurrentUserName();
    bool IsAuthenticated();
}