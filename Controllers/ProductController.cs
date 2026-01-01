using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Interfaces;
using TiendaDawWeb.ViewModels;

namespace TiendaDawWeb.Controllers;

/// <summary>
/// Controlador para gestión de productos (requiere autenticación)
/// </summary>
[Authorize]
public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly IStorageService _storageService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<ProductController> _logger;

    public ProductController(
        IProductService productService,
        IStorageService storageService,
        UserManager<User> userManager,
        ILogger<ProductController> logger)
    {
        _productService = productService;
        _storageService = storageService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Listado de productos (vista autenticada)
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var result = await _productService.GetAllAsync();
        
        if (result.IsFailure)
        {
            TempData["Error"] = "Error al cargar los productos";
            return View(Enumerable.Empty<Product>());
        }

        return View(result.Value);
    }

    /// <summary>
    /// Detalle de un producto
    /// </summary>
    public async Task<IActionResult> Details(long id)
    {
        var result = await _productService.GetByIdAsync(id);
        
        if (result.IsFailure)
        {
            TempData["Error"] = "Producto no encontrado";
            return RedirectToAction(nameof(Index));
        }

        return View(result.Value);
    }

    /// <summary>
    /// Formulario para crear nuevo producto
    /// </summary>
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    /// <summary>
    /// Procesar creación de producto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        string? imagenUrl = null;
        if (model.ImagenFile != null)
        {
            var saveResult = await _storageService.SaveFileAsync(model.ImagenFile, "products");
            if (saveResult.IsSuccess)
            {
                imagenUrl = saveResult.Value;
            }
        }

        var product = new Product
        {
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Precio = model.Precio,
            Categoria = model.Categoria,
            PropietarioId = user.Id,
            Imagen = imagenUrl
        };

        var result = await _productService.CreateAsync(product);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
            return View(model);
        }

        TempData["Success"] = "Producto creado exitosamente";
        return RedirectToAction(nameof(Details), new { id = result.Value.Id });
    }

    /// <summary>
    /// Formulario para editar producto
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Edit(long id)
    {
        var result = await _productService.GetByIdAsync(id);
        
        if (result.IsFailure)
        {
            TempData["Error"] = "Producto no encontrado";
            return RedirectToAction(nameof(Index));
        }

        var product = result.Value;
        var user = await _userManager.GetUserAsync(User);
        
        if (user == null || product.PropietarioId != user.Id)
        {
            TempData["Error"] = "No tienes permiso para editar este producto";
            return RedirectToAction(nameof(Index));
        }

        var viewModel = new ProductViewModel
        {
            Id = product.Id,
            Nombre = product.Nombre,
            Descripcion = product.Descripcion,
            Precio = product.Precio,
            Categoria = product.Categoria,
            ImagenUrl = product.Imagen
        };

        return View(viewModel);
    }

    /// <summary>
    /// Procesar edición de producto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, ProductViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        string? imagenUrl = model.ImagenUrl;
        if (model.ImagenFile != null)
        {
            var saveResult = await _storageService.SaveFileAsync(model.ImagenFile, "products");
            if (saveResult.IsSuccess)
            {
                imagenUrl = saveResult.Value;
            }
        }

        var product = new Product
        {
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Precio = model.Precio,
            Categoria = model.Categoria,
            Imagen = imagenUrl
        };

        var result = await _productService.UpdateAsync(id, product, user.Id);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
            return View(model);
        }

        TempData["Success"] = "Producto actualizado exitosamente";
        return RedirectToAction(nameof(Details), new { id });
    }

    /// <summary>
    /// Eliminar producto
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _productService.DeleteAsync(id, user.Id, User.IsInRole("ADMIN"));

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
        }
        else
        {
            TempData["Success"] = "Producto eliminado exitosamente";
        }

        return RedirectToAction(nameof(Index));
    }

    /// <summary>
    /// Mis productos
    /// </summary>
    public async Task<IActionResult> MyProducts()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _productService.GetAllAsync();
        
        if (result.IsFailure)
        {
            return View(Enumerable.Empty<Product>());
        }

        var myProducts = result.Value.Where(p => p.PropietarioId == user.Id);
        return View(myProducts);
    }
}
