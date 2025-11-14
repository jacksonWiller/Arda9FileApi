using Arda9FileApi.Domain.Entities.UserAggregate;
using Arda9UserApi.Core;

namespace Catalog.Domain.Entities.UserAggregate.Events;

public class UserRolesChangedEvent : BaseEvent
{
    public User User { get; }
    public List<Guid> OldRoles { get; }
    public List<Guid> NewRoles { get; }

    public UserRolesChangedEvent(User user, List<Guid> oldRoles, List<Guid> newRoles)
    {
        User = user;
        OldRoles = oldRoles;
        NewRoles = newRoles;
    }
}