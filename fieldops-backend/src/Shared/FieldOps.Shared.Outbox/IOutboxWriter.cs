using FieldOps.Shared.Kernel;

namespace FieldOps.Shared.Outbox;

/// <summary>
/// Cada módulo escribe a través de esto en su propio SaveChanges — nunca
/// publica un evento directo a Redis desde un Handler (eso lo hace el
/// BackgroundService de relay, ver OutboxRelayBackgroundService).
/// </summary>
public interface IOutboxWriter
{
    void Enqueue(Guid companyId, IDomainEvent domainEvent);
}
