using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Controllers;

/// <summary>
/// Controlador para la gestión del carrito de compras
/// </summary>
[Authorize]
[Route("app/carrito")]
public class CarritoController : Controller
{
    private readonly ICarritoService _carritoService;
    private readonly IPurchaseService _purchaseService;
    private readonly IProductService _productService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<CarritoController> _logger;

    public CarritoController(
        ICarritoService carritoService,
        IPurchaseService purchaseService,
        IProductService productService,
        UserManager<User> userManager,
        ILogger<CarritoController> logger)
    {
        _carritoService = carritoService;
        _purchaseService = purchaseService;
        _productService = productService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// GET /app/carrito - Ver carrito
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _carritoService.GetCarritoByUsuarioIdAsync(user.Id);
        
        if (result.IsFailure)
        {
            TempData["Error"] = "Error al cargar el carrito";
            return View(Enumerable.Empty<CarritoItem>());
        }

        var carritoItems = result.Value.ToList();
        
        // Calcular el total
        var totalResult = await _carritoService.GetTotalCarritoAsync(user.Id);
        ViewBag.Total = totalResult.IsSuccess ? totalResult.Value : 0;

        return View(carritoItems);
    }

    /// <summary>
    /// POST /app/carrito/add - Añadir producto
    /// </summary>
    [HttpPost("add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddToCart(long productId, int cantidad = 1)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _carritoService.AddToCarritoAsync(user.Id, productId, cantidad);
        
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
        }
        else
        {
            TempData["Success"] = $"Producto añadido al carrito (cantidad: {cantidad})";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// POST /app/carrito/update - Actualizar cantidad
    /// </summary>
    [HttpPost("update")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateQuantity(long itemId, int cantidad)
    {
        var result = await _carritoService.UpdateCantidadAsync(itemId, cantidad);
        
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
        }
        else
        {
            TempData["Success"] = "Cantidad actualizada";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// POST /app/carrito/remove - Eliminar item
    /// </summary>
    [HttpPost("remove")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveItem(long itemId)
    {
        var result = await _carritoService.RemoveFromCarritoAsync(itemId);
        
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
        }
        else
        {
            TempData["Success"] = "Producto eliminado del carrito";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// POST /app/carrito/clear - Vaciar carrito
    /// </summary>
    [HttpPost("clear")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Clear()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _carritoService.ClearCarritoAsync(user.Id);
        
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
        }
        else
        {
            TempData["Success"] = "Carrito vaciado";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// GET /app/carrito/resumen - Vista previa compra
    /// </summary>
    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _carritoService.GetCarritoByUsuarioIdAsync(user.Id);
        
        if (result.IsFailure || !result.Value.Any())
        {
            TempData["Error"] = "El carrito está vacío";
            return RedirectToAction(nameof(Index));
        }

        var carritoItems = result.Value.ToList();
        
        // Calcular el total
        var totalResult = await _carritoService.GetTotalCarritoAsync(user.Id);
        ViewBag.Total = totalResult.IsSuccess ? totalResult.Value : 0;
        ViewBag.User = user;

        return View(carritoItems);
    }

    /// <summary>
    /// POST /app/carrito/finalizar - Procesar compra con control de concurrencia
    /// </summary>
    [HttpPost("finalizar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> FinalizarCompra()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        _logger.LogInformation("Usuario {UserId} iniciando proceso de compra", user.Id);

        // Crear la compra con control de concurrencia
        var result = await _purchaseService.CreatePurchaseFromCarritoAsync(user.Id);

        if (result.IsFailure)
        {
            _logger.LogWarning("Error al finalizar compra para usuario {UserId}: {Error}", 
                user.Id, result.Error.Message);
            TempData["Error"] = result.Error.Message;
            return RedirectToAction(nameof(Index));
        }

        var purchase = result.Value;
        _logger.LogInformation("Compra {PurchaseId} finalizada exitosamente para usuario {UserId}", 
            purchase.Id, user.Id);

        TempData["Success"] = "¡Compra realizada con éxito!";
        
        // Redirigir a página de confirmación
        return RedirectToAction("Confirmacion", "Purchase", new { id = purchase.Id });
    }
}
