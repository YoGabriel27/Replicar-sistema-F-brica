using FieldOps.Modules.Configuracion.Domain;
using FieldOps.Shared.Kernel;
using FieldOps.Shared.Tenancy;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Modules.Configuracion.Infrastructure.Persistence;

/// <summary>
/// Un DbContext por módulo (08-Backend/README.md — ningún módulo comparte
/// Infrastructure con otro). Aplica el global query filter por
/// company_id en TODA entidad ITenantOwned sin excepción manual por
/// consulta (08-Backend/README.md regla #3) — defensa en profundidad
/// junto a RLS de PostgreSQL (05-Database/README.md).
///
/// Company y Partner NO llevan el filtro (no son ITenantOwned — Company
/// ES el tenant, Partner administra varios). Permission tampoco (catálogo
/// global).
/// </summary>
public sealed class ConfiguracionDbContext(
    DbContextOptions<ConfiguracionDbContext> options,
    ITenantContext tenantContext
) : DbContext(options)
{
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Partner> Partners => Set<Partner>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Membership> Memberships => Set<Membership>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<FeatureFlag> FeatureFlags => Set<FeatureFlag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ConfiguracionDbContext).Assembly);

        // Filtro global por company_id + soft delete (05-Database/README.md
        // reglas #1 y #3) aplicado reflexivamente a toda entidad
        // ITenantOwned — así ningún desarrollador puede "olvidarse" de
        // filtrarlo en una query individual.
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantOwned).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(ConfiguracionDbContext)
                    .GetMethod(nameof(BuildTenantFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .MakeGenericMethod(entityType.ClrType);
                var filter = method.Invoke(this, null);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter((System.Linq.Expressions.LambdaExpression)filter!);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    private System.Linq.Expressions.Expression<Func<TEntity, bool>> BuildTenantFilter<TEntity>()
        where TEntity : class, ITenantOwned
    {
        return entity => entity.CompanyId == tenantContext.CompanyId
            && ((Entity)(object)entity).DeletedAt == null;
    }
}
