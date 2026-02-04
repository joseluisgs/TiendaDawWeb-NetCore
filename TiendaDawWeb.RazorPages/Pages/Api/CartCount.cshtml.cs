using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;

namespace TiendaDawWeb.RazorPages.Pages.Api;

/// <summary>
///     API para obtener el número de items en el carrito
/// </summary>
[Authorize]
public class CartCountModel(
    ICarritoService carritoService,
    UserManager<User> userManager
) : PageModel {
    /// <summary>
    ///     GET /Api/CartCount - Devuelve el número de items en el carrito
    /// </summary>
    /// <returns>JSON con el count</returns>
    public async Task<IActionResult> OnGetAsync() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { count = 0 });
        }

        var result = await carritoService.GetCarritoCountAsync(user.Id);
        
        return new JsonResult(new { 
            count = result.IsSuccess ? result.Value : 0
        });
    }
}
