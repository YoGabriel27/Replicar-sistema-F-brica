namespace FieldOps.Shared.Kernel;

/// <summary>
/// Repository genérico — patrón obligatorio (MASTER.md §3). Toda
/// implementación concreta vive en la capa Infrastructure del módulo
/// correspondiente y respeta el filtro global por company_id
/// (08-Backend/README.md regla #3).
/// </summary>
public interface IRepository<TEntity> where TEntity : Entity
{
    Task<TEntity?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
