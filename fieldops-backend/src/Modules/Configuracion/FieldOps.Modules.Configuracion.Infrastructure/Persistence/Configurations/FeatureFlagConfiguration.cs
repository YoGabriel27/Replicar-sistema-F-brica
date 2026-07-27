using FieldOps.Modules.Configuracion.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Modules.Configuracion.Infrastructure.Persistence.Configurations;

public sealed class FeatureFlagConfiguration : IEntityTypeConfiguration<FeatureFlag>
{
    public void Configure(EntityTypeBuilder<FeatureFlag> builder)
    {
        builder.ToTable("feature_flag");
        builder.HasKey(f => f.Id);
        builder.Property(f => f.Key).HasMaxLength(100).IsRequired();
        builder.HasIndex(f => new { f.CompanyId, f.Key }).IsUnique();
    }
}
