using Arda9FileApi.Domain.Entities.UserAggregate;
using Arda9FileApi.Core;

namespace Arda9FileApi.Domain.Entities.UserAggregate.Events;

public class UserDisabledEvent : BaseEvent
{
    public User User { get; }

    public UserDisabledEvent(User user)
    {
        User = user;
    }
}