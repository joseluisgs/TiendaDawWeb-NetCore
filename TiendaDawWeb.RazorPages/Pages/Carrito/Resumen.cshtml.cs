using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;
using TiendaDawWeb.Shared.Services.Purchase;

namespace TiendaDawWeb.RazorPages.Pages.Carrito;

[Authorize]
public class ResumenModel(
    ICarritoService carritoService,
    IPurchaseService purchaseService,
    UserManager<User> userManager,
    ILogger<ResumenModel> logger
) : PageModel {
    public List<CarritoItem> CarritoItems { get; set; } = new();
    public IEnumerable<CarritoItem> Items => CarritoItems;

    public async Task<IActionResult> OnGetAsync() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var result = await carritoService.GetCarritoByUsuarioIdAsync(user.Id);

        if (result.IsFailure || !result.Value.Any()) {
            TempData["Error"] = "El carrito está vacío";
            return RedirectToPage("/Carrito/Index");
        }

        CarritoItems = result.Value.ToList();

        var totalResult = await carritoService.GetTotalCarritoAsync(user.Id);
        ViewData["Total"] = totalResult.IsSuccess ? totalResult.Value : 0;
        ViewData["User"] = user;

        return Page();
    }

    public async Task<IActionResult> OnPostFinalizarCompraAsync() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        logger.LogInformation("Usuario {UserId} iniciando proceso de compra", user.Id);

        var result = await purchaseService.CreatePurchaseFromCarritoAsync(user.Id);

        if (result.IsFailure) {
            logger.LogWarning("Error al finalizar compra para usuario {UserId}: {Error}",
                user.Id, result.Error.Message);
            TempData["Error"] = result.Error.Message;
            return RedirectToPage();
        }

        var purchase = result.Value;
        logger.LogInformation("Compra {PurchaseId} finalizada exitosamente para usuario {UserId}",
            purchase.Id, user.Id);

        TempData["Success"] = "¡Compra realizada con éxito!";
        return RedirectToPage("/Purchase/Details", new { id = purchase.Id });
    }
}
