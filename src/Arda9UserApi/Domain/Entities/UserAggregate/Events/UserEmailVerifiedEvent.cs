using Arda9UserApi.Core;
using Arda9UserApi.Domain.Entities.UserAggregate;

namespace Catalog.Domain.Entities.UserAggregate.Events;

public class UserEmailVerifiedEvent : BaseEvent
{
    public User User { get; }

    public UserEmailVerifiedEvent(User user)
    {
        User = user;
    }
}