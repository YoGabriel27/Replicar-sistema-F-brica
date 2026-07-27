using FieldOps.Modules.Configuracion.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FieldOps.Modules.Configuracion.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("app_user");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(320).IsRequired();
        // Nota: el índice de unicidad de email es POR EMPRESA (vía
        // Membership), no global — no se declara unique aquí a propósito.
        builder.HasIndex(u => u.Email);
        builder.Ignore(u => u.Memberships); // navegación se resuelve vía Membership.UserId
    }
}
