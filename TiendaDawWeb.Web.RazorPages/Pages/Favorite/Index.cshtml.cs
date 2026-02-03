using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Favorite;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.Web.RazorPages.Pages.Favorite;

[Authorize]
public class IndexModel(
    IFavoriteService favoriteService,
    UserManager<User> userManager
) : PageModel {
    public IEnumerable<ProductModel> Products { get; set; } = Enumerable.Empty<ProductModel>();
    public IEnumerable<ProductModel> Productos => Products;

    public async Task<IActionResult> OnGetAsync() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var result = await favoriteService.GetUserFavoritesAsync(user.Id);

        if (result.IsFailure) {
            TempData["Error"] = "Error al cargar favoritos";
            Products = Enumerable.Empty<ProductModel>();
            return Page();
        }

        Products = result.Value;
        return Page();
    }
}
