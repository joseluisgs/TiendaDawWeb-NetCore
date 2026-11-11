using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;

namespace TiendaDawWeb.Controllers;

/// <summary>
///     API para operaciones del carrito
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class CartCountController(
    ICarritoService carritoService,
    UserManager<User> userManager
) : ControllerBase {
    /// <summary>
    ///     GET /api/cartcount - Obtiene la cantidad de items en el carrito
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetCartCount() {
        User? user = await userManager.GetUserAsync(User);
        if (user == null) {
            return Ok(new { count = 0 });
        }

        Result<int, DomainError> result = await carritoService.GetCarritoCountAsync(user.Id);

        return Ok(new {
            count = result.IsSuccess ? result.Value : 0
        });
    }
}
