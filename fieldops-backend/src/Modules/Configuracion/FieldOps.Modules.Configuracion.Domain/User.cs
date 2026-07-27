using FieldOps.Shared.Kernel;

namespace FieldOps.Modules.Configuracion.Domain;

/// <summary>
/// Un User puede tener acceso a más de una Company vía Membership
/// (06-Modules/07-Configuracion/README.md regla #1 — reemplaza el 1:1
/// estricto original). El aislamiento de datos sigue siendo por Company
/// ACTIVA en la sesión, resuelta por FieldOps.Shared.Tenancy, no por
/// esta entidad.
/// </summary>
public sealed class User : Entity
{
    public string Email { get; private set; } = default!;
    public bool IsActive { get; private set; } = true;

    private readonly List<Membership> _memberships = [];
    public IReadOnlyCollection<Membership> Memberships => _memberships.AsReadOnly();

    private User() { }

    public static User Invite(string email, Guid actorUserId)
    {
        var user = new User { Email = email, IsActive = false };
        user.MarkUpdated(actorUserId);
        return user;
    }

    public void Activate(Guid actorUserId)
    {
        IsActive = true;
        MarkUpdated(actorUserId);
    }

    /// <summary>Soft — revoca acceso, no borra la cuenta ni su historial
    /// de auditoría en otros módulos (regla #5 del módulo).</summary>
    public void Deactivate(Guid actorUserId)
    {
        IsActive = false;
        MarkUpdated(actorUserId);
    }
}
