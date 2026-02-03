using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Localization;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Favorite;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.RazorPages.Pages.Product;

[AllowAnonymous]
public class DetailsModel(
    IProductService productService,
    IFavoriteService favoriteService,
    UserManager<User> userManager
) : PageModel {
    public ProductModel Product { get; set; } = default!;

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
}
