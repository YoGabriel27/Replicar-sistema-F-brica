using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FieldOps.Shared.Outbox;

/// <summary>
/// Decisión de 08-Backend/README.md: Outbox sobre PostgreSQL + relay a
/// Redis Streams. Este servicio lee outbox_event no publicados y los
/// empuja al stream correspondiente — es infraestructura compartida, no
/// específica de ningún módulo de negocio.
///
/// Esqueleto — la implementación concreta de lectura (vía el DbContext de
/// cada módulo o una vista consolidada) se resuelve al construir esto,
/// no en este scaffold.
/// </summary>
public sealed class OutboxRelayBackgroundService(
    ILogger<OutboxRelayBackgroundService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("OutboxRelayBackgroundService iniciado — placeholder de scaffold.");
        while (!stoppingToken.IsCancellationRequested)
        {
            // TODO: leer outbox_event WHERE published_at IS NULL,
            // publicar a Redis Streams (XADD), marcar MarkPublished().
            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }
}
