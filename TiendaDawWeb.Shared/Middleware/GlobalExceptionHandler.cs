using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Exceptions;

namespace TiendaDawWeb.Shared.Middleware;

/// <summary>
/// Manejador global de excepciones para la aplicación Web.
/// Captura excepciones no manejadas y excepciones de dominio,
/// y genera respuestas HTTP consistentes (JSON para APIs, redirect para MVC).
/// </summary>
public class GlobalExceptionHandler(
    RequestDelegate next,
    ILogger<GlobalExceptionHandler> logger,
    IWebHostEnvironment environment
)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<GlobalExceptionHandler> _logger = logger;
    private readonly IWebHostEnvironment _environment = environment;

    /// <summary>
    /// Intercepta la petición y traduce cualquier excepción en una respuesta HTTP.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var errorId = Guid.NewGuid().ToString()[..8];
            _logger.LogError(ex, "Excepción no manejada. ErrorId: {ErrorId}, Message: {Message}",
                errorId, ex.Message);

            if (IsApiRequest(context))
            {
                await HandleApiExceptionAsync(context, ex, errorId);
            }
            else
            {
                await HandleMvcExceptionAsync(context, ex, errorId);
            }
        }
    }

    /// <summary>
    /// Detecta si la petición es una API (JSON) o una vista MVC (HTML).
    /// </summary>
    private static bool IsApiRequest(HttpContext context)
    {
        return context.Request.Path.StartsWithSegments("/api")
               || context.Request.Headers.Accept.ToString().Contains("application/json")
               || context.Request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }

    /// <summary>
    /// Maneja excepciones para peticiones API (respuesta JSON).
    /// </summary>
    private async Task HandleApiExceptionAsync(HttpContext context, Exception exception, string errorId)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors, errorType) = MapException(exception);

        context.Response.StatusCode = statusCode;

        var response = new
        {
            errorId,
            message,
            errorType,
            timestamp = DateTime.UtcNow.ToString("o"),
            path = context.Request.Path,
            method = context.Request.Method,
            errors
        };

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    /// <summary>
    /// Maneja excepciones para peticiones MVC (redirect o vista de error).
    /// </summary>
    private async Task HandleMvcExceptionAsync(HttpContext context, Exception exception, string errorId)
    {
        var (statusCode, message, _, _) = MapException(exception);

        // Para peticiones AJAX, devolver JSON
        if (context.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            await context.Response.WriteAsync(JsonSerializer.Serialize(new
            {
                errorId,
                message,
                statusCode,
                timestamp = DateTime.UtcNow.ToString("o")
            }, jsonOptions));
            return;
        }

        // Para peticiones normales, redirigir a la página de error
        context.Response.StatusCode = statusCode;
        context.Response.Redirect($"/Error?errorId={errorId}&statusCode={statusCode}");
    }

    /// <summary>
    /// Mapea excepciones a códigos HTTP y mensajes.
    /// </summary>
    private static (int StatusCode, string Message, Dictionary<string, string[]>? Errors, string ErrorType) MapException(Exception exception)
    {
        return exception switch
        {
            NotFoundException notFound => (404, notFound.Message, null, "NotFoundError"),
            Exceptions.ValidationException validation => (400, validation.Message, validation.ValidationErrors, "ValidationError"),
            BusinessException business => (422, business.Message, null, "BusinessRuleError"),
            UnauthorizedException unauthorized => (401, unauthorized.Message, null, "UnauthorizedError"),
            ForbiddenException forbidden => (403, forbidden.Message, null, "ForbiddenError"),
            ConflictException conflict => (409, conflict.Message, null, "ConflictError"),
            InternalException internalEx => (500, internalEx.Message, null, "InternalError"),
            DbUpdateConcurrencyException => (409, "Conflicto de concurrencia. El registro fue modificado por otro usuario.", null, "ConflictError"),
            DbUpdateException => (409, "Error al actualizar la base de datos", null, "ConflictError"),
            ArgumentNullException => (400, "Parámetro requerido no proporcionado", null, "ValidationError"),
            ArgumentException argument => (400, argument.Message, null, "ValidationError"),
            TimeoutException => (408, "Tiempo de espera agotado", null, "InternalError"),
            _ => (500, "Ha ocurrido un error interno", null, "InternalError")
        };
    }
}

/// <summary>
/// Extensiones para registro del middleware.
/// </summary>
public static class GlobalExceptionHandlerExtensions
{
    /// <summary>
    /// Registra el middleware de excepciones globales.
    /// Debe registrarse ANTES de UseRouting y UseEndpoints.
    /// </summary>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandler>();
    }
}
