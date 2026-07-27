using FieldOps.Modules.Configuracion.Application.Abstractions;
using FieldOps.Modules.Configuracion.Domain;
using FieldOps.Modules.Configuracion.Domain.Events;
using FieldOps.Shared.Outbox;
using FieldOps.Shared.Tenancy;
using MediatR;

namespace FieldOps.Modules.Configuracion.Application.Commands.CreateCompany;

/// <summary>
/// Onboarding de una empresa nueva — independiente o bajo un Partner
/// existente (06-Modules/07-Configuracion/README.md §Screens, Onboarding
/// Wizard). No requiere Company activa en el TenantContext (es, de
/// hecho, quien la crea) — por eso no depende de ITenantContext salvo
/// para saber quién ejecuta la acción.
/// </summary>
public sealed record CreateCompanyCommand(
    string Name,
    Guid? PartnerId,
    string CountryCode,
    string DefaultCurrency
) : IRequest<Guid>;

public sealed class CreateCompanyCommandHandler(
    IConfiguracionRepository repository,
    IOutboxWriter outbox,
    ITenantContext tenantContext
) : IRequestHandler<CreateCompanyCommand, Guid>
{
    public async Task<Guid> Handle(CreateCompanyCommand request, CancellationToken ct)
    {
        // Invariante de dominio validado ANTES de persistir
        // (08-Backend/README.md regla #1) — nunca se salta la capa de
        // Dominio. Company.Create ya construye un objeto válido; si el
        // PartnerId viene informado, se valida que exista.
        if (request.PartnerId is { } partnerId)
        {
            var partner = await repository.GetPartnerByIdAsync(partnerId, ct)
                ?? throw new InvalidOperationException("PartnerNotFound");
        }

        var company = Company.Create(
            request.Name,
            actorUserId: tenantContext.CurrentUserId,
            partnerId: request.PartnerId,
            countryCode: request.CountryCode,
            defaultCurrency: request.DefaultCurrency);

        await repository.AddCompanyAsync(company, ct);

        outbox.Enqueue(company.Id, new CompanyCreated(company.Id, company.Name, company.PartnerId));

        await repository.SaveChangesAsync(ct);
        return company.Id;
    }
}
