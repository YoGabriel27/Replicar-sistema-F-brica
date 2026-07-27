using FieldOps.Shared.Kernel;

namespace FieldOps.Modules.Configuracion.Domain;

/// <summary>
/// Administra una o más Company bajo un mismo contrato de reventa
/// (06-Modules/07-Configuracion/README.md — decisión confirmada: modelo
/// de reventa/partner en el MVP). Es opcional: una Company puede no
/// pertenecer a ningún Partner.
/// </summary>
public sealed class Partner : Entity
{
    public string Name { get; private set; } = default!;

    private Partner() { }

    public static Partner Create(string name, Guid actorUserId)
    {
        var partner = new Partner { Name = name };
        partner.MarkUpdated(actorUserId);
        return partner;
    }
}
