using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Purchase;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.Web.RazorPages.Pages.Purchase;

[Authorize]
public class ConfirmacionModel(
    IPurchaseService purchaseService,
    UserManager<User> userManager
) : PageModel {
    public PurchaseModel Purchase { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(long id) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var result = await purchaseService.GetByIdAsync(id);

        if (result.IsFailure) {
            TempData["Error"] = result.Error.Message;
            return RedirectToPage("/Purchase/Index");
        }

        var purchase = result.Value;

        if (purchase.CompradorId != user.Id) {
            TempData["Error"] = "No tienes permiso para ver esta compra";
            return RedirectToPage("/Purchase/Index");
        }

        Purchase = purchase;
        return Page();
    }
}
