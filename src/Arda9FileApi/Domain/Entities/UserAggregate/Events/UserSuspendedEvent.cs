using Arda9FileApi.Domain.Entities.UserAggregate;
using Arda9FileApi.Core;

namespace Arda9FileApi.Domain.Entities.UserAggregate.Events;

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