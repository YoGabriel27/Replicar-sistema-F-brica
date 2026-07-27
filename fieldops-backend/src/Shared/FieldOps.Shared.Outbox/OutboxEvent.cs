using FieldOps.Shared.Kernel;

namespace FieldOps.Shared.Outbox;

/// <summary>
/// Fila de la tabla outbox_event (05-Database/README.md, ver también
/// 08-Backend/README.md §Decisión: transporte del Outbox). Se escribe en
/// la MISMA transacción que el cambio de negocio que la origina — nunca
/// en una transacción separada, o se pierde la garantía de at-least-once.
/// </summary>
public sealed class OutboxEvent : Entity
{
    public required Guid CompanyId { get; init; }
    public required string EventType { get; init; }   // p.ej. "WorkOrderClosed"
    public required string PayloadJson { get; init; }
    public DateTimeOffset? PublishedAt { get; private set; }

    public void MarkPublished() => PublishedAt = DateTimeOffset.UtcNow;
}
