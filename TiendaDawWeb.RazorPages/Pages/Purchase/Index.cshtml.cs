using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Purchase;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.RazorPages.Pages.Purchase;

[Authorize]
public class IndexModel(
    IPurchaseService purchaseService,
    UserManager<User> userManager
) : PageModel {
    public IEnumerable<PurchaseModel> Purchases { get; set; } = Enumerable.Empty<PurchaseModel>();

    public async Task<IActionResult> OnGetAsync(int page = 1) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var result = await purchaseService.GetByUserAsync(user.Id, page);

        if (result.IsFailure) {
            TempData["Error"] = "Error al cargar las compras";
            Purchases = Enumerable.Empty<PurchaseModel>();
            return Page();
        }

        ViewData["CurrentPage"] = page;
        Purchases = result.Value;
        return Page();
    }
}
