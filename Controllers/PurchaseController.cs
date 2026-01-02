using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Controllers;

/// <summary>
///     Controlador para la gestión de compras realizadas
/// </summary>
[Authorize]
[Route("app/compras")]
public class PurchaseController(
    IPurchaseService purchaseService,
    UserManager<User> userManager,
    ILogger<PurchaseController> logger
) : Controller {
    private readonly ILogger<PurchaseController> _logger = logger;

    /// <summary>
    ///     GET /app/compras or /Purchase/MyPurchases - Mis compras (paginadas)
    /// </summary>
    [HttpGet]
    [HttpGet("/Purchase/MyPurchases")]
    public async Task<IActionResult> Index(int page = 1) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var result = await purchaseService.GetByUserAsync(user.Id, page);

        if (result.IsFailure) {
            TempData["Error"] = "Error al cargar las compras";
            return View(Enumerable.Empty<Purchase>());
        }

        ViewBag.CurrentPage = page;
        return View(result.Value);
    }

    /// <summary>
    ///     GET /app/compras/{id} - Detalle de compra
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(long id) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var result = await purchaseService.GetByIdAsync(id);

        if (result.IsFailure) {
            TempData["Error"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        var purchase = result.Value;

        // Verificar que el usuario sea el comprador (o sea admin)
        if (purchase.CompradorId != user.Id && !User.IsInRole("ADMIN")) {
            TempData["Error"] = "No tienes permiso para ver esta compra";
            return RedirectToAction(nameof(Index));
        }

        return View(purchase);
    }

    /// <summary>
    ///     GET /app/compras/{id}/pdf - Descargar factura PDF
    /// </summary>
    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadPdf(long id) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        // Verificar que la compra pertenezca al usuario
        var purchaseResult = await purchaseService.GetByIdAsync(id);
        if (purchaseResult.IsFailure) {
            TempData["Error"] = "Compra no encontrada";
            return RedirectToAction(nameof(Index));
        }

        var purchase = purchaseResult.Value;
        if (purchase.CompradorId != user.Id && !User.IsInRole("ADMIN")) {
            TempData["Error"] = "No tienes permiso para descargar esta factura";
            return RedirectToAction(nameof(Index));
        }

        var pdfResult = await purchaseService.GeneratePdfAsync(id);

        if (pdfResult.IsFailure) {
            TempData["Error"] = pdfResult.Error.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        return File(pdfResult.Value, "application/pdf", $"factura-{id}.pdf");
    }

    /// <summary>
    ///     GET /app/compras/{id}/confirmacion - Página post-compra
    /// </summary>
    [HttpGet("{id}/confirmacion")]
    public async Task<IActionResult> Confirmacion(long id) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var result = await purchaseService.GetByIdAsync(id);

        if (result.IsFailure) {
            TempData["Error"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        var purchase = result.Value;

        // Verificar que el usuario sea el comprador
        if (purchase.CompradorId != user.Id) {
            TempData["Error"] = "No tienes permiso para ver esta compra";
            return RedirectToAction(nameof(Index));
        }

        return View(purchase);
    }
}