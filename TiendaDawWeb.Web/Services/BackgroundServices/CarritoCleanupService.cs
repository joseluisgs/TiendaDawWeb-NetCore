using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;

namespace TiendaDawWeb.Services.BackgroundServices;

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

    public void Dispose() => _timer?.Dispose();

    public Task StartAsync(CancellationToken cancellationToken) {
        logger.LogInformation("Servicio de limpieza de carritos iniciado");
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
            var expirationMinutes = configuration.GetValue("Carrito:ExpirationMinutes", 1440);
            var expirationTime = DateTime.UtcNow.AddMinutes(-expirationMinutes);
            var expiredItems = await context.CarritoItems.Where(ci => ci.CreatedAt < expirationTime).ToListAsync();
            if (expiredItems.Any()) {
                context.CarritoItems.RemoveRange(expiredItems);
                await context.SaveChangesAsync();
                logger.LogInformation("Limpieza completada: {Count} items eliminados", expiredItems.Count);
            }
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error durante la limpieza de carritos");
        }
    }
}
