using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;

namespace TiendaDawWeb.Services.Implementations.BackgroundServices;

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

        // Obtener configuración de intervalo (por defecto 5 minutos)
        var intervalMinutes = configuration.GetValue("Reservas:CleanupIntervalMinutes", 5);
        var interval = TimeSpan.FromMinutes(intervalMinutes);

        _timer = new Timer(DoWork, null, TimeSpan.Zero, interval);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Servicio de limpieza de reservas detenido");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void DoWork(object? state) {
        logger.LogDebug("Ejecutando limpieza de reservas expiradas...");

        try {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Encontrar productos con reservas expiradas
            var now = DateTime.UtcNow;
            var expiredReservations = await context.Products
                .Where(p => p.Reservado && p.ReservadoHasta.HasValue && p.ReservadoHasta.Value < now)
                .ToListAsync();

            if (expiredReservations.Any()) {
                foreach (var product in expiredReservations) {
                    product.Reservado = false;
                    product.ReservadoHasta = null;
                }

                await context.SaveChangesAsync();

                logger.LogInformation(
                    "Limpieza de reservas completada: {Count} productos liberados",
                    expiredReservations.Count);
            }
            else {
                logger.LogDebug("No se encontraron reservas expiradas");
            }
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error durante la limpieza de reservas");
        }
    }
}