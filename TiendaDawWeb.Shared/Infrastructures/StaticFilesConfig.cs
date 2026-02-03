using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.FileProviders;
using Serilog;

namespace TiendaDawWeb.Shared.Web.Infrastructures;

/// <summary>
/// Extensiones de configuración de archivos estáticos.
/// </summary>
public static class StaticFilesConfig
{
    /// <summary>
    /// Configura archivos estáticos para el directorio uploads.
    /// </summary>
    public static WebApplication ConfigureStaticFiles(this WebApplication app)
    {
        var webRootPath = app.Environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadPath = Path.Combine(webRootPath, "uploads");

        app.UseStaticFiles(new StaticFileOptions
        {
            FileProvider = new PhysicalFileProvider(uploadPath),
            RequestPath = "/uploads"
        });

        return app;
    }
}
