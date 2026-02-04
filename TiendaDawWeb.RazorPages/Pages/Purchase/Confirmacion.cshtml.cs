using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Purchase;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.RazorPages.Pages.Purchase;

/// <summary>
///     Modelo de página para mostrar la confirmación de una compra
/// </summary>
[Authorize]
public class ConfirmacionModel(
    IPurchaseService purchaseService,
    UserManager<User> userManager
) : PageModel {
    public PurchaseModel Purchase { get; set; } = default!;

    /// <summary>
    ///     GET /Purchase/Confirmacion/{id} - Muestra la confirmación de compra y envía email
    /// </summary>
    /// <param name="id">ID de la compra</param>
    /// <returns>Vista de confirmación</returns>
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

        _ = purchaseService.SendConfirmationEmailAsync(id);

        Purchase = purchase;
        return Page();
    }
}
