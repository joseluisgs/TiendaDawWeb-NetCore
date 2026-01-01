using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Controllers;

/// <summary>
/// Controlador para la gestión de compras realizadas
/// </summary>
[Authorize]
[Route("app/compras")]
public class PurchaseController : Controller
{
    private readonly IPurchaseService _purchaseService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<PurchaseController> _logger;

    public PurchaseController(
        IPurchaseService purchaseService,
        UserManager<User> userManager,
        ILogger<PurchaseController> logger)
    {
        _purchaseService = purchaseService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// GET /app/compras - Mis compras (paginadas)
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index(int page = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _purchaseService.GetByUserAsync(user.Id, page, 10);
        
        if (result.IsFailure)
        {
            TempData["Error"] = "Error al cargar las compras";
            return View(Enumerable.Empty<Purchase>());
        }

        ViewBag.CurrentPage = page;
        return View(result.Value);
    }

    /// <summary>
    /// GET /Purchase/MyPurchases - Alias para Mis compras (matching navbar link)
    /// </summary>
    [HttpGet("/Purchase/MyPurchases")]
    public async Task<IActionResult> MyPurchases(int page = 1)
    {
        return await Index(page);
    }

    /// <summary>
    /// GET /app/compras/{id} - Detalle de compra
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(long id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _purchaseService.GetByIdAsync(id);
        
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        var purchase = result.Value;

        // Verificar que el usuario sea el comprador (o sea admin)
        if (purchase.CompradorId != user.Id && !User.IsInRole("ADMIN"))
        {
            TempData["Error"] = "No tienes permiso para ver esta compra";
            return RedirectToAction(nameof(Index));
        }

        return View(purchase);
    }

    /// <summary>
    /// GET /app/compras/{id}/pdf - Descargar factura PDF
    /// </summary>
    [HttpGet("{id}/pdf")]
    public async Task<IActionResult> DownloadPdf(long id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        // Verificar que la compra pertenezca al usuario
        var purchaseResult = await _purchaseService.GetByIdAsync(id);
        if (purchaseResult.IsFailure)
        {
            TempData["Error"] = "Compra no encontrada";
            return RedirectToAction(nameof(Index));
        }

        var purchase = purchaseResult.Value;
        if (purchase.CompradorId != user.Id && !User.IsInRole("ADMIN"))
        {
            TempData["Error"] = "No tienes permiso para descargar esta factura";
            return RedirectToAction(nameof(Index));
        }

        var pdfResult = await _purchaseService.GeneratePdfAsync(id);
        
        if (pdfResult.IsFailure)
        {
            TempData["Error"] = pdfResult.Error.Message;
            return RedirectToAction(nameof(Details), new { id });
        }

        return File(pdfResult.Value, "application/pdf", $"factura-{id}.pdf");
    }

    /// <summary>
    /// GET /app/compras/{id}/confirmacion - Página post-compra
    /// </summary>
    [HttpGet("{id}/confirmacion")]
    public async Task<IActionResult> Confirmacion(long id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _purchaseService.GetByIdAsync(id);
        
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        var purchase = result.Value;

        // Verificar que el usuario sea el comprador
        if (purchase.CompradorId != user.Id)
        {
            TempData["Error"] = "No tienes permiso para ver esta compra";
            return RedirectToAction(nameof(Index));
        }

        return View(purchase);
    }
}
