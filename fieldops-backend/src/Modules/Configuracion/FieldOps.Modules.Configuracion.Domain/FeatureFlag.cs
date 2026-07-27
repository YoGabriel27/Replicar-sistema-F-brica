using FieldOps.Shared.Kernel;

namespace FieldOps.Modules.Configuracion.Domain;

/// <summary>
/// Refleja los límites del plan de la Company (02-Business/README.md).
/// Se recalcula automáticamente al cambiar de plan — nunca se edita a
/// mano salvo por soporte de plataforma en un caso excepcional auditado
/// (regla #3 del módulo, ver también 13-Security/README.md §Auditoría).
/// </summary>
public sealed class FeatureFlag : Entity, ITenantOwned
{
    public Guid CompanyId { get; private set; }
    public string Key { get; private set; } = default!;
    public bool IsEnabled { get; private set; }

    private FeatureFlag() { }

    public static FeatureFlag Create(Guid companyId, string key, bool isEnabled, Guid actorUserId)
    {
        var flag = new FeatureFlag { CompanyId = companyId, Key = key, IsEnabled = isEnabled };
        flag.MarkUpdated(actorUserId);
        return flag;
    }

    public void Toggle(bool isEnabled, Guid actorUserId)
    {
        IsEnabled = isEnabled;
        MarkUpdated(actorUserId);
    }
}
