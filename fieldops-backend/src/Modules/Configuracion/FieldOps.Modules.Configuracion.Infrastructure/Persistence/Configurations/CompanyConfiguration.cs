using FieldOps.Modules.Configuracion.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Modules.Configuracion.Infrastructure.Persistence.Configurations;

public sealed class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("company");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.CountryCode).HasMaxLength(2).IsRequired();
        builder.Property(c => c.DefaultCurrency).HasMaxLength(3).IsRequired();
        builder.HasIndex(c => c.PartnerId);
    }
}
