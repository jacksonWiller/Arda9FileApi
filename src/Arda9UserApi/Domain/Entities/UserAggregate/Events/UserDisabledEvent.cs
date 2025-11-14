using Arda9UserApi.Core;
using Arda9UserApi.Domain.Entities.UserAggregate;

namespace Catalog.Domain.Entities.UserAggregate.Events;

public class UserDisabledEvent : BaseEvent
{
    public User User { get; }

    public UserDisabledEvent(User user)
    {
        User = user;
    }
}