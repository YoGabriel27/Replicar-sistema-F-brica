using FieldOps.Shared.Kernel;

namespace FieldOps.Modules.Configuracion.Domain;

/// <summary>Catálogo global — no lleva CompanyId (no es ITenantOwned):
/// las acciones del sistema son las mismas para todas las empresas,
/// lo que varía por empresa es qué Role las agrupa.</summary>
public sealed class Permission : Entity
{
    public string Code { get; private set; } = default!;   // p.ej. "operaciones.workorder.close"
    public string Description { get; private set; } = default!;

    private Permission() { }

    public static Permission Create(string code, string description, Guid actorUserId)
    {
        var permission = new Permission { Code = code, Description = description };
        permission.MarkUpdated(actorUserId);
        return permission;
    }
}
