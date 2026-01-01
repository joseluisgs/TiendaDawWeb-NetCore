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
    public async Task<IActionResult> Index(string? search, string? categoria)
    {
        var result = string.IsNullOrWhiteSpace(search) && string.IsNullOrWhiteSpace(categoria)
            ? await _productService.GetAllAsync()
            : await _productService.SearchAsync(search, categoria);

        if (result.IsFailure)
        {
            _logger.LogWarning("Error obteniendo productos: {Error}", result.Error.Message);
            return View(Enumerable.Empty<Models.Product>());
        }

        ViewBag.Search = search;
        ViewBag.Categoria = categoria;
        return View(result.Value);
    }
}
