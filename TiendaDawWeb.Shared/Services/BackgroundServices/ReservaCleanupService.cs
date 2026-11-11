using Microsoft.Extensions.Configuration;
using TiendaDawWeb.Shared.Data;

namespace TiendaDawWeb.Shared.Services.BackgroundServices;

/// <summary>
///     Servicio de limpieza automática de reservas de productos expiradas
///     Se ejecuta periódicamente para liberar productos cuya reserva temporal ha expirado
/// </summary>
public class ReservaCleanupService(
    IServiceProvider serviceProvider,
    ILogger<ReservaCleanupService> logger,
    IConfiguration configuration
) : IHostedService, IDisposable {
    private Timer? _timer;

    public void Dispose() {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Servicio de limpieza de reservas iniciado");
        int intervalMinutes = configuration.GetValue("Reservas:CleanupIntervalMinutes", 5);
        var interval = TimeSpan.FromMinutes(intervalMinutes);
        _timer = new Timer(async _ => await DoWorkAsync(_), null, TimeSpan.Zero, interval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Servicio de limpieza de reservas detenido");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async Task DoWorkAsync(object? state) {
        try {
            using IServiceScope scope = serviceProvider.CreateScope();
            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            DateTime now = DateTime.UtcNow;
            List<Models.Product> expiredReservations = await context.Products
                .Where(p => p.Reservado && p.ReservadoHasta.HasValue && p.ReservadoHasta.Value < now).ToListAsync();
            if (expiredReservations.Any()) {
                foreach (Models.Product product in expiredReservations) {
                    product.Reservado = false;
                    product.ReservadoHasta = null;
                }

                await context.SaveChangesAsync();
                logger.LogInformation("Limpieza de reservas completada: {Count} productos liberados",
                    expiredReservations.Count);
            }
        } catch (Exception ex) {
            logger.LogError(ex, "Error durante la limpieza de reservas");
        }
    }
}
