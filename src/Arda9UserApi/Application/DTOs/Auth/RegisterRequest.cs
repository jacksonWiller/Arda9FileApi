namespace Arda9UserApi.Application.DTOs;

public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
}

public class RegisterResponse
{
    public bool Success { get; set; }
    public string UserSub { get; set; } = string.Empty;
    public bool UserConfirmed { get; set; }
    public string Message { get; set; } = string.Empty;
}