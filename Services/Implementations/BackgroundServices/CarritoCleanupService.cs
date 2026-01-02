using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;

namespace TiendaDawWeb.Services.Implementations.BackgroundServices;

/// <summary>
///     Servicio de limpieza automática de carritos abandonados
///     Se ejecuta periódicamente para eliminar items antiguos del carrito
/// </summary>
public class CarritoCleanupService(
    IServiceProvider serviceProvider,
    ILogger<CarritoCleanupService> logger,
    IConfiguration configuration
) : IHostedService, IDisposable {
    private Timer? _timer;

    public void Dispose() {
        _timer?.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Servicio de limpieza de carritos iniciado");

        // Obtener configuración de intervalo (por defecto 60 minutos)
        var intervalMinutes = configuration.GetValue("Carrito:CleanupIntervalMinutes", 60);
        var interval = TimeSpan.FromMinutes(intervalMinutes);

        _timer = new Timer(DoWork, null, TimeSpan.Zero, interval);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Servicio de limpieza de carritos detenido");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void DoWork(object? state) {
        logger.LogInformation("Ejecutando limpieza de carritos abandonados...");

        try {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Obtener tiempo de expiración (por defecto 24 horas)
            var expirationMinutes = configuration.GetValue("Carrito:ExpirationMinutes", 1440);
            var expirationTime = DateTime.UtcNow.AddMinutes(-expirationMinutes);

            // Eliminar items del carrito más antiguos que el tiempo de expiración
            var expiredItems = await context.CarritoItems
                .Where(ci => ci.CreatedAt < expirationTime)
                .ToListAsync();

            if (expiredItems.Any()) {
                context.CarritoItems.RemoveRange(expiredItems);
                await context.SaveChangesAsync();

                logger.LogInformation(
                    "Limpieza de carritos completada: {Count} items eliminados",
                    expiredItems.Count);
            }
            else {
                logger.LogInformation("No se encontraron carritos para limpiar");
            }
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error durante la limpieza de carritos");
        }
    }
}