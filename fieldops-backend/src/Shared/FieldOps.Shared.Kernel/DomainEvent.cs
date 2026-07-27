namespace FieldOps.Shared.Kernel;

/// <summary>
/// Marca un evento de dominio, tal como se nombran en
/// 03-Domain-Model/README.md §Events. Todo evento se publica vía Outbox
/// (04-Architecture/README.md regla #2) — nunca se invoca síncronamente
/// la infraestructura de otro módulo.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAt { get; }
}

public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
