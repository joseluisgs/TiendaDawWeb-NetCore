using Microsoft.Extensions.Configuration;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Shared.Services.BackgroundServices;

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
        int intervalMinutes = configuration.GetValue("Carrito:CleanupIntervalMinutes", 60);
        var interval = TimeSpan.FromMinutes(intervalMinutes);
        _timer = new Timer(async _ => await DoWorkAsync(_), null, TimeSpan.Zero, interval);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Servicio de limpieza de carritos detenido");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async Task DoWorkAsync(object? state) {
        logger.LogInformation("Ejecutando limpieza de carritos abandonados...");
        try {
            using IServiceScope scope = serviceProvider.CreateScope();
            ApplicationDbContext context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            int expirationMinutes = configuration.GetValue("Carrito:ExpirationMinutes", 1440);
            DateTime expirationTime = DateTime.UtcNow.AddMinutes(-expirationMinutes);
            List<CarritoItem> expiredItems =
                await context.CarritoItems.Where(ci => ci.CreatedAt < expirationTime).ToListAsync();
            if (expiredItems.Any()) {
                context.CarritoItems.RemoveRange(expiredItems);
                await context.SaveChangesAsync();
                logger.LogInformation("Limpieza completada: {Count} items eliminados", expiredItems.Count);
            }
        } catch (Exception ex) {
            logger.LogError(ex, "Error durante la limpieza de carritos");
        }
    }
}
