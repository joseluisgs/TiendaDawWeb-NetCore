using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;

namespace TiendaDawWeb.RazorPages.Pages.Carrito;

[Authorize]
public class IndexModel(
    ICarritoService carritoService,
    UserManager<User> userManager
) : PageModel {
    public List<CarritoItem> CarritoItems { get; set; } = new();
    public IEnumerable<CarritoItem> Items => CarritoItems;

    public async Task<IActionResult> OnGetAsync() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var result = await carritoService.GetCarritoByUsuarioIdAsync(user.Id);

        if (result.IsFailure) {
            TempData["Error"] = "Error al cargar el carrito";
            CarritoItems = new List<CarritoItem>();
            return Page();
        }

        CarritoItems = result.Value.ToList();

        var totalResult = await carritoService.GetTotalCarritoAsync(user.Id);
        ViewData["Total"] = totalResult.IsSuccess ? totalResult.Value : 0;

        return Page();
    }
}
