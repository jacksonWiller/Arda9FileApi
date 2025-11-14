using Arda9UserApi.Core;
using Arda9UserApi.Domain.Entities.UserAggregate.Events;
using Catalog.Core.SharedKernel;
using Catalog.Domain.Entities.UserAggregate.Events;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Arda9UserApi.Domain.Entities.UserAggregate;

public class User : BaseEntity, IAggregateRoot
{
    public Guid CompanyId { get; private set; }
    public Guid? SubCompanyId { get; private set; }
    public Email Email { get; private set; }
    public PersonName Name { get; private set; }
    public Phone? Phone { get; private set; }
    public UserStatus Status { get; private set; }
    public List<Guid> Roles { get; private set; }
    public Url? PictureUrl { get; private set; }
    public string? Locale { get; private set; }
    public string? CognitoSub { get; private set; }
    public string? Username { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid? CreatedBy { get; private set; }
    public Guid? UpdatedBy { get; private set; }

    public User()
    {
        Email = new Email("default@example.com");
        Name = new PersonName("Default User");
        Roles = new List<Guid>();
        Status = UserStatus.ACTIVE;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public User(
        Guid companyId,
        Email email,
        PersonName name,
        Phone? phone = null,
        List<Guid>? roles = null,
        Url? pictureUrl = null,
        string? locale = null,
        string? cognitoSub = null,
        string? username = null,
        Guid? subCompanyId = null,
        Guid? createdBy = null)
    {
        if (companyId == Guid.Empty)
            throw new ArgumentException("CompanyId cannot be empty", nameof(companyId));

        CompanyId = companyId;
        SubCompanyId = subCompanyId;
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Phone = phone;
        Roles = ValidateRoles(roles ?? new List<Guid>());
        PictureUrl = pictureUrl;
        Locale = ValidateLocale(locale);
        CognitoSub = cognitoSub;
        Username = username;
        Status = string.IsNullOrEmpty(cognitoSub) ? UserStatus.PENDING_VERIFICATION : UserStatus.ACTIVE;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        CreatedBy = createdBy;

        AddDomainEvent(new UserCreatedEvent(this));
    }

    public void Update(
        PersonName name,
        Phone? phone = null,
        Url? pictureUrl = null,
        string? locale = null,
        Guid? updatedBy = null)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Phone = phone;
        PictureUrl = pictureUrl;
        Locale = ValidateLocale(locale);
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        AddDomainEvent(new UserUpdatedEvent(this));
    }

    public void UpdateRoles(List<Guid> roles, Guid? updatedBy = null)
    {
        var oldRoles = new List<Guid>(Roles);
        Roles = ValidateRoles(roles ?? new List<Guid>());
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        AddDomainEvent(new UserRolesChangedEvent(this, oldRoles, Roles));
    }

    public void AssignRole(Guid roleId, Guid? updatedBy = null)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("RoleId cannot be empty", nameof(roleId)); 

        if (Roles.Contains(roleId))
            return; // Already has this role

        var oldRoles = new List<Guid>(Roles);
        Roles.Add(roleId);
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        AddDomainEvent(new UserRolesChangedEvent(this, oldRoles, Roles));
    }

    public void RemoveRole(Guid roleId, Guid? updatedBy = null)
    {
        if (roleId == Guid.Empty)
            throw new ArgumentException("RoleId cannot be empty", nameof(roleId));

        if (!Roles.Contains(roleId))
            return; // Doesn't have this role

        var oldRoles = new List<Guid>(Roles);
        Roles.Remove(roleId);
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        AddDomainEvent(new UserRolesChangedEvent(this, oldRoles, Roles));
    }

    public void LinkCognitoAccount(string cognitoSub, string username, Guid? updatedBy = null)
    {
        if (string.IsNullOrWhiteSpace(cognitoSub))
            throw new ArgumentException("CognitoSub cannot be null or whitespace", nameof(cognitoSub));

        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username cannot be null or whitespace", nameof(username));

        CognitoSub = cognitoSub;
        Username = username;

        if (Status == UserStatus.PENDING_VERIFICATION)
        {
            Status = UserStatus.ACTIVE;
            AddDomainEvent(new UserEmailVerifiedEvent(this));
        }

        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        AddDomainEvent(new UserUpdatedEvent(this));
    }

    public void VerifyEmail(Guid? verifiedBy = null)
    {
        if (Status != UserStatus.PENDING_VERIFICATION)
            return; // Already verified or in different status

        Status = UserStatus.ACTIVE;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = verifiedBy;

        AddDomainEvent(new UserEmailVerifiedEvent(this));
    }

    public void Suspend(string reason, Guid? suspendedBy = null)
    {
        if (Status == UserStatus.DISABLED)
            throw new InvalidOperationException("Cannot suspend a disabled user");

        Status = UserStatus.SUSPENDED;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = suspendedBy;

        AddDomainEvent(new UserSuspendedEvent(this, reason));
    }

    public void Resume(Guid? resumedBy = null)
    {
        if (Status != UserStatus.SUSPENDED)
            throw new InvalidOperationException("Only suspended users can be resumed");

        Status = UserStatus.ACTIVE;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = resumedBy;

        AddDomainEvent(new UserResumedEvent(this));
    }

    public void Disable(Guid? disabledBy = null)
    {
        if (Status == UserStatus.DISABLED)
            return; // Already disabled

        Status = UserStatus.DISABLED;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = disabledBy;

        AddDomainEvent(new UserDisabledEvent(this));
    }

    public void ChangeSubCompany(Guid? subCompanyId, Guid? updatedBy = null)
    {
        SubCompanyId = subCompanyId;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;

        AddDomainEvent(new UserUpdatedEvent(this));
    }

    private static List<Guid> ValidateRoles(List<Guid> roles)
    {
        if (roles.Count > 10)
            throw new ArgumentException("User cannot have more than 10 roles");

        var validated = roles
            .Where(r => r != Guid.Empty)
            .Distinct()
            .ToList();

        return validated;
    }

    private static string? ValidateLocale(string? locale)
    {
        if (string.IsNullOrWhiteSpace(locale))
            return null;

        var normalized = locale.Trim();

        var validLocales = new[] { "pt-BR", "en-US", "es-ES", "fr-FR" };

        if (!validLocales.Contains(normalized))
            throw new ArgumentException($"Locale must be one of: {string.Join(", ", validLocales)}");

        return normalized;
    }
}