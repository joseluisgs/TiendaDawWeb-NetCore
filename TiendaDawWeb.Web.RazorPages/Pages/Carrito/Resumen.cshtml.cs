using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;

namespace TiendaDawWeb.Web.RazorPages.Pages.Carrito;

[Authorize]
public class ResumenModel(
    ICarritoService carritoService,
    UserManager<User> userManager
) : PageModel {
    public List<CarritoItem> CarritoItems { get; set; } = new();
    public IEnumerable<CarritoItem> Items => CarritoItems;

    public async Task<IActionResult> OnGetAsync() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var result = await carritoService.GetCarritoByUsuarioIdAsync(user.Id);

        if (result.IsFailure || !result.Value.Any()) {
            TempData["Error"] = "El carrito está vacío";
            return RedirectToPage("/Carrito/Index");
        }

        CarritoItems = result.Value.ToList();

        var totalResult = await carritoService.GetTotalCarritoAsync(user.Id);
        ViewData["Total"] = totalResult.IsSuccess ? totalResult.Value : 0;
        ViewData["User"] = user;

        return Page();
    }
}
