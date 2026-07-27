using FieldOps.Modules.Configuracion.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Modules.Configuracion.Infrastructure.Persistence.Configurations;

public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("role");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Name).HasMaxLength(100).IsRequired();
        builder.HasIndex(r => r.CompanyId);
        builder.Property(r => r.PermissionIds)
            .HasColumnName("permission_ids"); // simplificado para el scaffold;
                                                // en producción: tabla puente role_permission
    }
}
