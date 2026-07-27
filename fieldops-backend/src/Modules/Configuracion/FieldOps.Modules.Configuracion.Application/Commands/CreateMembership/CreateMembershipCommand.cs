using FieldOps.Modules.Configuracion.Application.Abstractions;
using FieldOps.Modules.Configuracion.Domain;
using FieldOps.Modules.Configuracion.Domain.Events;
using FieldOps.Shared.Outbox;
using MediatR;

namespace FieldOps.Modules.Configuracion.Application.Commands.CreateMembership;

/// <summary>
/// Otorga acceso de un User YA EXISTENTE a una Company adicional — el
/// caso de un usuario de Partner que ya tiene cuenta y se le agrega
/// acceso a una nueva empresa administrada por el mismo Partner.
/// </summary>
public sealed record CreateMembershipCommand(Guid UserId, Guid CompanyId, Guid RoleId) : IRequest<Guid>;

public sealed class CreateMembershipCommandHandler(
    IConfiguracionRepository repository,
    IOutboxWriter outbox
) : IRequestHandler<CreateMembershipCommand, Guid>
{
    public async Task<Guid> Handle(CreateMembershipCommand request, CancellationToken ct)
    {
        _ = await repository.GetUserByIdAsync(request.UserId, ct)
            ?? throw new InvalidOperationException("UserNotFound");
        _ = await repository.GetRoleByIdAsync(request.RoleId, ct)
            ?? throw new InvalidOperationException("RoleNotFound");

        // Validations: no se puede CreateMembership duplicado (mismo
        // User + Company) — MembershipAlreadyExists.
        if (await repository.MembershipExistsAsync(request.UserId, request.CompanyId, ct))
        {
            throw new InvalidOperationException("MembershipAlreadyExists");
        }

        var membership = Membership.Create(request.UserId, request.CompanyId, request.RoleId, actorUserId: request.UserId);
        await repository.AddMembershipAsync(membership, ct);

        outbox.Enqueue(request.CompanyId, new MembershipCreated(membership.Id, request.UserId, request.CompanyId, request.RoleId));

        await repository.SaveChangesAsync(ct);
        return membership.Id;
    }
}
