using Arda9FileApi.Domain.Entities.UserAggregate;
using Arda9UserApi.Core;

namespace Catalog.Domain.Entities.UserAggregate.Events;

public class UserCreatedEvent : BaseEvent
{
    public User User { get; }

    public UserCreatedEvent(User user)
    {
        User = user;
    }
}