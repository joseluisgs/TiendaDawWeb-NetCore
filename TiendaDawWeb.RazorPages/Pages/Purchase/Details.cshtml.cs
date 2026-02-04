using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Purchase;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.RazorPages.Pages.Purchase;

[Authorize]
[IgnoreAntiforgeryToken]
public class DetailsModel(
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

        if (purchase.CompradorId != user.Id && !User.IsInRole("ADMIN")) {
            TempData["Error"] = "No tienes permiso para ver esta compra";
            return RedirectToPage("/Purchase/Index");
        }

        Purchase = purchase;
        return Page();
    }

    public async Task<IActionResult> OnPostReenviarEmailAsync(long id) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { success = false, message = "Usuario no autenticado" }) { StatusCode = 401 };
        }

        var purchaseResult = await purchaseService.GetByIdAsync(id);
        if (purchaseResult.IsFailure) {
            return new JsonResult(new { success = false, message = "Compra no encontrada" });
        }

        var purchase = purchaseResult.Value;
        if (purchase.CompradorId != user.Id && !User.IsInRole("ADMIN")) {
            return new JsonResult(new { success = false, message = "No tienes permiso" });
        }

        var result = await purchaseService.SendConfirmationEmailAsync(id);

        if (result.IsFailure) {
            return new JsonResult(new { success = false, message = result.Error.Message });
        }

        return new JsonResult(new { success = true, message = "Email reenviado correctamente" });
    }

    public async Task<IActionResult> OnGetDownloadPdfAsync(long id) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var purchaseResult = await purchaseService.GetByIdAsync(id);
        if (purchaseResult.IsFailure) {
            TempData["Error"] = "Compra no encontrada";
            return RedirectToPage("/Purchase/Index");
        }

        var purchase = purchaseResult.Value;
        if (purchase.CompradorId != user.Id && !User.IsInRole("ADMIN")) {
            TempData["Error"] = "No tienes permiso para descargar esta factura";
            return RedirectToPage("/Purchase/Index");
        }

        var pdfResult = await purchaseService.GeneratePdfAsync(id);

        if (pdfResult.IsFailure) {
            TempData["Error"] = pdfResult.Error.Message;
            return RedirectToPage("/Purchase/Details", new { id });
        }

        return File(pdfResult.Value, "application/pdf", $"factura-{id}.pdf");
    }
}
