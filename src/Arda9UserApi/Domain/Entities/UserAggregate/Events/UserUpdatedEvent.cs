using Arda9UserApi.Core;

namespace Arda9UserApi.Domain.Entities.UserAggregate.Events;

public class UserUpdatedEvent : BaseEvent
{
    public User User { get; }

    public UserUpdatedEvent(User user)
    {
        User = user;
    }
}