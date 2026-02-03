using Serilog;
using TiendaDawWeb.Shared.Web.Infrastructures;
using System.Globalization;
using System.Text;

Console.OutputEncoding = Encoding.UTF8;

Log.Logger = SerilogConfig.Configure().CreateLogger();

var options = WebRootConfig.CreateOptionsWithArgs(args);
var builder = WebApplication.CreateBuilder(options);

builder.WebHost.UseStaticWebAssets();

builder.Host.UseSerilog(Log.Logger);

var defaultCulture = new CultureInfo("es-ES");
CultureInfo.DefaultThreadCurrentCulture = defaultCulture;
CultureInfo.DefaultThreadCurrentUICulture = defaultCulture;

Log.Information("🚀 Inicializando TiendaDawWeb...");

// ============================================================================
// 🔧 CONFIGURACIÓN DE SERVICIOS (Extension Methods en Infrastructure)
// ============================================================================

var services = builder.Services;
var configuration = builder.Configuration;
var environment = builder.Environment;

// Data
services.AddDatabases();

// Auth
services.AddAuthentication(configuration);

// Email
services.AddEmail();

// Business
services.AddApplicationServices();
services.AddBackgroundJobs();

// Servicios Adicionales
services.AddCleanupServices();
services.AddAppLocalization();

// Core - MVC, Razor Pages, Blazor
services.AddMvcControllers();
services.AddAppRazorPages();
services.AddBlazorServer();

// Cache & Session
services.AddCaching();

// Security
services.AddAppAntiforgery();
services.AddRateLimitingPolicy();

// CORS
services.AddCorsPolicy(configuration, environment.IsDevelopment());

// SignalR (Realtime)
services.AddAppSignalR();

// ============================================================================
// 🚀 CONSTRUCCIÓN DE LA APLICACIÓN
// ============================================================================

var app = builder.Build();
var isDevelopment = app.Environment.IsDevelopment();

Log.Information("✅ Aplicación construida");

// ============================================================================
// 📍 PIPELINE DE MIDDLEWARES (Extension Methods)
// ============================================================================

// Security Headers - Siempre activo (no afecta funcionalidad)
app.UseSecurityHeaders();

// Rate Limiting - Protege contra DDoS y fuerza bruta
app.UseRateLimiting();

if (!isDevelopment)
{
    app.UseHsts();
    app.UseHttpsRedirection();
}
else
{
    Log.Information("🔓 Modo desarrollo: HTTP permitido (sin redirección HTTPS)");
}

if (!isDevelopment)
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseStatusCodePagesWithReExecute("/Error/{0}");

app.MapStaticAssets();
app.ConfigureStaticFiles();
app.UseRouting();
app.UseAppLocalization();
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapAppEndpoints();
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

// ============================================================================
// 🗄️ INICIALIZACIÓN DE DATOS
// ============================================================================

await app.InitializeDatabaseAsync(isDevelopment);
app.InitializeStorage(isDevelopment);

PrintStartupInfo(isDevelopment, configuration);

// ============================================================================
// ▶️ ARRANQUE DE LA APLICACIÓN
// ============================================================================

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "💥 La aplicación falló al iniciar");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

/// <summary>
/// Imprime en los logs la información de inicio de la aplicación.
/// </summary>
static void PrintStartupInfo(bool isDevelopment, IConfiguration configuration)
{
    var urls = configuration["ASPNETCORE_URLS"]?.Split(';') ?? new[] { "http://localhost:5000" };
    var firstUrl = urls.FirstOrDefault() ?? "http://localhost:5000";
    var protocol = firstUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ? "https" : "http";
    var host = firstUrl.Contains("://") ? firstUrl.Split("://")[1].Split(':')[0] : "localhost";
    var port = firstUrl.Contains(':') ? firstUrl.Split(':').Last() : "5000";

    var mode = isDevelopment ? "DESARROLLO" : "PRODUCCION";
    var baseUrl = $"{protocol}://{host}:{port}";

    Log.Information("=================================================================");
    Log.Information("TiendaDawWeb - Aplicación Web Educativa");
    Log.Information("=================================================================");
    Log.Information("Acceso Publico:         {BaseUrl}/Public", baseUrl);
    Log.Information("Panel Admin:            {BaseUrl}/Admin", baseUrl);
    Log.Information("=================================================================");
    Log.Information("CREDENCIALES DE PRUEBA:");
    Log.Information("  Admin:   admin@waladaw.com / admin (ROLE_ADMIN)");
    Log.Information("  Usuario: prueba@prueba.com / prueba (ROLE_USER)");
    Log.Information("=================================================================");
    Log.Information("DATOS SEMBRADOS (Seed):");
    Log.Information("  SQLite In-Memory: 10 usuarios, 42 productos");
    Log.Information("  Roles: ADMIN, MODERATOR, USER");
    Log.Information("=================================================================");
    Log.Information("🚀 Aplicacion iniciada correctamente en {BaseUrl} ({Mode})",
        baseUrl, mode);
    Log.Information("=================================================================");
}
