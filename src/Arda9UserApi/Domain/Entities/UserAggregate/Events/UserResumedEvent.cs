using Arda9UserApi.Core;
using Arda9UserApi.Domain.Entities.UserAggregate;

namespace Catalog.Domain.Entities.UserAggregate.Events;

public class UserResumedEvent : BaseEvent
{
    public User User { get; }

    public UserResumedEvent(User user)
    {
        User = user;
    }
}