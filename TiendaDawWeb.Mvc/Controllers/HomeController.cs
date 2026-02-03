using Microsoft.AspNetCore.Mvc;

namespace TiendaDawWeb.Controllers;

/// <summary>
/// Controlador raíz que redirige a la página pública preservando parámetros de búsqueda
/// </summary>
public class HomeController : Controller
{
    /// <summary>
    /// GET / - Redirige a PublicController.Index preservando todos los query parameters
    /// </summary>
    [Route("")]
    public IActionResult Index(
        string? search, 
        string? q,
        string? categoria, 
        float? minPrecio, 
        float? maxPrecio, 
        int page = 1, 
        int size = 12)
    {
        // Normalizar parámetros de búsqueda (puede venir como "search" o "q")
        var searchQuery = search ?? q;
        
        // Preservar todos los parámetros en la redirección
        return RedirectToAction("Index", "Public", new 
        { 
            q = searchQuery,
            categoria, 
            minPrecio, 
            maxPrecio, 
            page, 
            size 
        });
    }
}
