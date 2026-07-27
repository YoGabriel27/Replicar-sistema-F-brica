using FieldOps.Modules.Configuracion.Application.Abstractions;
using FieldOps.Modules.Configuracion.Domain;
using FieldOps.Modules.Configuracion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Modules.Configuracion.Infrastructure.Repositories;

/// <summary>
/// Implementación concreta del puerto IConfiguracionRepository — vive
/// solo aquí, en Infrastructure. Application y Domain nunca importan
/// Microsoft.EntityFrameworkCore directamente (04-Architecture/README.md).
/// </summary>
public sealed class EfConfiguracionRepository(ConfiguracionDbContext db) : IConfiguracionRepository
{
    public Task<Company?> GetCompanyByIdAsync(Guid companyId, CancellationToken ct) =>
        db.Companies.FirstOrDefaultAsync(c => c.Id == companyId, ct);

    public Task AddCompanyAsync(Company company, CancellationToken ct) =>
        db.Companies.AddAsync(company, ct).AsTask();

    public Task<Partner?> GetPartnerByIdAsync(Guid partnerId, CancellationToken ct) =>
        db.Partners.FirstOrDefaultAsync(p => p.Id == partnerId, ct);

    public Task AddPartnerAsync(Partner partner, CancellationToken ct) =>
        db.Partners.AddAsync(partner, ct).AsTask();

    public Task<User?> GetUserByEmailAsync(string email, Guid companyId, CancellationToken ct) =>
        db.Users
            .Join(db.Memberships.Where(m => m.CompanyId == companyId && m.IsActive),
                u => u.Id, m => m.UserId, (u, m) => u)
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public Task<User?> GetUserByIdAsync(Guid userId, CancellationToken ct) =>
        db.Users.FirstOrDefaultAsync(u => u.Id == userId, ct);

    public Task AddUserAsync(User user, CancellationToken ct) =>
        db.Users.AddAsync(user, ct).AsTask();

    public Task<bool> MembershipExistsAsync(Guid userId, Guid companyId, CancellationToken ct) =>
        db.Memberships.AnyAsync(m => m.UserId == userId && m.CompanyId == companyId && m.IsActive, ct);

    public async Task<IReadOnlyCollection<Membership>> ListMembershipsByUserAsync(Guid userId, CancellationToken ct) =>
        await db.Memberships
            .IgnoreQueryFilters() // cruza empresas a propósito: lista TODAS las del usuario
            .Where(m => m.UserId == userId && m.IsActive)
            .ToListAsync(ct);

    public Task AddMembershipAsync(Membership membership, CancellationToken ct) =>
        db.Memberships.AddAsync(membership, ct).AsTask();

    public Task<Membership?> GetMembershipAsync(Guid membershipId, CancellationToken ct) =>
        db.Memberships.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == membershipId, ct);

    public Task<Role?> GetRoleByIdAsync(Guid roleId, CancellationToken ct) =>
        db.Roles.FirstOrDefaultAsync(r => r.Id == roleId, ct);

    public Task SaveChangesAsync(CancellationToken ct) => db.SaveChangesAsync(ct);
}
