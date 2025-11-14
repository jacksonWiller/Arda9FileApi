using Arda9FileApi.Domain.Entities.UserAggregate;
using Arda9UserApi.Core;

namespace Catalog.Domain.Entities.UserAggregate.Events;

public class UserEmailVerifiedEvent : BaseEvent
{
    public User User { get; }

    public UserEmailVerifiedEvent(User user)
    {
        User = user;
    }
}