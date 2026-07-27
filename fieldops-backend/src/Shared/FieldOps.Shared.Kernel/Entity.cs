namespace FieldOps.Shared.Kernel;

/// <summary>
/// Base para toda entidad de negocio del sistema. Fija las columnas de
/// auditoría obligatorias (05-Database/README.md regla #4) y el soft
/// delete obligatorio (MASTER.md §3, 05-Database/README.md regla #3).
/// Ninguna entidad de negocio debe heredar de otra base ni omitir estas
/// columnas.
/// </summary>
public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();

    public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
    public Guid CreatedBy { get; protected set; }

    public DateTimeOffset? UpdatedAt { get; protected set; }
    public Guid? UpdatedBy { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }
    public bool IsDeleted => DeletedAt is not null;

    public void MarkUpdated(Guid actorUserId)
    {
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = actorUserId;
    }

    /// <summary>
    /// Soft delete — nunca DELETE físico de una entidad de negocio
    /// (05-Database/README.md regla #3).
    /// </summary>
    public void SoftDelete(Guid actorUserId)
    {
        DeletedAt = DateTimeOffset.UtcNow;
        MarkUpdated(actorUserId);
    }
}

/// <summary>
/// Entidad de negocio que pertenece a un tenant (Company). La inmensa
/// mayoría de las entidades del sistema implementan esta interfaz —
/// 05-Database/README.md regla #1: ninguna tabla de negocio omite
/// company_id salvo catálogos explícitamente globales.
/// </summary>
public interface ITenantOwned
{
    Guid CompanyId { get; }
}
