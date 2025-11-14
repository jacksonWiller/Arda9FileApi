using Arda9FileApi.Domain.Entities.UserAggregate;
using Arda9FileApi.Core;

namespace Arda9FileApi.Domain.Entities.UserAggregate.Events;

public class UserUpdatedEvent : BaseEvent
{
    public User User { get; }

    public UserUpdatedEvent(User user)
    {
        User = user;
    }
}