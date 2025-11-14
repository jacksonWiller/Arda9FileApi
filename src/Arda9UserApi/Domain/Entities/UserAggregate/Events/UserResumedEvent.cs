using Arda9FileApi.Domain.Entities.UserAggregate;
using Arda9UserApi.Core;

namespace Catalog.Domain.Entities.UserAggregate.Events;

public class UserResumedEvent : BaseEvent
{
    public User User { get; }

    public UserResumedEvent(User user)
    {
        User = user;
    }
}