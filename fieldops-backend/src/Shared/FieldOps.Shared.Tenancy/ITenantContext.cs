namespace FieldOps.Shared.Tenancy;

/// <summary>
/// Contexto ambiente de la empresa activa en la request actual.
/// Reemplaza lo que en un diseño 1:1 User↔Company hubiera sido un claim
/// fijo — ahora el valor sale de la Company seleccionada como activa por
/// el usuario (06-Modules/07-Configuracion/README.md regla #1), nunca de
/// un cálculo distinto por módulo.
/// </summary>
public interface ITenantContext
{
    /// <summary>
    /// Company activa de la sesión. Lanza si ninguna acción intentó
    /// resolverla todavía (ver TenantMiddleware) — un Handler nunca debe
    /// recibir un CompanyId nulo silenciosamente.
    /// </summary>
    Guid CompanyId { get; }

    Guid CurrentUserId { get; }

    bool HasActiveCompany { get; }
}

public sealed class TenantContext : ITenantContext
{
    private Guid? _companyId;
    private Guid _currentUserId;

    public Guid CompanyId => _companyId
        ?? throw new InvalidOperationException(
            "NoActiveCompanySelected: no hay Company activa en la sesión. " +
            "Ver 06-Modules/07-Configuracion/README.md — error de negocio " +
            "'NoActiveCompanySelected'.");

    public Guid CurrentUserId => _currentUserId;

    public bool HasActiveCompany => _companyId.HasValue;

    public void SetActiveCompany(Guid companyId) => _companyId = companyId;

    public void SetCurrentUser(Guid userId) => _currentUserId = userId;
}
