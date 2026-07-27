using FieldOps.Modules.Configuracion.Domain;

namespace FieldOps.Modules.Configuracion.Application.Abstractions;

/// <summary>
/// Puerto hacia Infrastructure (Clean/Hexagonal — 04-Architecture/README.md).
/// Application nunca referencia EF Core directamente, solo esta interfaz;
/// la implementación concreta vive en el proyecto .Infrastructure.
/// </summary>
public interface IConfiguracionRepository
{
    Task<Company?> GetCompanyByIdAsync(Guid companyId, CancellationToken ct);
    Task AddCompanyAsync(Company company, CancellationToken ct);

    Task<Partner?> GetPartnerByIdAsync(Guid partnerId, CancellationToken ct);
    Task AddPartnerAsync(Partner partner, CancellationToken ct);

    Task<User?> GetUserByEmailAsync(string email, Guid companyId, CancellationToken ct);
    Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct);
    Task AddUserAsync(User user, CancellationToken ct);

    Task<bool> MembershipExistsAsync(Guid userId, Guid companyId, CancellationToken ct);
    Task<IReadOnlyCollection<Membership>> ListMembershipsByUserAsync(Guid userId, CancellationToken ct);
    Task AddMembershipAsync(Membership membership, CancellationToken ct);
    Task<Membership?> GetMembershipAsync(Guid membershipId, CancellationToken ct);

    Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);
}
