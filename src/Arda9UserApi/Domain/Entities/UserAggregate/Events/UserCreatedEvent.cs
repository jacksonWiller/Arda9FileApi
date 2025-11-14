using Arda9UserApi.Core;
using Arda9UserApi.Domain.Entities.UserAggregate;

namespace Catalog.Domain.Entities.UserAggregate.Events;

public class UserCreatedEvent : BaseEvent
{
    public User User { get; }

    public UserCreatedEvent(User user)
    {
        User = user;
    }
}