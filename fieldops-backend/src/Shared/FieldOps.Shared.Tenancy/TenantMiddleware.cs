using Microsoft.AspNetCore.Http;

namespace FieldOps.Shared.Tenancy;

/// <summary>
/// Resuelve company_id UNA sola vez por request, en el borde del sistema
/// (04-Architecture/README.md regla #4) — ningún módulo vuelve a
/// resolverlo por su cuenta. Lee el claim "active_company_id" del JWT,
/// puesto ahí tras el flujo de selección de empresa activa
/// (06-Modules/07-Configuracion/README.md regla #1).
///
/// Nota: la validación de que el usuario efectivamente tiene un
/// Membership vigente sobre esa Company se hace en el módulo
/// Configuración (GetUserPermissionsQuery), no aquí — este middleware
/// solo transporta el valor ya validado en el token.
/// </summary>
public sealed class TenantMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        var userIdClaim = context.User.FindFirst("sub")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            tenantContext.SetCurrentUser(userId);
        }

        var companyClaim = context.User.FindFirst("active_company_id")?.Value;
        if (Guid.TryParse(companyClaim, out var companyId))
        {
            tenantContext.SetActiveCompany(companyId);
        }
        // Si no hay claim, se deja sin resolver a propósito — un Handler
        // que necesite CompanyId y no lo encuentre falla explícito
        // (NoActiveCompanySelected), nunca con un tenant equivocado.

        await next(context);
    }
}
