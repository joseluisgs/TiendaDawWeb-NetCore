using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Favorite;
using TiendaDawWeb.Shared.Services.Product;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.RazorPages.Pages.Product;

[Authorize]
public class IndexModel(
    IProductService productService,
    IFavoriteService favoriteService,
    UserManager<User> userManager
) : PageModel {
    public IEnumerable<ProductModel> Products { get; set; } = Enumerable.Empty<ProductModel>();

    public async Task<IActionResult> OnGetAsync() {
        Result<IEnumerable<ProductModel>, DomainError> result = await productService.GetAllAsync();

        if (result.IsFailure) {
            TempData["Error"] = "Error al cargar los productos";
            Products = Enumerable.Empty<ProductModel>();
            return Page();
        }

        if (User.Identity?.IsAuthenticated == true) {
            User? user = await userManager.GetUserAsync(User);
            if (user != null) {
                Result<IEnumerable<ProductModel>, DomainError> favoritesResult =
                    await favoriteService.GetUserFavoritesAsync(user.Id);
                if (favoritesResult.IsSuccess) {
                    ViewData["FavoriteIds"] = favoritesResult.Value.Select(p => p.Id).ToList();
                }
            }
        }

        Products = result.Value;
        return Page();
    }
}
