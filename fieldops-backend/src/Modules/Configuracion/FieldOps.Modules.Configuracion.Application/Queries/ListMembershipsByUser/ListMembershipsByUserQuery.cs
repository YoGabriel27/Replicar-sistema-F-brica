using FieldOps.Modules.Configuracion.Application.Abstractions;
using MediatR;

namespace FieldOps.Modules.Configuracion.Application.Queries.ListMembershipsByUser;

/// <summary>
/// Empresas a las que un usuario tiene acceso — alimenta el Selector de
/// Empresa Activa del frontend (06-Modules/07-Configuracion/README.md
/// §Screens) cuando un User tiene más de un Membership.
/// </summary>
public sealed record MembershipDto(Guid MembershipId, Guid CompanyId, Guid RoleId, bool IsActive);

public sealed record ListMembershipsByUserQuery(Guid UserId) : IRequest<IReadOnlyCollection<MembershipDto>>;

public sealed class ListMembershipsByUserQueryHandler(
    IConfiguracionRepository repository
) : IRequestHandler<ListMembershipsByUserQuery, IReadOnlyCollection<MembershipDto>>
{
    public async Task<IReadOnlyCollection<MembershipDto>> Handle(
        ListMembershipsByUserQuery request, CancellationToken ct)
    {
        var memberships = await repository.ListMembershipsByUserAsync(request.UserId, ct);
        return memberships
            .Select(m => new MembershipDto(m.Id, m.CompanyId, m.RoleId, m.IsActive))
            .ToList();
    }
}
