using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.RazorPages.Pages.Product;

[Authorize]
public class MyProductsModel(
    IProductService productService,
    UserManager<User> userManager
) : PageModel {
    public IEnumerable<ProductModel> Products { get; set; } = Enumerable.Empty<ProductModel>();

    public async Task<IActionResult> OnGetAsync() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var result = await productService.GetAllAsync();

        if (result.IsFailure) {
            Products = Enumerable.Empty<ProductModel>();
            return Page();
        }

        Products = result.Value.Where(p => p.PropietarioId == user.Id);
        return Page();
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var result = await productService.DeleteAsync(id, user.Id);
        
        if (result.IsFailure) {
            TempData["Error"] = result.Error;
        } else {
            TempData["Success"] = "Producto eliminado correctamente";
        }
        
        return RedirectToPage("/Product/MyProducts");
    }
}
