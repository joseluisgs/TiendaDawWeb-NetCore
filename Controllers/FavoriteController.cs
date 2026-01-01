using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Controllers;

/// <summary>
/// Controlador para gestión de favoritos (requiere autenticación)
/// </summary>
[Authorize]
public class FavoriteController : Controller
{
    private readonly IFavoriteService _favoriteService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<FavoriteController> _logger;

    public FavoriteController(
        IFavoriteService favoriteService,
        UserManager<User> userManager,
        ILogger<FavoriteController> logger)
    {
        _favoriteService = favoriteService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Listado de productos favoritos del usuario
    /// </summary>
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _favoriteService.GetUserFavoritesAsync(user.Id);
        
        if (result.IsFailure)
        {
            TempData["Error"] = "Error al cargar favoritos";
            return View(Enumerable.Empty<Product>());
        }

        return View(result.Value);
    }

    /// <summary>
    /// Añadir producto a favoritos (API endpoint)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Add(long productId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "No autorizado" });
        }

        var result = await _favoriteService.AddFavoriteAsync(user.Id, productId);
        
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Message });
        }

        return Json(new { success = true, message = "Añadido a favoritos" });
    }

    /// <summary>
    /// Quitar producto de favoritos (API endpoint)
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Remove(long productId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Json(new { success = false, message = "No autorizado" });
        }

        var result = await _favoriteService.RemoveFavoriteAsync(user.Id, productId);
        
        if (result.IsFailure)
        {
            return Json(new { success = false, message = result.Error.Message });
        }

        return Json(new { success = true, message = "Eliminado de favoritos" });
    }
}
