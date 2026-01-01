using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.ViewModels;
using System.Diagnostics;

namespace TiendaDawWeb.Controllers;

/// <summary>
/// Controller for handling errors
/// </summary>
public class ErrorController : Controller
{
    private readonly ILogger<ErrorController> _logger;

    public ErrorController(ILogger<ErrorController> logger)
    {
        _logger = logger;
    }

    [Route("Error")]
    [Route("Error/{statusCode}")]
    public IActionResult Index(int? statusCode = null)
    {
        var model = new ErrorViewModel
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            StatusCode = statusCode ?? Response.StatusCode
        };

        // Set appropriate message based on status code
        model.Message = statusCode switch
        {
            404 => "La página que buscas no existe.",
            403 => "No tienes permisos para acceder a este recurso.",
            401 => "Debes iniciar sesión para acceder a este recurso.",
            500 => "Ha ocurrido un error interno del servidor.",
            _ => "Ha ocurrido un error inesperado. Por favor, inténtalo de nuevo."
        };

        _logger.LogError("Error {StatusCode} - RequestId: {RequestId}", model.StatusCode, model.RequestId);

        return View("Error", model);
    }
}
