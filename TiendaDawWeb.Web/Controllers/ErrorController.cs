using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.ViewModels;

namespace TiendaDawWeb.Controllers;

/// <summary>
///     Controller for handling errors
/// </summary>
public class ErrorController(
    ILogger<ErrorController> logger
) : Controller {
    [Route("Error")]
    [Route("Error/{statusCode}")]
    public IActionResult Index(int? statusCode = null) {
        // Si no viene statusCode, es una excepción (500)
        var code = statusCode ?? HttpContext.Response.StatusCode;
        if (code == 200) code = 500; // Si llegamos aquí por exception handler

        var model = new ErrorViewModel {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = code
        };

        // Mensajes centralizados
        model.Message = code switch {
            404 => "Lo sentimos, la página o producto que buscas no existe o ha sido movido.",
            403 => "Acceso denegado. No tienes permisos para ver este contenido.",
            401 => "Sesión expirada o no iniciada. Por favor, identifícate.",
            500 => "Error interno del servidor. Nuestro equipo ha sido notificado.",
            _ => "Ha ocurrido un error inesperado en la plataforma."
        };

        logger.LogError("Error detectado: {StatusCode} | Path: {Path} | RequestId: {RequestId}", 
            code, HttpContext.Items["OriginalPath"], model.RequestId);

        return View("Error", model);
    }
}