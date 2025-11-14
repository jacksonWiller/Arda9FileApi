using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Arda9FileApi.Configuration;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Arda9FileApi.Application.Services;

public interface IAuthService
{
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
}

public class AuthService : IAuthService
{
    private readonly IAmazonCognitoIdentityProvider cognitoClient;
    private readonly AwsCognitoConfig cognitoConfig;
    private readonly ILogger<AuthService> logger;

    public AuthService(
        IAmazonCognitoIdentityProvider cognitoClient,
        IOptions<AwsCognitoConfig> cognitoConfig,
        ILogger<AuthService> logger)
    {
        this.cognitoClient = cognitoClient;
        this.cognitoConfig = cognitoConfig.Value;
        this.logger = logger;
    }

    public string ComputeSecretHash(string username)
    {
        var message = username + cognitoConfig.ClientId;
        var key = Encoding.UTF8.GetBytes(cognitoConfig.ClientSecret);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hash);
    }

    public async Task<SignUpResponse> RegisterUserAsync(string email, string password, string name, string? phoneNumber)
    {
        var secretHash = ComputeSecretHash(email);

        var signUpRequest = new SignUpRequest
        {
            ClientId = cognitoConfig.ClientId,
            SecretHash = secretHash,
            Username = email,
            Password = password,
            UserAttributes = new List<AttributeType>
            {
                new() { Name = "email", Value = email },
                new() { Name = "name", Value = name }
            }
        };

        if (!string.IsNullOrEmpty(phoneNumber))
        {
            signUpRequest.UserAttributes.Add(new AttributeType
            {
                Name = "phone_number",
                Value = phoneNumber
            });
        }

        var response = await cognitoClient.SignUpAsync(signUpRequest);
        logger.LogInformation("User {Email} registered successfully with UserSub {UserSub}", email, response.UserSub);
        
        return response;
    }

    public async Task ConfirmSignUpAsync(string email, string code)
    {
        var secretHash = ComputeSecretHash(email);

        var confirmRequest = new ConfirmSignUpRequest
        {
            ClientId = cognitoConfig.ClientId,
            SecretHash = secretHash,
            Username = email,
            ConfirmationCode = code
        };

        await cognitoClient.ConfirmSignUpAsync(confirmRequest);
        logger.LogInformation("User {Email} confirmed successfully", email);
    }

    public async Task ResendConfirmationCodeAsync(string email)
    {
        var secretHash = ComputeSecretHash(email);

        var resendRequest = new ResendConfirmationCodeRequest
        {
            ClientId = cognitoConfig.ClientId,
            SecretHash = secretHash,
            Username = email
        };

        await cognitoClient.ResendConfirmationCodeAsync(resendRequest);
        logger.LogInformation("Confirmation code resent to {Email}", email);
    }

    public async Task<InitiateAuthResponse> LoginAsync(string email, string password)
    {
        var secretHash = ComputeSecretHash(email);

        var authRequest = new InitiateAuthRequest
        {
            ClientId = cognitoConfig.ClientId,
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            AuthParameters = new Dictionary<string, string>
            {
                ["USERNAME"] = email,
                ["PASSWORD"] = password,
                ["SECRET_HASH"] = secretHash
            }
        };

        var response = await cognitoClient.InitiateAuthAsync(authRequest);
        logger.LogInformation("User {Email} logged in successfully", email);
        
        return response;
    }

    public async Task<InitiateAuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var secretHash = ComputeSecretHash(string.Empty);

        var refreshRequest = new InitiateAuthRequest
        {
            ClientId = cognitoConfig.ClientId,
            AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
            AuthParameters = new Dictionary<string, string>
            {
                ["REFRESH_TOKEN"] = refreshToken,
                ["SECRET_HASH"] = secretHash
            }
        };

        var response = await cognitoClient.InitiateAuthAsync(refreshRequest);
        logger.LogInformation("Token refreshed successfully");
        
        return response;
    }

    public async Task SendForgotPasswordCodeAsync(string email)
    {
        var secretHash = ComputeSecretHash(email);

        var forgotRequest = new ForgotPasswordRequest
        {
            ClientId = cognitoConfig.ClientId,
            SecretHash = secretHash,
            Username = email
        };

        await cognitoClient.ForgotPasswordAsync(forgotRequest);
        logger.LogInformation("Password reset code sent to {Email}", email);
    }

    public async Task ConfirmForgotPasswordAsync(string email, string code, string newPassword)
    {
        var secretHash = ComputeSecretHash(email);

        var confirmRequest = new ConfirmForgotPasswordRequest
        {
            ClientId = cognitoConfig.ClientId,
            SecretHash = secretHash,
            Username = email,
            ConfirmationCode = code,
            Password = newPassword
        };

        await cognitoClient.ConfirmForgotPasswordAsync(confirmRequest);
        logger.LogInformation("Password reset successfully for {Email}", email);
    }

    public async Task ChangePasswordAsync(string accessToken, string oldPassword, string newPassword)
    {
        var changeRequest = new ChangePasswordRequest
        {
            AccessToken = accessToken,
            PreviousPassword = oldPassword,
            ProposedPassword = newPassword
        };

        await cognitoClient.ChangePasswordAsync(changeRequest);
        logger.LogInformation("Password changed successfully");
    }

    public async Task GlobalSignOutAsync(string accessToken)
    {
        var signOutRequest = new GlobalSignOutRequest
        {
            AccessToken = accessToken
        };

        await cognitoClient.GlobalSignOutAsync(signOutRequest);
        logger.LogInformation("User logged out successfully");
    }

    public async Task<GetUserResponse> GetUserAsync(string accessToken)
    {
        var getUserRequest = new GetUserRequest
        {
            AccessToken = accessToken
        };

        return await cognitoClient.GetUserAsync(getUserRequest);
    }
}