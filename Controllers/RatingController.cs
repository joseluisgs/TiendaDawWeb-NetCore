using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;
using TiendaDawWeb.ViewModels;

namespace TiendaDawWeb.Controllers;

/// <summary>
/// Controlador para la gestión de valoraciones de productos
/// </summary>
[Authorize]
[Route("app/ratings")]
public class RatingController : Controller
{
    private readonly IRatingService _ratingService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<RatingController> _logger;

    public RatingController(
        IRatingService ratingService,
        UserManager<User> userManager,
        ILogger<RatingController> logger)
    {
        _ratingService = ratingService;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// POST /app/ratings/add - Añadir valoración a un producto
    /// </summary>
    [HttpPost("add")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRating([FromForm] RatingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Datos de valoración inválidos";
            return RedirectToAction("Details", "Product", new { id = model.ProductoId });
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _ratingService.AddRatingAsync(
            user.Id, 
            model.ProductoId, 
            model.Puntuacion, 
            model.Comentario);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
        }
        else
        {
            TempData["Success"] = "¡Valoración añadida correctamente!";
        }

        return RedirectToAction("Details", "Product", new { id = model.ProductoId });
    }

    /// <summary>
    /// GET /app/ratings/{id} - Ver detalle de una valoración
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> Details(long id)
    {
        var result = await _ratingService.GetByIdAsync(id);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
            return RedirectToAction("Index", "Public");
        }

        return View(result.Value);
    }

    /// <summary>
    /// GET /app/ratings/{id}/edit - Formulario para editar valoración
    /// </summary>
    [HttpGet("{id}/edit")]
    public async Task<IActionResult> Edit(long id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _ratingService.GetByIdAsync(id);
        
        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
            return RedirectToAction("Index", "Public");
        }

        var rating = result.Value;

        // Verificar que sea el propietario
        if (rating.UsuarioId != user.Id)
        {
            TempData["Error"] = "No tienes permiso para editar esta valoración";
            return RedirectToAction("Details", "Product", new { id = rating.ProductoId });
        }

        var viewModel = new RatingViewModel
        {
            ProductoId = rating.ProductoId,
            Puntuacion = rating.Puntuacion,
            Comentario = rating.Comentario
        };

        return View(viewModel);
    }

    /// <summary>
    /// POST /app/ratings/{id}/edit - Actualizar valoración
    /// </summary>
    [HttpPost("{id}/edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, [FromForm] RatingViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var result = await _ratingService.UpdateRatingAsync(
            id, 
            user.Id, 
            model.Puntuacion, 
            model.Comentario);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
        }
        else
        {
            TempData["Success"] = "Valoración actualizada correctamente";
        }

        return RedirectToAction("Details", "Product", new { id = model.ProductoId });
    }

    /// <summary>
    /// POST /app/ratings/{id}/delete - Eliminar valoración
    /// </summary>
    [HttpPost("{id}/delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(long id, long productoId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var isAdmin = User.IsInRole("ADMIN");
        var result = await _ratingService.DeleteRatingAsync(id, user.Id, isAdmin);

        if (result.IsFailure)
        {
            TempData["Error"] = result.Error.Message;
        }
        else
        {
            TempData["Success"] = "Valoración eliminada correctamente";
        }

        return RedirectToAction("Details", "Product", new { id = productoId });
    }
}
