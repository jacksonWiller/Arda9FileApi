namespace Arda9FileApi.Models.Auth;

public class RegisterResponse
{
    public bool Success { get; set; }
    public string UserSub { get; set; } = string.Empty;
    public bool UserConfirmed { get; set; }
    public string Message { get; set; } = string.Empty;
}