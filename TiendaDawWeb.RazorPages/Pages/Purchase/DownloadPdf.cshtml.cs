using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Purchase;

namespace TiendaDawWeb.RazorPages.Pages.Purchase;

/// <summary>
///     Modelo de página para descargar la factura en PDF
/// </summary>
[Authorize]
public class DownloadPdfModel(
    IPurchaseService purchaseService,
    UserManager<User> userManager
) : PageModel {
    /// <summary>
    ///     GET /Purchase/DownloadPdf/{id} - Descarga la factura PDF de una compra
    /// </summary>
    /// <param name="id">ID de la compra</param>
    /// <returns>Archivo PDF de la factura</returns>
    public async Task<IActionResult> OnGetAsync(long id) {
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
