using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Purchase;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.RazorPages.Pages.Purchase;

/// <summary>
///     Modelo de página para mostrar los detalles de una compra
/// </summary>
[Authorize]
[IgnoreAntiforgeryToken]
public class DetailsModel(
    IPurchaseService purchaseService,
    UserManager<User> userManager
) : PageModel {
    public PurchaseModel Purchase { get; set; } = default!;

    /// <summary>
    ///     GET /Purchase/Details/{id} - Muestra los detalles de una compra
    /// </summary>
    /// <param name="id">ID de la compra</param>
    /// <returns>Vista con los detalles de la compra</returns>
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

    /// <summary>
    ///     POST /Purchase/Details/ReenviarEmail - Reenvía el email de confirmación
    /// </summary>
    /// <param name="id">ID de la compra</param>
    /// <returns>JSON con el resultado</returns>
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

    /// <summary>
    ///     GET /Purchase/DownloadPdf/{id} - Descarga la factura en PDF
    /// </summary>
    /// <param name="id">ID de la compra</param>
    /// <returns>Archivo PDF</returns>
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
