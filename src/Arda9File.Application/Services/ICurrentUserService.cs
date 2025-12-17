namespace Arda9FileApi.Services;

public interface ICurrentUserService
{
    Guid GetTenantId();
    string? GetUserId();
    string? GetUserEmail();
    bool IsAuthenticated();
}