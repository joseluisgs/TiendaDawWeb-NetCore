using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Controllers;

/// <summary>
/// Controlador para páginas públicas (sin autenticación requerida)
/// </summary>
public class PublicController : Controller
{
    private readonly IProductService _productService;
    private readonly ILogger<PublicController> _logger;

    public PublicController(IProductService productService, ILogger<PublicController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Página principal con listado de productos
    /// </summary>
    public async Task<IActionResult> Index(string? search, string? categoria, decimal? minPrecio, decimal? maxPrecio)
    {
        var result = string.IsNullOrWhiteSpace(search) && string.IsNullOrWhiteSpace(categoria) && !minPrecio.HasValue && !maxPrecio.HasValue
            ? await _productService.GetAllAsync()
            : await _productService.SearchAsync(search, categoria);

        if (result.IsFailure)
        {
            _logger.LogWarning("Error obteniendo productos: {Error}", result.Error.Message);
            return View(Enumerable.Empty<Models.Product>());
        }

        // Apply price filtering if specified
        var products = result.Value;
        if (minPrecio.HasValue)
        {
            products = products.Where(p => p.Precio >= minPrecio.Value);
        }
        if (maxPrecio.HasValue)
        {
            products = products.Where(p => p.Precio <= maxPrecio.Value);
        }

        ViewBag.Search = search;
        ViewBag.Categoria = categoria;
        ViewBag.MinPrecio = minPrecio;
        ViewBag.MaxPrecio = maxPrecio;
        
        return View(products);
    }
}
