using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;

namespace TiendaDawWeb.Services.Implementations.BackgroundServices;

/// <summary>
/// Servicio de limpieza automática de carritos abandonados
/// Se ejecuta periódicamente para eliminar items antiguos del carrito
/// </summary>
public class CarritoCleanupService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<CarritoCleanupService> _logger;
    private readonly IConfiguration _configuration;
    private Timer? _timer;

    public CarritoCleanupService(
        IServiceProvider serviceProvider,
        ILogger<CarritoCleanupService> logger,
        IConfiguration configuration)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _configuration = configuration;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Servicio de limpieza de carritos iniciado");

        // Obtener configuración de intervalo (por defecto 60 minutos)
        var intervalMinutes = _configuration.GetValue("Carrito:CleanupIntervalMinutes", 60);
        var interval = TimeSpan.FromMinutes(intervalMinutes);

        _timer = new Timer(DoWork, null, TimeSpan.Zero, interval);

        return Task.CompletedTask;
    }

    private async void DoWork(object? state)
    {
        _logger.LogInformation("Ejecutando limpieza de carritos abandonados...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Obtener tiempo de expiración (por defecto 24 horas)
            var expirationMinutes = _configuration.GetValue("Carrito:ExpirationMinutes", 1440);
            var expirationTime = DateTime.UtcNow.AddMinutes(-expirationMinutes);

            // Eliminar items del carrito más antiguos que el tiempo de expiración
            var expiredItems = await context.CarritoItems
                .Where(ci => ci.CreatedAt < expirationTime)
                .ToListAsync();

            if (expiredItems.Any())
            {
                context.CarritoItems.RemoveRange(expiredItems);
                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "Limpieza de carritos completada: {Count} items eliminados", 
                    expiredItems.Count);
            }
            else
            {
                _logger.LogInformation("No se encontraron carritos para limpiar");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante la limpieza de carritos");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Servicio de limpieza de carritos detenido");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
