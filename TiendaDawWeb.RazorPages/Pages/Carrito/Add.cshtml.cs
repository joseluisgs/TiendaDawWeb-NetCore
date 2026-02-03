using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;

namespace TiendaDawWeb.RazorPages.Pages.Carrito;

[Authorize]
public class AddModel(
    ICarritoService carritoService,
    UserManager<User> userManager
) : PageModel {
    public async Task<IActionResult> OnGetAsync([FromQuery] long productoId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            return new JsonResult(new { success = false, message = "Debes iniciar sesión" });
        }

        var result = await carritoService.AddToCarritoAsync(user.Id, productoId);
        
        if (result.IsFailure) {
            return new JsonResult(new { success = false, message = result.Error });
        }

        var countResult = await carritoService.GetCarritoCountAsync(user.Id);
        return new JsonResult(new { 
            success = true, 
            message = "Producto añadido al carrito",
            count = countResult.IsSuccess ? countResult.Value : 0
        });
    }
}
