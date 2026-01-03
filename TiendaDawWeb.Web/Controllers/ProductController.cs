using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;
using TiendaDawWeb.ViewModels;
using TiendaDawWeb.Web.Mappers;
using Microsoft.AspNetCore.SignalR;
using TiendaDawWeb.Web.Hubs;

namespace TiendaDawWeb.Controllers;

/// <summary>
///     Controlador para gestión de productos (requiere autenticación)
/// </summary>
[Authorize]
public class ProductController(
    IProductService productService,
    IStorageService storageService,
    UserManager<User> userManager,
    IHubContext<NotificationHub> hubContext,
    ILogger<ProductController> logger
) : Controller {
    private readonly ILogger<ProductController> _logger = logger;

    /// <summary>
    ///     Listado de productos (vista autenticada)
    /// </summary>
    public async Task<IActionResult> Index() {
        var result = await productService.GetAllAsync();

        if (result.IsFailure) {
            TempData["Error"] = "Error al cargar los productos";
            return View(Enumerable.Empty<Product>());
        }

        return View(result.Value);
    }

    /// <summary>
    ///     Detalle de un producto
    /// </summary>
    [AllowAnonymous]
    public async Task<IActionResult> Details(long id) {
        var result = await productService.GetByIdAsync(id);

        if (result.IsFailure) {
            TempData["Error"] = "Producto no encontrado";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    /// <summary>
    ///     Formulario para crear nuevo producto
    /// </summary>
    [HttpGet]
    public IActionResult Create() {
        return View();
    }

    /// <summary>
    ///     Procesar creación de producto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model) {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        string? imagenUrl = null;
        if (model.ImagenFile != null) {
            var saveResult = await storageService.SaveFileAsync(model.ImagenFile, "products");
            if (saveResult.IsSuccess) imagenUrl = saveResult.Value;
        }

        // 🧹 REFACTOR: Usamos el Mapper para construir la entidad
        var product = model.ToEntity(user.Id, imagenUrl);

        var result = await productService.CreateAsync(product);

        if (result.IsFailure) {
            TempData["Error"] = result.Error.Message;
            return View(model);
        }

        // 🔔 NOTIFICACIÓN EN TIEMPO REAL: Informamos a todos los usuarios del nuevo producto
        await hubContext.Clients.All.SendAsync("ReceiveNotification", 
            "¡Nuevo Producto!", 
            $"Se ha publicado: {product.Nombre}",
            result.Value.Id); // <--- Enviamos el ID para poder generar el enlace

        TempData["Success"] = "Producto creado exitosamente";
        return RedirectToAction(nameof(Details), new { id = result.Value.Id });
    }

    /// <summary>
    ///     Formulario para editar producto
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(long id) {
        var result = await productService.GetByIdAsync(id);

        if (result.IsFailure) {
            TempData["Error"] = "Producto no encontrado";
            return RedirectToAction(nameof(Index));
        }

        var product = result.Value;
        var user = await userManager.GetUserAsync(User);

        if (user == null || product.PropietarioId != user.Id) {
            TempData["Error"] = "No tienes permiso para editar este producto";
            return RedirectToAction(nameof(Index));
        }

        // 🧹 REFACTOR: Usamos el método de extensión .ToViewModel()
        return View(product.ToViewModel());
    }

    /// <summary>
    ///     Procesar edición de producto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, ProductViewModel model) {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var imagenUrl = model.ImagenUrl;
        if (model.ImagenFile != null) {
            var saveResult = await storageService.SaveFileAsync(model.ImagenFile, "products");
            if (saveResult.IsSuccess) imagenUrl = saveResult.Value;
        }

        // 🧹 REFACTOR: Usamos el Mapper para construir la entidad actualizada
        var product = model.ToEntity(user.Id, imagenUrl);

        var result = await productService.UpdateAsync(id, product, user.Id);

        if (result.IsFailure) {
            TempData["Error"] = result.Error.Message;
            return View(model);
        }

        TempData["Success"] = "Producto actualizado exitosamente";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    ///     Eliminar producto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var result = await productService.DeleteAsync(id, user.Id, User.IsInRole("ADMIN"));

        if (result.IsFailure)
            TempData["Error"] = result.Error.Message;
        else
            TempData["Success"] = "Producto eliminado exitosamente";

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    ///     Mis productos
    /// </summary>
    public async Task<IActionResult> MyProducts() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var result = await productService.GetAllAsync();

        if (result.IsFailure) return View(Enumerable.Empty<Product>());

        var myProducts = result.Value.Where(p => p.PropietarioId == user.Id);
        return View(myProducts);
    }
}