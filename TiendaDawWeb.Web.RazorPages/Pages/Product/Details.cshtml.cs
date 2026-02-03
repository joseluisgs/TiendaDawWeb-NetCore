using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Favorite;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.Web.RazorPages.Pages.Product;

[AllowAnonymous]
public class DetailsModel(
    IProductService productService,
    IFavoriteService favoriteService,
    UserManager<User> userManager
) : PageModel {
    public ProductModel Product { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(long id) {
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
