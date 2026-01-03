using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Controllers;

/// <summary>
///     Controlador para páginas públicas (sin autenticación requerida)
/// </summary>
public class PublicController(
    IProductService productService,
    ILogger<PublicController> logger
) : Controller {
    /// <summary>
    ///     Página principal con listado de productos.
    ///     🚀 CACHÉ: Se guarda la respuesta en el servidor durante 60 segundos.
    ///     Varía por todos los parámetros de filtrado y el idioma (cookie).
    /// </summary>
    [OutputCache(Duration = 60, VaryByQueryKeys = new[] { "q", "categoria", "minPrecio", "maxPrecio", "page", "size" })]
    public async Task<IActionResult> Index(
        string? q,
        string? categoria,
        float? minPrecio,
        float? maxPrecio,
        int page = 1,
        int size = 12,
        string? lang = null) {
        // Manejar cambio de idioma si se proporciona
        if (!string.IsNullOrEmpty(lang)) {
            var culture = lang.ToLowerInvariant() switch {
                "en" => "en-US",
                "es" => "es-ES",
                "fr" => "fr-FR",
                "de" => "de-DE",
                "pt" => "pt-PT",
                _ => "es-ES"
            };

            // Añadir la cookie de cultura al response
            //De esta forma se guarda la preferencia del usuario
            // Se aplicará en futuras peticiones
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    Path = "/"
                }
            );

            // Redirigir sin el parámetro lang para limpiar la URL
            return RedirectToAction("Index", new { q, categoria, minPrecio, maxPrecio, page, size });
        }

        var result = await productService.GetAllAsync();

        if (result.IsFailure) {
            logger.LogWarning("Error obteniendo productos: {Error}", result.Error.Message);
            return View(Enumerable.Empty<Product>());
        }

        // Apply filters
        var products = result.Value.AsEnumerable();

        // Filtro de búsqueda por nombre
        if (!string.IsNullOrWhiteSpace(q)) products = products.Where(p => p.Nombre.ToLower().Contains(q.ToLower()));

        // Filtro por categoría
        if (!string.IsNullOrWhiteSpace(categoria) && Enum.TryParse<ProductCategory>(categoria, out var cat))
            products = products.Where(p => p.Categoria == cat);

        // Filtro por rango de precio
        if (minPrecio.HasValue) products = products.Where(p => (float)p.Precio >= minPrecio.Value);
        if (maxPrecio.HasValue) products = products.Where(p => (float)p.Precio <= maxPrecio.Value);

        // Ordenar por ID descendente (más recientes primero)
        products = products.OrderByDescending(p => p.Id);

        // Calculate pagination
        var totalItems = products.Count();
        var totalPages = (int)Math.Ceiling(totalItems / (double)size);
        var currentPage = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

        // Get page of products
        var pagedProducts = products
            .Skip((currentPage - 1) * size)
            .Take(size)
            .ToList();

        // ViewBag para paginación (1-based indexing matching URL parameters)
        ViewBag.CurrentPage = currentPage;
        ViewBag.Size = size;
        ViewBag.TotalElements = totalItems;
        ViewBag.TotalPages = totalPages;
        ViewBag.HasPrevious = currentPage > 1;
        ViewBag.HasNext = currentPage < totalPages;

        // ViewBag para filtros
        ViewBag.Search = q;
        ViewBag.Categoria = categoria;
        ViewBag.MinPrecio = minPrecio;
        ViewBag.MaxPrecio = maxPrecio;

        return View(pagedProducts);
    }
}