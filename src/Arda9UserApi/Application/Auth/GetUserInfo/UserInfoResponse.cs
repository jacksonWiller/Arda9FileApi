namespace Arda9UserApi.Application.Auth.GetUserInfo;

public class UserInfoResponse
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailVerified { get; set; }
    public string Sub { get; set; } = string.Empty;
}