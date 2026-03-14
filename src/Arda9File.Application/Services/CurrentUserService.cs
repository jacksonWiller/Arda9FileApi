using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Arda9File.Application.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid GetTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null)
        {
            return Guid.Empty;
        }

        // Tentar buscar do custom attribute primeiro
        var tenantIdClaim = httpContext.User.FindFirst("custom:tenantId")?.Value;

        if (!string.IsNullOrEmpty(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            return tenantId;
        }

        // Extrair do username do Cognito (formato: {tenancyId}#{email})
        var username = httpContext.User.FindFirst("cognito:username")?.Value 
                      ?? httpContext.User.FindFirst("username")?.Value;

        if (string.IsNullOrEmpty(username))
        {
            return Guid.Empty;
        }

        // Parse do formato: 3590021ab8114b6e97f6bc385072b127#jacksonwillerduarte@gmail.com
        var parts = username.Split('#');
        if (parts.Length != 2)
        {
            return Guid.Empty;
        }

        var tenancyIdString = parts[0];

        if (string.IsNullOrEmpty(tenancyIdString) || !Guid.TryParse(tenancyIdString, out var tenancyId))
        {
            return Guid.Empty;
        }

        return tenancyId;
    }

    public string? GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null)
        {
            return null;
        }

        // Cognito usa 'sub' como identificador único do usuário
        return httpContext.User.FindFirst("sub")?.Value 
            ?? httpContext.User.FindFirst("cognito:username")?.Value
            ?? httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public string? GetUserEmail()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User == null)
        {
            return null;
        }

        return httpContext.User.FindFirst(ClaimTypes.Email)?.Value
            ?? httpContext.User.FindFirst("email")?.Value;
    }

    public bool IsAuthenticated()
    {
        return _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
    }
}