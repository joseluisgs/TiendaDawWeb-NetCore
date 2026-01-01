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
        string? q, 
        string? categoria, 
        float? minPrecio, 
        float? maxPrecio,
        int page = 1,
        int size = 12)
    {
        var result = await _productService.GetAllAsync();

        if (result.IsFailure)
        {
            _logger.LogWarning("Error obteniendo productos: {Error}", result.Error.Message);
            return View(Enumerable.Empty<Models.Product>());
        }

        // Apply filters
        var products = result.Value.AsEnumerable();
        
        // Filtro de búsqueda por nombre
        if (!string.IsNullOrWhiteSpace(q))
        {
            products = products.Where(p => p.Nombre.ToLower().Contains(q.ToLower()));
        }

        // Filtro por categoría
        if (!string.IsNullOrWhiteSpace(categoria) && Enum.TryParse<Models.Enums.ProductCategory>(categoria, out var cat))
        {
            products = products.Where(p => p.Categoria == cat);
        }

        // Filtro por rango de precio
        if (minPrecio.HasValue)
        {
            products = products.Where(p => (float)p.Precio >= minPrecio.Value);
        }
        if (maxPrecio.HasValue)
        {
            products = products.Where(p => (float)p.Precio <= maxPrecio.Value);
        }

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

        // ViewData para mantener filtros activos
        ViewData["q"] = q;
        ViewData["categoriaActual"] = categoria;
        ViewData["minPrecio"] = minPrecio;
        ViewData["maxPrecio"] = maxPrecio;
        ViewData["CurrentPage"] = currentPage;
        ViewData["TotalPages"] = totalPages;
        ViewData["PageSize"] = size;
        
        return View(pagedProducts);
    }
}
