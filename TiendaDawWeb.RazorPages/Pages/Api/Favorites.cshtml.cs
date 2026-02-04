using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Favorite;

namespace TiendaDawWeb.RazorPages.Pages.Api;

[Authorize]
[IgnoreAntiforgeryToken]
public class FavoritesModel(
    IFavoriteService favoriteService,
    UserManager<User> userManager
) : PageModel {
    public async Task<IActionResult> OnGetToggleAsync(long productoId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { success = false, message = "Debes iniciar sesión" });
        }

        var isFavoriteResult = await favoriteService.IsFavoriteAsync(user.Id, productoId);
        if (isFavoriteResult.IsFailure) {
            return new JsonResult(new { success = false, message = isFavoriteResult.Error });
        }

        var isFavorite = isFavoriteResult.Value;

        if (isFavorite) {
            var removeResult = await favoriteService.RemoveFavoriteAsync(user.Id, productoId);
            if (removeResult.IsFailure) {
                return new JsonResult(new { success = false, message = removeResult.Error });
            }
            return new JsonResult(new { 
                success = true, 
                isFavorite = false,
                message = "Eliminado de favoritos"
            });
        } else {
            var addResult = await favoriteService.AddFavoriteAsync(user.Id, productoId);
            if (addResult.IsFailure) {
                return new JsonResult(new { success = false, message = addResult.Error });
            }
            return new JsonResult(new { 
                success = true, 
                isFavorite = true,
                message = "Añadido a favoritos"
            });
        }
    }

    public async Task<IActionResult> OnGetDeleteAsync(long productoId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { success = false, message = "Debes iniciar sesión" });
        }

        var result = await favoriteService.RemoveFavoriteAsync(user.Id, productoId);
        
        if (result.IsFailure) {
            return new JsonResult(new { success = false, message = result.Error });
        }

        return new JsonResult(new { 
            success = true, 
            message = "Eliminado de favoritos"
        });
    }

    public async Task<IActionResult> OnGetCheckAsync(long productoId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { success = false, message = "Usuario no autenticado" }) { StatusCode = 401 };
        }

        var result = await favoriteService.IsFavoriteAsync(user.Id, productoId);

        if (result.IsFailure) return new JsonResult(new { success = false, message = result.Error.Message }) { StatusCode = 400 };

        return new JsonResult(new { success = true, isFavorite = result.Value });
    }
}
