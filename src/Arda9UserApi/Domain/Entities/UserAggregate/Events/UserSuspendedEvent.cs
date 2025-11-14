using Arda9FileApi.Domain.Entities.UserAggregate;
using Arda9UserApi.Core;

namespace Arda9UserApi.Domain.Entities.UserAggregate.Events;

public class UserSuspendedEvent : BaseEvent
{
    public User User { get; }
    public string Reason { get; }

    public UserSuspendedEvent(User user, string reason)
    {
        User = user;
        Reason = reason;
    }
}