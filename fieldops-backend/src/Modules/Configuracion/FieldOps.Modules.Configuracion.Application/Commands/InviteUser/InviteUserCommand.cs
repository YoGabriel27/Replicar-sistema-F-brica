using FieldOps.Modules.Configuracion.Application.Abstractions;
using FieldOps.Modules.Configuracion.Domain;
using FieldOps.Modules.Configuracion.Domain.Events;
using FieldOps.Shared.Outbox;
using FieldOps.Shared.Tenancy;
using MediatR;

namespace FieldOps.Modules.Configuracion.Application.Commands.InviteUser;

/// <summary>
/// Invita un usuario a UNA Company con un Role inicial para esa empresa
/// (06-Modules/07-Configuracion/README.md §Forms — "Invitar usuario:
/// email, empresa a la que se le da acceso, rol inicial para esa
/// empresa"). Si el email ya existe en otra Company distinta, no hay
/// conflicto — solo se agrega un nuevo Membership.
/// </summary>
public sealed record InviteUserCommand(string Email, Guid CompanyId, Guid RoleId) : IRequest<Guid>;

public sealed class InviteUserCommandHandler(
    IConfiguracionRepository repository,
    IOutboxWriter outbox
) : IRequestHandler<InviteUserCommand, Guid>
{
    public async Task<Guid> Handle(InviteUserCommand request, CancellationToken ct)
    {
        var role = await repository.GetRoleByIdAsync(request.RoleId, ct)
            ?? throw new InvalidOperationException("RoleNotFound");

        // Validación: email ya existente CON Membership activo en la
        // MISMA empresa (regla del módulo, sección Validations) —
        // UserAlreadyExists. El mismo email en otra Company no conflictúa.
        var existingUser = await repository.GetUserByEmailAsync(request.Email, request.CompanyId, ct);
        if (existingUser is not null)
        {
            throw new InvalidOperationException("UserAlreadyExists");
        }

        var user = User.Invite(request.Email, actorUserId: Guid.Empty /* sistema/invitador, ver Api */);
        await repository.AddUserAsync(user, ct);

        var membership = Membership.Create(user.Id, request.CompanyId, request.RoleId, actorUserId: Guid.Empty);
        await repository.AddMembershipAsync(membership, ct);

        outbox.Enqueue(request.CompanyId, new UserInvited(user.Id, user.Email, request.CompanyId, request.RoleId));
        outbox.Enqueue(request.CompanyId, new MembershipCreated(membership.Id, user.Id, request.CompanyId, request.RoleId));

        await repository.SaveChangesAsync(ct);
        return user.Id;
    }
}
