using FieldOps.Shared.Kernel;

namespace FieldOps.Modules.Configuracion.Domain.Events;

// Nombres tal como se fijaron en 06-Modules/07-Configuracion/README.md
// §Events — no se renombran al implementar.

public sealed record CompanyCreated(Guid CompanyId, string Name, Guid? PartnerId) : DomainEvent;

public sealed record PartnerCreated(Guid PartnerId, string Name) : DomainEvent;

public sealed record CompanyLinkedToPartner(Guid CompanyId, Guid PartnerId) : DomainEvent;

public sealed record UserInvited(Guid UserId, string Email, Guid CompanyId, Guid RoleId) : DomainEvent;

public sealed record MembershipCreated(Guid MembershipId, Guid UserId, Guid CompanyId, Guid RoleId) : DomainEvent;

public sealed record MembershipRevoked(Guid MembershipId, Guid UserId, Guid CompanyId) : DomainEvent;

public sealed record UserActivated(Guid UserId) : DomainEvent;

public sealed record UserDeactivated(Guid UserId) : DomainEvent;

public sealed record RoleAssigned(Guid MembershipId, Guid RoleId) : DomainEvent;

public sealed record FeatureFlagToggled(Guid CompanyId, string Key, bool IsEnabled) : DomainEvent;
