using FieldOps.Modules.Configuracion.Application.Abstractions;
using FieldOps.Modules.Configuracion.Domain.Events;
using FieldOps.Shared.Outbox;
using FieldOps.Shared.Tenancy;
using MediatR;

namespace FieldOps.Modules.Configuracion.Application.Commands.RevokeMembership;

/// <summary>
/// Quita acceso a UNA Company sin afectar memberships del mismo usuario
/// en otras (06-Modules/07-Configuracion/README.md regla #5 — más
/// granular que DeactivateUser).
/// </summary>
public sealed record RevokeMembershipCommand(Guid MembershipId) : IRequest;

public sealed class RevokeMembershipCommandHandler(
    IConfiguracionRepository repository,
    IOutboxWriter outbox,
    ITenantContext tenantContext
) : IRequestHandler<RevokeMembershipCommand>
{
    public async Task Handle(RevokeMembershipCommand request, CancellationToken ct)
    {
        var membership = await repository.GetMembershipAsync(request.MembershipId, ct)
            ?? throw new InvalidOperationException("MembershipNotFound");

        membership.Revoke(tenantContext.CurrentUserId);

        outbox.Enqueue(membership.CompanyId,
            new MembershipRevoked(membership.Id, membership.UserId, membership.CompanyId));

        await repository.SaveChangesAsync(ct);
    }
}
