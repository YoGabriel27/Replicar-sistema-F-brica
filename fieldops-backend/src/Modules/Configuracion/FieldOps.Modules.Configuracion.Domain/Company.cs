using FieldOps.Shared.Kernel;

namespace FieldOps.Modules.Configuracion.Domain;

/// <summary>
/// El tenant — límite de aislamiento de todo el sistema
/// (05-Database/README.md §Estrategia multiempresa). Puede pertenecer
/// opcionalmente a un Partner (reventa).
/// </summary>
public sealed class Company : Entity
{
    public string Name { get; private set; } = default!;
    public Guid? PartnerId { get; private set; }

    /// <summary>Lanzamiento inicial confirmado: Argentina / ARS
    /// (05-Database/README.md §Multimoneda / multipaís) — default de
    /// onboarding, no un valor fijo en el esquema.</summary>
    public string CountryCode { get; private set; } = "AR";
    public string DefaultCurrency { get; private set; } = "ARS";

    private Company() { }

    public static Company Create(
        string name,
        Guid actorUserId,
        Guid? partnerId = null,
        string countryCode = "AR",
        string defaultCurrency = "ARS")
    {
        var company = new Company
        {
            Name = name,
            PartnerId = partnerId,
            CountryCode = countryCode,
            DefaultCurrency = defaultCurrency,
        };
        company.MarkUpdated(actorUserId);
        return company;
    }

    public void LinkToPartner(Guid partnerId, Guid actorUserId)
    {
        PartnerId = partnerId;
        MarkUpdated(actorUserId);
    }

    public void UnlinkFromPartner(Guid actorUserId)
    {
        PartnerId = null;
        MarkUpdated(actorUserId);
    }
}
