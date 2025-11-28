using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Arda9FileApi.Application.Auth.GetUserInfo;
using Arda9FileApi.Core.Configuration;
using Ardalis.Result;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Arda9FileApi.Application.Services;

public class AuthService : IAuthService
{
    private readonly IAmazonCognitoIdentityProvider _cognitoClient;
    private readonly AwsCognitoConfig _cognitoConfig;
    private readonly ILogger<AuthService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuthService(
        IAmazonCognitoIdentityProvider cognitoClient,
        IOptions<AwsCognitoConfig> cognitoConfig,
        ILogger<AuthService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _cognitoClient = cognitoClient;
        _cognitoConfig = cognitoConfig.Value;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    #region Cognito Authentication Methods

    public string ComputeSecretHash(string username)
    {
        var message = username + _cognitoConfig.ClientId;
        var key = Encoding.UTF8.GetBytes(_cognitoConfig.ClientSecret);
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
            ClientId = _cognitoConfig.ClientId,
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

        var response = await _cognitoClient.SignUpAsync(signUpRequest);
        _logger.LogInformation("User {Email} registered successfully with UserSub {UserSub}", email, response.UserSub);
        
        return response;
    }

    public async Task ConfirmSignUpAsync(string email, string code)
    {
        var secretHash = ComputeSecretHash(email);

        var confirmRequest = new ConfirmSignUpRequest
        {
            ClientId = _cognitoConfig.ClientId,
            SecretHash = secretHash,
            Username = email,
            ConfirmationCode = code
        };

        await _cognitoClient.ConfirmSignUpAsync(confirmRequest);
        _logger.LogInformation("User {Email} confirmed successfully", email);
    }

    public async Task ResendConfirmationCodeAsync(string email)
    {
        var secretHash = ComputeSecretHash(email);

        var resendRequest = new ResendConfirmationCodeRequest
        {
            ClientId = _cognitoConfig.ClientId,
            SecretHash = secretHash,
            Username = email
        };

        await _cognitoClient.ResendConfirmationCodeAsync(resendRequest);
        _logger.LogInformation("Confirmation code resent to {Email}", email);
    }

    public async Task<InitiateAuthResponse> LoginAsync(string email, string password)
    {
        var secretHash = ComputeSecretHash(email);

        var authRequest = new InitiateAuthRequest
        {
            ClientId = _cognitoConfig.ClientId,
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            AuthParameters = new Dictionary<string, string>
            {
                ["USERNAME"] = email,
                ["PASSWORD"] = password,
                ["SECRET_HASH"] = secretHash
            }
        };

        var response = await _cognitoClient.InitiateAuthAsync(authRequest);
        _logger.LogInformation("User {Email} logged in successfully", email);
        
        return response;
    }

    public async Task<InitiateAuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var secretHash = ComputeSecretHash(string.Empty);

        var refreshRequest = new InitiateAuthRequest
        {
            ClientId = _cognitoConfig.ClientId,
            AuthFlow = AuthFlowType.REFRESH_TOKEN_AUTH,
            AuthParameters = new Dictionary<string, string>
            {
                ["REFRESH_TOKEN"] = refreshToken,
                ["SECRET_HASH"] = secretHash
            }
        };

        var response = await _cognitoClient.InitiateAuthAsync(refreshRequest);
        _logger.LogInformation("Token refreshed successfully");
        
        return response;
    }

    public async Task SendForgotPasswordCodeAsync(string email)
    {
        var secretHash = ComputeSecretHash(email);

        var forgotRequest = new ForgotPasswordRequest
        {
            ClientId = _cognitoConfig.ClientId,
            SecretHash = secretHash,
            Username = email
        };

        await _cognitoClient.ForgotPasswordAsync(forgotRequest);
        _logger.LogInformation("Password reset code sent to {Email}", email);
    }

    public async Task ConfirmForgotPasswordAsync(string email, string code, string newPassword)
    {
        var secretHash = ComputeSecretHash(email);

        var confirmRequest = new ConfirmForgotPasswordRequest
        {
            ClientId = _cognitoConfig.ClientId,
            SecretHash = secretHash,
            Username = email,
            ConfirmationCode = code,
            Password = newPassword
        };

        await _cognitoClient.ConfirmForgotPasswordAsync(confirmRequest);
        _logger.LogInformation("Password reset successfully for {Email}", email);
    }

    public async Task ChangePasswordAsync(string accessToken, string oldPassword, string newPassword)
    {
        var changeRequest = new ChangePasswordRequest
        {
            AccessToken = accessToken,
            PreviousPassword = oldPassword,
            ProposedPassword = newPassword
        };

        await _cognitoClient.ChangePasswordAsync(changeRequest);
        _logger.LogInformation("Password changed successfully");
    }

    public async Task GlobalSignOutAsync(string accessToken)
    {
        var signOutRequest = new GlobalSignOutRequest
        {
            AccessToken = accessToken
        };

        await _cognitoClient.GlobalSignOutAsync(signOutRequest);
        _logger.LogInformation("User logged out successfully");
    }

    public async Task<GetUserResponse> GetUserAsync(string accessToken)
    {
        var getUserRequest = new GetUserRequest
        {
            AccessToken = accessToken
        };

        return await _cognitoClient.GetUserAsync(getUserRequest);
    }

    public async Task<UserInfoResponse> GetUserInfoAsync(string accessToken)
    {
        var getUserRequest = new GetUserRequest
        {
            AccessToken = accessToken
        };

        var response = await _cognitoClient.GetUserAsync(getUserRequest);

        var userInfo = new UserInfoResponse
        {
            Username = response.Username,
            Email = response.UserAttributes.FirstOrDefault(a => a.Name == "email")?.Value ?? string.Empty,
            Name = response.UserAttributes.FirstOrDefault(a => a.Name == "name")?.Value ?? string.Empty,
            PhoneNumber = response.UserAttributes.FirstOrDefault(a => a.Name == "phone_number")?.Value,
            EmailVerified = bool.Parse(response.UserAttributes.FirstOrDefault(a => a.Name == "email_verified")?.Value ?? "false"),
            Sub = response.UserAttributes.FirstOrDefault(a => a.Name == "sub")?.Value ?? string.Empty
        };

        return userInfo;
    }

    #endregion

    #region User Context Methods

    public bool IsAuthenticated()
    {
        var userClaims = _httpContextAccessor.HttpContext?.User;
        return userClaims?.Identity?.IsAuthenticated == true;
    }

    public Result<Guid> GetCurrentUserId()
    {
        var userClaims = _httpContextAccessor.HttpContext?.User;
        
        if (userClaims == null || !userClaims.Identity?.IsAuthenticated == true)
        {
            _logger.LogWarning("Usuário não autenticado");
            return Result<Guid>.Unauthorized();
        }

        var userId = userClaims.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? userClaims.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Claim 'sub' não encontrada no token");
            return Result<Guid>.Unauthorized();
        }

        if (!Guid.TryParse(userId, out var userGuid))
        {
            _logger.LogWarning("UserId '{UserId}' não é um GUID válido", userId);
            return Result<Guid>.Invalid(new ValidationError
            {
                ErrorMessage = "UserId inválido"
            });
        }

        return Result<Guid>.Success(userGuid);
    }

    public Result<string> GetCurrentUserEmail()
    {
        var userClaims = _httpContextAccessor.HttpContext?.User;
        
        if (userClaims == null || !userClaims.Identity?.IsAuthenticated == true)
        {
            _logger.LogWarning("Usuário não autenticado");
            return Result<string>.Unauthorized();
        }

        var email = userClaims.FindFirst(ClaimTypes.Email)?.Value
                    ?? userClaims.FindFirst("email")?.Value;

        if (string.IsNullOrEmpty(email))
        {
            _logger.LogWarning("Claim 'email' não encontrada no token");
            return Result<string>.Error();
        }

        return Result<string>.Success(email);
    }

    public Result<string> GetCurrentUserName()
    {
        var userClaims = _httpContextAccessor.HttpContext?.User;
        
        if (userClaims == null || !userClaims.Identity?.IsAuthenticated == true)
        {
            _logger.LogWarning("Usuário não autenticado");
            return Result<string>.Unauthorized();
        }

        var name = userClaims.FindFirst(ClaimTypes.Name)?.Value
                   ?? userClaims.FindFirst("name")?.Value;

        if (string.IsNullOrEmpty(name))
        {
            _logger.LogWarning("Claim 'name' não encontrada no token");
            return Result<string>.Error();
        }

        return Result<string>.Success(name);
    }

    #endregion
}