using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Localization;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Favorite;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.RazorPages.Pages.Product;

/// <summary>
///     Modelo de página para mostrar los detalles de un producto
/// </summary>
[AllowAnonymous]
public class DetailsModel(
    IProductService productService,
    ICarritoService carritoService,
    IFavoriteService favoriteService,
    UserManager<User> userManager
) : PageModel {
    public ProductModel Product { get; set; } = default!;

    /// <summary>
    ///     GET /Product/Details/{id} - Muestra los detalles de un producto
    /// </summary>
    /// <param name="id">ID del producto a mostrar</param>
    /// <param name="lang">Código de idioma para cambiar la cultura</param>
    /// <returns>Vista con los detalles del producto</returns>
    public async Task<IActionResult> OnGetAsync(long id, string? lang) {
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

            return RedirectToPage(new { id, lang = (string?)null });
        }

        var result = await productService.GetByIdAsync(id);

        if (result.IsFailure) {
            TempData["Error"] = "Producto no encontrado";
            return RedirectToPage("/Public/Index");
        }

        if (User.Identity?.IsAuthenticated == true) {
            var user = await userManager.GetUserAsync(User);
            if (user != null) {
                var favoriteResult = await favoriteService.IsFavoriteAsync(user.Id, id);
                ViewData["IsFavorite"] = favoriteResult.IsSuccess && favoriteResult.Value;
            }
        } else {
            ViewData["IsFavorite"] = false;
        }

        Product = result.Value;
        return Page();
    }

    /// <summary>
    ///     POST /Product/Details/Add - Añade un producto al carrito desde la página de detalles
    /// </summary>
    /// <param name="productoId">ID del producto a añadir</param>
    /// <returns>Redirect al carrito</returns>
    public async Task<IActionResult> OnPostAddAsync(long productoId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var productResult = await productService.GetByIdAsync(productoId);
        if (productResult.IsFailure) {
            TempData["Error"] = "Producto no encontrado";
            return RedirectToPage("/Public/Index");
        }

        var product = productResult.Value;
        if (product.Reservado) {
            TempData["Error"] = "Este producto está reservado";
            return RedirectToPage("/Public/Index");
        }

        var result = await carritoService.AddToCarritoAsync(user.Id, productoId);

        if (result.IsFailure)
            TempData["Error"] = result.Error.Message;
        else
            TempData["Success"] = "Producto añadido al carrito";

        return RedirectToPage("/Carrito/Index");
    }

    /// <summary>
    ///     POST /Product/Details/Delete - Elimina un producto (solo propietario)
    /// </summary>
    /// <param name="id">ID del producto a eliminar</param>
    /// <returns>Redirect a Mis Productos</returns>
    public async Task<IActionResult> OnPostDeleteAsync(long id) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return RedirectToPage("/Auth/Login");
        }

        var result = await productService.DeleteAsync(id, user.Id);
        
        if (result.IsFailure) {
            TempData["Error"] = result.Error;
            Product = (await productService.GetByIdAsync(id)).Value;
            return Page();
        }
        
        TempData["Success"] = "Producto eliminado correctamente";
        return RedirectToPage("/Product/MyProducts");
    }
}
