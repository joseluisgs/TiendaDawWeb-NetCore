using Microsoft.Extensions.FileProviders;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
///     Extensiones de configuración de archivos estáticos.
/// </summary>
public static class StaticFilesConfig {
    /// <summary>
    ///     Configura archivos estáticos para el directorio uploads.
    /// </summary>
    public static WebApplication ConfigureStaticFiles(this WebApplication app) {
        string webRootPath = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        string uploadPath = Path.Combine(webRootPath, "uploads");

        // 🛡️ Crear directorio si no existe (evita DirectoryNotFoundException en arranque)
        Directory.CreateDirectory(uploadPath);

        app.UseStaticFiles(new StaticFileOptions {
            FileProvider = new PhysicalFileProvider(uploadPath),
            RequestPath = "/uploads"
        });

        return app;
    }
}
