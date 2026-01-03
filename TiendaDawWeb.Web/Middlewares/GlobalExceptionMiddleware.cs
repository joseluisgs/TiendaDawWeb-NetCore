using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace TiendaDawWeb.Web.Middlewares;

/// <summary>
/// OBJETIVO: Centralizar el manejo de excepciones de toda la aplicación.
/// UBICACIÓN: /Middlewares
/// RAZÓN: Actúa como una red de seguridad global. Captura cualquier error no controlado,
/// lo registra en el log y devuelve una respuesta coherente según el tipo de cliente (API o Web).
/// </summary>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Continuar con el siguiente componente del pipeline
            await next(context);
        }
        catch (Exception ex)
        {
            // 1. Loguear el error con Serilog incluyendo detalles del contexto
            logger.LogError(ex, "🚨 ERROR NO CONTROLADO en {Path}: {Message}", 
                context.Request.Path, ex.Message);

            // 2. Gestionar la respuesta según el origen
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        // Comprobamos si la petición es para una API (AJAX)
        bool isApiRequest = context.Request.Path.StartsWithSegments("/api") || 
                           context.Request.Headers["Accept"].ToString().Contains("application/json");

        if (isApiRequest)
        {
            // RESPUESTA PARA APIs: JSON estructurado
            context.Response.ContentType = "application/json";
            
            var response = new
            {
                success = false,
                message = "Ha ocurrido un error interno en el servidor.",
                error = exception.Message // En producción, es mejor no exponer este detalle
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        else
        {
            // RESPUESTA PARA WEB: Redirigir al controlador de errores amigable
            context.Response.Redirect("/Error");
            return Task.CompletedTask;
        }
    }
}

/// <summary>
/// Método de extensión para facilitar el registro en Program.cs
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
