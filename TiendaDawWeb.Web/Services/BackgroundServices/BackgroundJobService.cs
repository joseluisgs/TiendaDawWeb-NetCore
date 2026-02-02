namespace TiendaDawWeb.Services.BackgroundServices;

/// <summary>
/// Servicio de fondo que ejecuta tareas programadas.
/// </summary>
public class BackgroundJobService(
    IServiceProvider serviceProvider,
    ILogger<BackgroundJobService> logger,
    IHostEnvironment environment
) : BackgroundService
{
    private readonly bool _isDevelopment = environment.IsDevelopment();
    private readonly int _executionIntervalMinutes = 1440;
    private readonly int _executionIntervalHours = 168;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("BackgroundJobService iniciado - Modo: {Modo}",
            _isDevelopment ? "DESARROLLO" : "PRODUCCION");

        var interval = _isDevelopment
            ? TimeSpan.FromMinutes(_executionIntervalMinutes)
            : TimeSpan.FromHours(_executionIntervalHours);

        logger.LogInformation("Primera ejecucion en {Interval} minutos", Math.Round(interval.TotalMinutes, 0));

        await Task.Delay(interval, stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ExecuteJobsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error en BackgroundJobService");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        logger.LogInformation("BackgroundJobService detenido");
    }

    private async Task ExecuteJobsAsync(CancellationToken ct)
    {
        using var scope = serviceProvider.CreateScope();
        var productoTask = scope.ServiceProvider.GetRequiredService<ProductoReportTask>();

        logger.LogDebug("Ejecutando jobs programados");

        await productoTask.ExecuteAsync();

        logger.LogDebug("Job completado, esperando siguiente ejecucion");
    }
}
