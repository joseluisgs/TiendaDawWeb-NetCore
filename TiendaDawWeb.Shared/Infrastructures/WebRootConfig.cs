namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
///     Extensiones de configuración de rutas web.
/// </summary>
public static class WebRootConfig {
    /// <summary>
    ///     Crea WebApplicationOptions con rutas ajustadas dinámicamente.
    /// </summary>
    public static WebApplicationOptions CreateOptionsWithArgs(string[] args) {
        string currentDirectory = Directory.GetCurrentDirectory();
        string wwwrootPath = Path.Combine(currentDirectory, "wwwroot");
        string projectWwwrootPath = Path.Combine(currentDirectory, "TiendaDawWeb.Web", "wwwroot");

        if (!Directory.Exists(wwwrootPath) && Directory.Exists(projectWwwrootPath)) {
            Log.Information("📁 Ajustando rutas para ejecución desde solución...");
            return new WebApplicationOptions {
                Args = args,
                ContentRootPath = Path.Combine(currentDirectory, "TiendaDawWeb.Web"),
                WebRootPath = projectWwwrootPath
            };
        }

        return new WebApplicationOptions {
            Args = args,
            ContentRootPath = currentDirectory,
            WebRootPath = "wwwroot"
        };
    }
}
