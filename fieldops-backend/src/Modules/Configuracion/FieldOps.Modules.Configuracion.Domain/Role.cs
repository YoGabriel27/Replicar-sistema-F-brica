using FieldOps.Shared.Kernel;

namespace FieldOps.Modules.Configuracion.Domain;

public sealed class Role : Entity, ITenantOwned
{
    public Guid CompanyId { get; private set; }
    public string Name { get; private set; } = default!;

    private readonly List<Guid> _permissionIds = [];
    public IReadOnlyCollection<Guid> PermissionIds => _permissionIds.AsReadOnly();

    private Role() { }

    public static Role Create(Guid companyId, string name, Guid actorUserId)
    {
        var role = new Role { CompanyId = companyId, Name = name };
        role.MarkUpdated(actorUserId);
        return role;
    }

    /// <summary>Permission se agrupa siempre en Role — nunca se asigna
    /// un permiso individual suelto a un User (regla #4 del módulo).</summary>
    public void GrantPermission(Guid permissionId, Guid actorUserId)
    {
        if (!_permissionIds.Contains(permissionId))
        {
            _permissionIds.Add(permissionId);
            MarkUpdated(actorUserId);
        }
    }
}
