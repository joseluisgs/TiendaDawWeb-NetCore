using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.RazorPages.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    public string? RequestId { get; set; }
    public int StatusCodeValue { get; set; }
    public string? Message { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public void OnGet(int? statusCode = null)
    {
        StatusCodeValue = statusCode ?? HttpContext.Response.StatusCode;
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        if (StatusCodeValue == 200)
            StatusCodeValue = 500;

        Message = StatusCodeValue switch
        {
            404 => "Lo sentimos, la página o producto que buscas no existe o ha sido movido.",
            403 => "Acceso denegado. No tienes permisos para ver este contenido.",
            401 => "Sesión expirada o no iniciada. Por favor, identifícate.",
            500 => "Error interno del servidor. Nuestro equipo ha sido notificado.",
            _ => "Ha ocurrido un error inesperado en la plataforma."
        };
    }
}
