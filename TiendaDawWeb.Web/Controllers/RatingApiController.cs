using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Controllers;

/// <summary>
///     REST API Controller for rating operations (AJAX)
/// </summary>
[ApiController]
[Route("api/ratings")]
[Authorize]
public class RatingApiController(
    IRatingService ratingService,
    UserManager<User> userManager,
    ILogger<RatingApiController> logger
) : ControllerBase {
    private readonly ILogger<RatingApiController> _logger = logger;

    /// <summary>
    ///     POST /api/ratings - Add or update rating
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddRating([FromBody] AddRatingRequest request) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return Unauthorized(new { success = false, message = "Usuario no autenticado" });

        if (request.Puntuacion < 1 || request.Puntuacion > 5)
            return BadRequest(new { success = false, message = "La puntuación debe estar entre 1 y 5" });

        var result = await ratingService.AddRatingAsync(
            user.Id,
            request.ProductId,
            request.Puntuacion,
            request.Comentario);

        if (result.IsFailure) return BadRequest(new { success = false, message = result.Error.Message });

        return Ok(new {
            success = true,
            message = "Valoración añadida correctamente",
            rating = new {
                puntuacion = result.Value.Puntuacion,
                comentario = result.Value.Comentario
            }
        });
    }

    /// <summary>
    ///     GET /api/ratings/product/{productId} - Get product ratings
    /// </summary>
    [HttpGet("product/{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetProductRatings(long productId) {
        var result = await ratingService.GetByProductoIdAsync(productId);

        if (result.IsFailure) return BadRequest(new { success = false, message = result.Error.Message });

        var ratings = result.Value.Select(r => new {
            id = r.Id,
            puntuacion = r.Puntuacion,
            comentario = r.Comentario,
            fecha = r.CreatedAt,
            usuario = new {
                nombre = r.Usuario?.Nombre ?? "Usuario",
                apellidos = r.Usuario?.Apellidos ?? "",
                avatar = r.Usuario?.Avatar ?? "https://robohash.org/default?size=50x50"
            }
        });

        return Ok(new { success = true, ratings });
    }

    /// <summary>
    ///     GET /api/ratings/user/{productId} - Get user's rating for a product
    /// </summary>
    [HttpGet("user/{productId}")]
    public async Task<IActionResult> GetUserRating(long productId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null)
            return Unauthorized(new { success = false });
        var result = await ratingService.GetByProductoIdAsync(productId);
        if (result.IsFailure)
            return Ok(new { success = true, rating = (object?)null });

        // 👇— AJUSTE CLAVE —👇
        var rating = result.Value.FirstOrDefault(r => r.UsuarioId == user.Id);
        if (rating == null)
            return Ok(new { success = true, rating = (object?)null });
        return Ok(new {
            success = true,
            rating = new {
                puntuacion = rating.Puntuacion,
                comentario = rating.Comentario,
                fecha = rating.CreatedAt
            }
        });
    }
}

public record AddRatingRequest(long ProductId, int Puntuacion, string? Comentario);