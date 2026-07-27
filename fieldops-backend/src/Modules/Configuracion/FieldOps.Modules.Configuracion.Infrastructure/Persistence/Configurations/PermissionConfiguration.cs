using FieldOps.Modules.Configuracion.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Modules.Configuracion.Infrastructure.Persistence.Configurations;

public sealed class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("permission"); // catálogo global — sin company_id
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Code).HasMaxLength(150).IsRequired();
        builder.HasIndex(p => p.Code).IsUnique();
    }
}
