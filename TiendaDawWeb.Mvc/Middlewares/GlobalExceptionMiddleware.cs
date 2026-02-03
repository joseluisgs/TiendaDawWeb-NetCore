using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Shared.Exceptions;

namespace TiendaDawWeb.Mvc.Middlewares;

/// <summary>
/// Middleware centralizado de manejo de excepciones.
/// Soporta tanto errores del dominio como excepciones tipadas.
/// </summary>
public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "🚨 ERROR en {Path}: {Message}", context.Request.Path, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            NotFoundException notFound => (404, notFound.Message, null),
            ValidationException validation => (400, validation.Message, validation.ValidationErrors),
            BusinessException business => (400, business.Message, null),
            UnauthorizedException => (401, "No autorizado", null),
            ForbiddenException => (403, "Acceso prohibido", null),
            ConflictException conflict => (409, conflict.Message, null),
            InternalException => (500, "Error interno del servidor", null),
            ArgumentException argument => (400, argument.Message, null),
            InvalidOperationException => (400, "Operación inválida", null),
            _ => (500, "Ha ocurrido un error interno", null)
        };

        context.Response.StatusCode = statusCode;

        bool isApiRequest = context.Request.Path.StartsWithSegments("/api") ||
                           context.Request.Headers["Accept"].ToString().Contains("application/json");

        if (isApiRequest)
        {
            context.Response.ContentType = "application/json";

            var response = new
            {
                success = false,
                message,
                errorType = exception.GetType().Name.Replace("Exception", ""),
                errors,
                timestamp = DateTime.UtcNow.ToString("o")
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            }));
        }
        else
        {
            context.Response.Redirect($"/Error?code={statusCode}&message={Uri.EscapeDataString(message)}");
            return Task.CompletedTask;
        }
    }
}

/// <summary>
/// Extensiones para registrar el middleware.
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
