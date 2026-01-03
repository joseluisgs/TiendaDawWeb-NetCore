using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Controllers;

/// <summary>
///     Controlador para la gestión del carrito de compras
/// </summary>
[Authorize]
[Route("app/carrito")]
public class CarritoController(
    ICarritoService carritoService,
    IPurchaseService purchaseService,
    IProductService productService,
    UserManager<User> userManager,
    ILogger<CarritoController> logger
) : Controller {
    /// <summary>
    ///     GET /app/carrito - Ver carrito
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var result = await carritoService.GetCarritoByUsuarioIdAsync(user.Id);

        if (result.IsFailure) {
            TempData["Error"] = "Error al cargar el carrito";
            return View(Enumerable.Empty<CarritoItem>());
        }

        var carritoItems = result.Value.ToList();

        // Calcular el total
        var totalResult = await carritoService.GetTotalCarritoAsync(user.Id);
        ViewBag.Total = totalResult.IsSuccess ? totalResult.Value : 0;

        return View(carritoItems);
    }

    /// <summary>
    ///     POST /Carrito/Add - Añadir producto al carrito (sin cantidad)
    /// </summary>
    [HttpPost]
    [Route("/Carrito/Add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(long productoId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        // Verificar si el producto está reservado
        var productResult = await productService.GetByIdAsync(productoId);
        if (productResult.IsFailure) {
            TempData["Error"] = "Producto no encontrado";
            return RedirectToAction("Details", "Product", new { id = productoId });
        }

        var product = productResult.Value;
        if (product.Reservado) {
            TempData["Error"] = "Este producto está reservado y no se puede añadir al carrito";
            return RedirectToAction("Details", "Product", new { id = productoId });
        }

        var result = await carritoService.AddToCarritoAsync(user.Id, productoId);

        if (result.IsFailure)
            TempData["Error"] = result.Error.Message;
        else
            TempData["Success"] = "Producto añadido al carrito";

        return RedirectToAction("Details", "Product", new { id = productoId });
    }

    /// <summary>
    ///     POST /app/carrito/add - Añadir producto (ruta alternativa, sin cantidad)
    /// </summary>
    [HttpPost("add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(long productId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var result = await carritoService.AddToCarritoAsync(user.Id, productId);

        if (result.IsFailure)
            TempData["Error"] = result.Error.Message;
        else
            TempData["Success"] = "Producto añadido al carrito";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    ///     POST /app/carrito/remove - Eliminar item
    /// </summary>
    [HttpPost("remove")]
    [Route("/Carrito/remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Remove(long itemId) {
        var result = await carritoService.RemoveFromCarritoAsync(itemId);

        if (result.IsFailure)
            TempData["Error"] = result.Error.Message;
        else
            TempData["Success"] = "Producto eliminado del carrito";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    ///     POST /app/carrito/clear - Vaciar carrito
    /// </summary>
    [HttpPost("clear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var result = await carritoService.ClearCarritoAsync(user.Id);

        if (result.IsFailure)
            TempData["Error"] = result.Error.Message;
        else
            TempData["Success"] = "Carrito vaciado";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    ///     GET /app/carrito/resumen - Vista previa compra
    /// </summary>
    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var result = await carritoService.GetCarritoByUsuarioIdAsync(user.Id);

        if (result.IsFailure || !result.Value.Any()) {
            TempData["Error"] = "El carrito está vacío";
            return RedirectToAction(nameof(Index));
        }

        var carritoItems = result.Value.ToList();

        // Calcular el total
        var totalResult = await carritoService.GetTotalCarritoAsync(user.Id);
        ViewBag.Total = totalResult.IsSuccess ? totalResult.Value : 0;
        ViewBag.User = user;

        return View(carritoItems);
    }

    /// <summary>
    ///     POST /app/carrito/finalizar - Procesar compra con control de concurrencia
    /// </summary>
    [HttpPost("finalizar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizarCompra() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        logger.LogInformation("Usuario {UserId} iniciando proceso de compra", user.Id);

        // Crear la compra con control de concurrencia
        var result = await purchaseService.CreatePurchaseFromCarritoAsync(user.Id);

        if (result.IsFailure) {
            logger.LogWarning("Error al finalizar compra para usuario {UserId}: {Error}",
                user.Id, result.Error.Message);
            TempData["Error"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        var purchase = result.Value;
        logger.LogInformation("Compra {PurchaseId} finalizada exitosamente para usuario {UserId}",
            purchase.Id, user.Id);

        TempData["Success"] = "¡Compra realizada con éxito!";

        // Redirigir a página de confirmación
        return RedirectToAction("Confirmacion", "Purchase", new { id = purchase.Id });
    }
}