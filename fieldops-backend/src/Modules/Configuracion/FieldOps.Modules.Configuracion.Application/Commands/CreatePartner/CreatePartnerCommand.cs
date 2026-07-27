using FieldOps.Modules.Configuracion.Application.Abstractions;
using FieldOps.Modules.Configuracion.Domain;
using FieldOps.Modules.Configuracion.Domain.Events;
using FieldOps.Shared.Outbox;
using FieldOps.Shared.Tenancy;
using MediatR;

namespace FieldOps.Modules.Configuracion.Application.Commands.CreatePartner;

public sealed record CreatePartnerCommand(string Name) : IRequest<Guid>;

public sealed class CreatePartnerCommandHandler(
    IConfiguracionRepository repository,
    IOutboxWriter outbox,
    ITenantContext tenantContext
) : IRequestHandler<CreatePartnerCommand, Guid>
{
    public async Task<Guid> Handle(CreatePartnerCommand request, CancellationToken ct)
    {
        var partner = Partner.Create(request.Name, tenantContext.CurrentUserId);
        await repository.AddPartnerAsync(partner, ct);

        // Partner no es ITenantOwned (administra empresas, no pertenece a
        // una) — el evento se enruta sin company_id de origen único.
        outbox.Enqueue(companyId: Guid.Empty, new PartnerCreated(partner.Id, partner.Name));

        await repository.SaveChangesAsync(ct);
        return partner.Id;
    }
}
