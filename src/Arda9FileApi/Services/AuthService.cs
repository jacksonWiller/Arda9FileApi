using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Arda9FileApi.Models.Auth;

namespace Arda9FileApi.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuthService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthService(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuthService> logger)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public void SetAccessToken(string accessToken)
    {
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);
    }

    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Registrando novo usuário: {Email}", request.Email);

            var response = await PostAsync<RegisterRequest, RegisterResponse>(
                "/api/Auth/register",
                request,
                cancellationToken);

            _logger.LogInformation("Usuário registrado com sucesso: {Email}", request.Email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário: {Email}", request.Email);
            throw;
        }
    }

    public async Task<AuthResponse> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Confirmando email: {Email}", request.Email);

            var response = await PostAsync<ConfirmEmailRequest, AuthResponse>(
                "/api/Auth/confirm",
                request,
                cancellationToken);

            _logger.LogInformation("Email confirmado com sucesso: {Email}", request.Email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao confirmar email: {Email}", request.Email);
            throw;
        }
    }

    public async Task<AuthResponse> ResendCodeAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Reenviando código de confirmação: {Email}", email);

            var response = await PostAsync<object, AuthResponse>(
                "/api/Auth/resend-code",
                new { email },
                cancellationToken);

            _logger.LogInformation("Código reenviado com sucesso: {Email}", email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao reenviar código: {Email}", email);
            throw;
        }
    }

    public async Task<LoginResponse> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Realizando login: {Email}", request.Email);

            var response = await PostAsync<LoginRequest, LoginResponse>(
                "/api/Auth/login",
                request,
                cancellationToken);

            if (response.Success && !string.IsNullOrEmpty(response.AccessToken))
            {
                SetAccessToken(response.AccessToken);
            }

            _logger.LogInformation("Login realizado com sucesso: {Email}", request.Email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar login: {Email}", request.Email);
            throw;
        }
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Atualizando token de acesso");

            var response = await PostAsync<RefreshTokenRequest, RefreshTokenResponse>(
                "/api/Auth/refresh",
                request,
                cancellationToken);

            if (response.Success && !string.IsNullOrEmpty(response.AccessToken))
            {
                SetAccessToken(response.AccessToken);
            }

            _logger.LogInformation("Token atualizado com sucesso");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar token");
            throw;
        }
    }

    public async Task<AuthResponse> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Solicitando recuperação de senha: {Email}", request.Email);

            var response = await PostAsync<ForgotPasswordRequest, AuthResponse>(
                "/api/Auth/forgot-password",
                request,
                cancellationToken);

            _logger.LogInformation("Código de recuperação enviado: {Email}", request.Email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao solicitar recuperação de senha: {Email}", request.Email);
            throw;
        }
    }

    public async Task<AuthResponse> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Resetando senha: {Email}", request.Email);

            var response = await PostAsync<ResetPasswordRequest, AuthResponse>(
                "/api/Auth/reset-password",
                request,
                cancellationToken);

            _logger.LogInformation("Senha resetada com sucesso: {Email}", request.Email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao resetar senha: {Email}", request.Email);
            throw;
        }
    }

    public async Task<AuthResponse> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Alterando senha do usuário autenticado");

            var response = await PostAsync<ChangePasswordRequest, AuthResponse>(
                "/api/Auth/change-password",
                request,
                cancellationToken);

            _logger.LogInformation("Senha alterada com sucesso");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar senha");
            throw;
        }
    }

    public async Task<AuthResponse> LogoutAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Realizando logout");

            var response = await PostAsync<object, AuthResponse>(
                "/api/Auth/logout",
                new { },
                cancellationToken);

            _httpClient.DefaultRequestHeaders.Authorization = null;

            _logger.LogInformation("Logout realizado com sucesso");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar logout");
            throw;
        }
    }

    public async Task<UserInfoResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Obtendo informações do usuário autenticado");

            EnsureAccessTokenIsSet();

            var response = await _httpClient.GetAsync("/api/Auth/me", cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var userInfo = JsonSerializer.Deserialize<UserInfoResponse>(content, _jsonOptions);

            if (userInfo == null)
            {
                throw new InvalidOperationException("Resposta inválida da API");
            }

            _logger.LogInformation("Informações do usuário obtidas com sucesso: {Email}", userInfo.Email);
            return userInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter informações do usuário");
            throw;
        }
    }

    private void EnsureAccessTokenIsSet()
    {
        var token = GetAccessTokenFromContext();
        if (!string.IsNullOrEmpty(token))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
        }
    }

    private string? GetAccessTokenFromContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        var authHeader = httpContext.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader))
        {
            return null;
        }

        // Remove "Bearer " do início se presente
        return authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authHeader.Substring(7)
            : authHeader;
    }

    private async Task<TResponse> PostAsync<TRequest, TResponse>(
        string endpoint,
        TRequest request,
        CancellationToken cancellationToken)
    {
        EnsureAccessTokenIsSet();

        var json = JsonSerializer.Serialize(request, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var result = JsonSerializer.Deserialize<TResponse>(responseContent, _jsonOptions);

        if (result == null)
        {
            throw new InvalidOperationException("Resposta inválida da API");
        }

        return result;
    }
}