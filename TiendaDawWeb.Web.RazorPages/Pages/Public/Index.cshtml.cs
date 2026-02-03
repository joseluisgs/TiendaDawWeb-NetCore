using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.OutputCaching;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Product;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.Web.RazorPages.Pages.Public;

public class IndexModel(
    IProductService productService,
    ILogger<IndexModel> logger
) : PageModel {
    public IEnumerable<ProductModel> Products { get; set; } = Enumerable.Empty<ProductModel>();

    public async Task<IActionResult> OnGetAsync(
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

            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    IsEssential = true,
                    Path = "/"
                }
            );

            return RedirectToPage("/Public/Index", new { q, categoria, minPrecio, maxPrecio, page, size });
        }

        var result = await productService.GetAllAsync();

        if (result.IsFailure) {
            logger.LogWarning("Error obteniendo productos: {Error}", result.Error.Message);
            Products = Enumerable.Empty<ProductModel>();
            return Page();
        }

        // Apply filters
        var products = result.Value.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(q)) products = products.Where(p => p.Nombre.ToLower().Contains(q.ToLower()));
        if (!string.IsNullOrWhiteSpace(categoria) && Enum.TryParse<ProductCategory>(categoria, out var cat))
            products = products.Where(p => p.Categoria == cat);
        if (minPrecio.HasValue) products = products.Where(p => (float)p.Precio >= minPrecio.Value);
        if (maxPrecio.HasValue) products = products.Where(p => (float)p.Precio <= maxPrecio.Value);

        products = products.OrderByDescending(p => p.UpdatedAt);

        var totalItems = products.Count();
        var totalPages = (int)Math.Ceiling(totalItems / (double)size);
        var currentPage = Math.Max(1, Math.Min(page, Math.Max(1, totalPages)));

        var pagedProducts = products
            .Skip((currentPage - 1) * size)
            .Take(size)
            .ToList();

        ViewData["CurrentPage"] = currentPage;
        ViewData["Size"] = size;
        ViewData["TotalElements"] = totalItems;
        ViewData["TotalPages"] = totalPages;
        ViewData["HasPrevious"] = currentPage > 1;
        ViewData["HasNext"] = currentPage < totalPages;
        ViewData["Search"] = q;
        ViewData["Categoria"] = categoria;
        ViewData["MinPrecio"] = minPrecio;
        ViewData["MaxPrecio"] = maxPrecio;

        Products = pagedProducts;
        return Page();
    }
}
