using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TiendaDawWeb.RazorPages.Pages;

/// <summary>
/// Página raíz que redirige a la página pública preservando parámetros de búsqueda
/// </summary>
public class IndexModel : PageModel
{
    /// <summary>
    /// GET / - Redirige a /Public preservando todos los query parameters
    /// </summary>
    public IActionResult OnGet(
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
        return RedirectToPage("/Public/Index", new
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
