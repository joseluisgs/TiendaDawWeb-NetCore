using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Favorite;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.Web.RazorPages.Pages.Product;

[Authorize]
public class IndexModel(
    IProductService productService,
    IFavoriteService favoriteService,
    UserManager<User> userManager
) : PageModel {
    public IEnumerable<ProductModel> Products { get; set; } = Enumerable.Empty<ProductModel>();

    public async Task<IActionResult> OnGetAsync() {
        var result = await productService.GetAllAsync();

        if (result.IsFailure) {
            TempData["Error"] = "Error al cargar los productos";
            Products = Enumerable.Empty<ProductModel>();
            return Page();
        }

        if (User.Identity?.IsAuthenticated == true) {
            var user = await userManager.GetUserAsync(User);
            if (user != null) {
                var favoritesResult = await favoriteService.GetUserFavoritesAsync(user.Id);
                if (favoritesResult.IsSuccess) {
                    ViewData["FavoriteIds"] = favoritesResult.Value.Select(p => p.Id).ToList();
                }
            }
        }

        Products = result.Value;
        return Page();
    }
}
