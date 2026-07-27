using FieldOps.Shared.Kernel;

namespace FieldOps.Modules.Configuracion.Domain;

/// <summary>
/// Vincula un User a una Company con un Role ESPECÍFICO para esa empresa
/// — el rol vive en el Membership, no globalmente en el User
/// (06-Modules/07-Configuracion/README.md, Relationships). Un mismo User
/// puede ser Admin en una Company y Supervisor en otra.
/// </summary>
public sealed class Membership : Entity, ITenantOwned
{
    public Guid UserId { get; private set; }
    public Guid CompanyId { get; private set; }
    public Guid RoleId { get; private set; }
    public DateTimeOffset? RevokedAt { get; private set; }
    public bool IsActive => RevokedAt is null;

    private Membership() { }

    public static Membership Create(Guid userId, Guid companyId, Guid roleId, Guid actorUserId)
    {
        var membership = new Membership
        {
            UserId = userId,
            CompanyId = companyId,
            RoleId = roleId,
        };
        membership.MarkUpdated(actorUserId);
        return membership;
    }

    /// <summary>Más granular que DeactivateUser: quita acceso a UNA
    /// Company sin afectar memberships del mismo User en otras (regla #5
    /// del módulo).</summary>
    public void Revoke(Guid actorUserId)
    {
        RevokedAt = DateTimeOffset.UtcNow;
        MarkUpdated(actorUserId);
    }
}
