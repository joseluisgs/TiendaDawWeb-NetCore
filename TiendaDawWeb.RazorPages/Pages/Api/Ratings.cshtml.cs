using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Rating;

namespace TiendaDawWeb.RazorPages.Pages.Api;

/// <summary>
///     API para gestión de valoraciones de productos
/// </summary>
[Authorize]
[IgnoreAntiforgeryToken]
public class RatingsModel(
    IRatingService ratingService,
    UserManager<User> userManager
) : PageModel {
    /// <summary>
    ///     GET /Api/Ratings?productId={id} - Obtiene rating promedio del producto
    /// </summary>
    /// <param name="productId">ID del producto</param>
    /// <returns>JSON con los ratings</returns>
    public async Task<IActionResult> OnGetProductRatingAsync([FromQuery] long productId) {
        Result<double, DomainError> averageResult = await ratingService.GetAverageRatingAsync(productId);
        Result<IEnumerable<Rating>, DomainError> ratingsResult = await ratingService.GetByProductoIdAsync(productId);

        double average = averageResult.IsSuccess ? averageResult.Value : 0;
        List<Rating> ratings = ratingsResult.IsSuccess ? ratingsResult.Value.ToList() : new List<Rating>();

        User? user = await userManager.GetUserAsync(User);
        Rating? userRating = ratings.FirstOrDefault(r => r.UsuarioId == user?.Id);
        bool hasRated = userRating != null;
        bool canRate = !hasRated;

        return new JsonResult(new {
            average,
            count = ratings.Count,
            hasRated,
            canRate,
            rating = average > 0 ? Math.Round(average, 1) : 0,
            userRating = userRating?.Puntuacion ?? 0
        });
    }

    /// <summary>
    ///     GET /Api/Ratings?handler=UserRating&productId={id} - Obtiene rating del usuario actual
    /// </summary>
    /// <param name="productId">ID del producto</param>
    /// <returns>JSON con el rating del usuario</returns>
    public async Task<IActionResult> OnGetUserRatingAsync([FromQuery] long productId) {
        User? user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { rated = false, rating = 0 });
        }

        Result<IEnumerable<Rating>, DomainError> ratingsResult = await ratingService.GetByProductoIdAsync(productId);
        if (ratingsResult.IsFailure) {
            return new JsonResult(new { rated = false, rating = 0 });
        }

        Rating? userRating = ratingsResult.Value.FirstOrDefault(r => r.UsuarioId == user.Id);

        return new JsonResult(new {
            rated = userRating != null,
            rating = userRating?.Puntuacion ?? 0,
            comment = userRating?.Comentario ?? ""
        });
    }

    /// <summary>
    ///     POST /Api/Ratings - Añade una valoración
    /// </summary>
    /// <param name="request">Datos de la valoración</param>
    /// <returns>JSON con el resultado</returns>
    public async Task<IActionResult> OnPostAsync([FromBody] RatingRequest request) {
        User? user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { success = false, message = "Debes iniciar sesión" });
        }

        if (request.ProductId <= 0 || request.Rating < 1 || request.Rating > 5) {
            return new JsonResult(new { success = false, message = "Datos inválidos" });
        }

        Result<Rating, DomainError> result =
            await ratingService.AddRatingAsync(user.Id, request.ProductId, request.Rating, request.Comment ?? "");

        if (result.IsFailure) {
            return new JsonResult(new { success = false, message = result.Error });
        }

        return new JsonResult(new {
            success = true,
            message = "Valoración guardada correctamente",
            rating = request.Rating
        });
    }

    /// <summary>
    ///     PUT /Api/Ratings - Actualiza una valoración
    /// </summary>
    /// <param name="request">Datos de la valoración</param>
    /// <returns>JSON con el resultado</returns>
    public async Task<IActionResult> OnPutAsync([FromBody] RatingRequest request) {
        User? user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { success = false, message = "Debes iniciar sesión" });
        }

        if (request.ProductId <= 0 || request.Rating < 1 || request.Rating > 5) {
            return new JsonResult(new { success = false, message = "Datos inválidos" });
        }

        Result<IEnumerable<Rating>, DomainError> ratingsResult =
            await ratingService.GetByProductoIdAsync(request.ProductId);
        if (ratingsResult.IsFailure) {
            return new JsonResult(new { success = false, message = "Producto no encontrado" });
        }

        Rating? userRating = ratingsResult.Value.FirstOrDefault(r => r.UsuarioId == user.Id);
        if (userRating == null) {
            return new JsonResult(new { success = false, message = "No tienes valoración para actualizar" });
        }

        Result<Rating, DomainError> result =
            await ratingService.UpdateRatingAsync(userRating.Id, user.Id, request.Rating, request.Comment ?? "");

        if (result.IsFailure) {
            return new JsonResult(new { success = false, message = result.Error });
        }

        return new JsonResult(new {
            success = true,
            message = "Valoración actualizada correctamente",
            rating = request.Rating
        });
    }

    /// <summary>
    ///     DELETE /Api/Ratings?productId={id} - Elimina una valoración
    /// </summary>
    /// <param name="productId">ID del producto</param>
    /// <returns>JSON con el resultado</returns>
    public async Task<IActionResult> OnDeleteAsync([FromQuery] long productId) {
        User? user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { success = false, message = "Debes iniciar sesión" });
        }

        Result<IEnumerable<Rating>, DomainError> ratingsResult = await ratingService.GetByProductoIdAsync(productId);
        if (ratingsResult.IsFailure) {
            return new JsonResult(new { success = false, message = "Producto no encontrado" });
        }

        Rating? userRating = ratingsResult.Value.FirstOrDefault(r => r.UsuarioId == user.Id);
        if (userRating == null) {
            return new JsonResult(new { success = false, message = "No tienes valoración para eliminar" });
        }

        Result<bool, DomainError> result = await ratingService.DeleteRatingAsync(userRating.Id, user.Id);

        if (result.IsFailure) {
            return new JsonResult(new { success = false, message = result.Error });
        }

        return new JsonResult(new { success = true, message = "Valoración eliminada" });
    }
}

/// <summary>
///     Request para operaciones de rating
/// </summary>
public class RatingRequest {
    public long ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
