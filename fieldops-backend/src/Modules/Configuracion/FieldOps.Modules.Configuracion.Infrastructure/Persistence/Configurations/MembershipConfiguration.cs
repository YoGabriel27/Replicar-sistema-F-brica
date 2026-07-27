using FieldOps.Modules.Configuracion.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Modules.Configuracion.Infrastructure.Persistence.Configurations;

public sealed class MembershipConfiguration : IEntityTypeConfiguration<Membership>
{
    public void Configure(EntityTypeBuilder<Membership> builder)
    {
        builder.ToTable("user_company_membership");
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => new { m.UserId, m.CompanyId }).IsUnique();
        // Validations: no CreateMembership duplicado (mismo User + Company)
        // — MembershipAlreadyExists, reforzado también a nivel de BD.
        builder.HasIndex(m => m.CompanyId); // requerido por ITenantOwned
    }
}
