using Ardalis.Result;

namespace Arda9FileApi.Application.Services;

public interface IUserContextService
{
    Result<Guid> GetCurrentUserId();
    Result<string> GetCurrentUserEmail();
    bool IsAuthenticated();
}