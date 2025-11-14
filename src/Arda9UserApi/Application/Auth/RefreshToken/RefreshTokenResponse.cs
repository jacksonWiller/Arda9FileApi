namespace Arda9UserApi.Application.Auth.RefreshToken;

public class RefreshTokenResponse
{
    public bool Success { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string IdToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = string.Empty;
}