using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Rating;

namespace TiendaDawWeb.RazorPages.Pages.Api;

[Authorize]
[IgnoreAntiforgeryToken]
public class RatingsModel(
    IRatingService ratingService,
    UserManager<User> userManager
) : PageModel {
    public async Task<IActionResult> OnGetProductRatingAsync([FromQuery] long productId) {
        var averageResult = await ratingService.GetAverageRatingAsync(productId);
        var ratingsResult = await ratingService.GetByProductoIdAsync(productId);
        
        var average = averageResult.IsSuccess ? averageResult.Value : 0;
        var ratings = ratingsResult.IsSuccess ? ratingsResult.Value.ToList() : new List<Rating>();
        
        var user = await userManager.GetUserAsync(User);
        var userRating = ratings.FirstOrDefault(r => r.UsuarioId == user?.Id);
        var hasRated = userRating != null;
        var canRate = !hasRated;

        return new JsonResult(new {
            average = average,
            count = ratings.Count,
            hasRated = hasRated,
            canRate = canRate,
            rating = average > 0 ? Math.Round(average, 1) : 0,
            userRating = userRating?.Puntuacion ?? 0
        });
    }

    public async Task<IActionResult> OnGetUserRatingAsync([FromQuery] long productId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { rated = false, rating = 0 });
        }

        var ratingsResult = await ratingService.GetByProductoIdAsync(productId);
        if (ratingsResult.IsFailure) {
            return new JsonResult(new { rated = false, rating = 0 });
        }

        var userRating = ratingsResult.Value.FirstOrDefault(r => r.UsuarioId == user.Id);
        
        return new JsonResult(new { 
            rated = userRating != null, 
            rating = userRating?.Puntuacion ?? 0,
            comment = userRating?.Comentario ?? ""
        });
    }

    public async Task<IActionResult> OnPostAsync([FromBody] RatingRequest request) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { success = false, message = "Debes iniciar sesión" });
        }

        if (request.ProductId <= 0 || request.Rating < 1 || request.Rating > 5) {
            return new JsonResult(new { success = false, message = "Datos inválidos" });
        }

        var result = await ratingService.AddRatingAsync(user.Id, request.ProductId, request.Rating, request.Comment ?? "");
        
        if (result.IsFailure) {
            return new JsonResult(new { success = false, message = result.Error });
        }

        return new JsonResult(new { 
            success = true, 
            message = "Valoración guardada correctamente",
            rating = request.Rating 
        });
    }

    public async Task<IActionResult> OnPutAsync([FromBody] RatingRequest request) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { success = false, message = "Debes iniciar sesión" });
        }

        if (request.ProductId <= 0 || request.Rating < 1 || request.Rating > 5) {
            return new JsonResult(new { success = false, message = "Datos inválidos" });
        }

        var ratingsResult = await ratingService.GetByProductoIdAsync(request.ProductId);
        if (ratingsResult.IsFailure) {
            return new JsonResult(new { success = false, message = "Producto no encontrado" });
        }

        var userRating = ratingsResult.Value.FirstOrDefault(r => r.UsuarioId == user.Id);
        if (userRating == null) {
            return new JsonResult(new { success = false, message = "No tienes valoración para actualizar" });
        }

        var result = await ratingService.UpdateRatingAsync(userRating.Id, user.Id, request.Rating, request.Comment ?? "");
        
        if (result.IsFailure) {
            return new JsonResult(new { success = false, message = result.Error });
        }

        return new JsonResult(new { 
            success = true, 
            message = "Valoración actualizada correctamente",
            rating = request.Rating 
        });
    }

    public async Task<IActionResult> OnDeleteAsync([FromQuery] long productId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { success = false, message = "Debes iniciar sesión" });
        }

        var ratingsResult = await ratingService.GetByProductoIdAsync(productId);
        if (ratingsResult.IsFailure) {
            return new JsonResult(new { success = false, message = "Producto no encontrado" });
        }

        var userRating = ratingsResult.Value.FirstOrDefault(r => r.UsuarioId == user.Id);
        if (userRating == null) {
            return new JsonResult(new { success = false, message = "No tienes valoración para eliminar" });
        }

        var result = await ratingService.DeleteRatingAsync(userRating.Id, user.Id);
        
        if (result.IsFailure) {
            return new JsonResult(new { success = false, message = result.Error });
        }

        return new JsonResult(new { success = true, message = "Valoración eliminada" });
    }
}

public class RatingRequest {
    public long ProductId { get; set; }
    public int Rating { get; set; }
    public string? Comment { get; set; }
}
