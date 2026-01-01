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
    public async Task<IActionResult> Index(
        string? search, 
        string? categoria, 
        decimal? minPrecio, 
        decimal? maxPrecio,
        int page = 0,
        int size = 9)
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
        var products = result.Value.AsEnumerable();
        if (minPrecio.HasValue)
        {
            products = products.Where(p => p.Precio >= minPrecio.Value);
        }
        if (maxPrecio.HasValue)
        {
            products = products.Where(p => p.Precio <= maxPrecio.Value);
        }

        // Calculate pagination
        var totalElements = products.Count();
        var totalPages = (int)Math.Ceiling(totalElements / (double)size);
        var currentPage = Math.Max(0, Math.Min(page, totalPages - 1));
        
        // Get page of products
        var pagedProducts = products
            .Skip(currentPage * size)
            .Take(size)
            .ToList();

        // Pass pagination data to view
        ViewBag.Search = search;
        ViewBag.Categoria = categoria;
        ViewBag.MinPrecio = minPrecio;
        ViewBag.MaxPrecio = maxPrecio;
        ViewBag.CurrentPage = currentPage;
        ViewBag.Size = size;
        ViewBag.TotalPages = totalPages;
        ViewBag.TotalElements = totalElements;
        ViewBag.HasPrevious = currentPage > 0;
        ViewBag.HasNext = currentPage < totalPages - 1;
        
        return View(pagedProducts);
    }
}
