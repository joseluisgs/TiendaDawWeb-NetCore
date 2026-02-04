using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;
using TiendaDawWeb.Shared.Services.Product;

namespace TiendaDawWeb.RazorPages.Pages.Carrito;

[Authorize]
public class IndexModel(
    ICarritoService carritoService,
    IProductService productService,
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

    public async Task<IActionResult> OnPostAddAsync(long productoId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var productResult = await productService.GetByIdAsync(productoId);
        if (productResult.IsFailure) {
            TempData["Error"] = "Producto no encontrado";
            return RedirectToPage("/Public/Index");
        }

        var product = productResult.Value;
        if (product.Reservado) {
            TempData["Error"] = "Este producto está reservado";
            return RedirectToPage("/Public/Index");
        }

        var result = await carritoService.AddToCarritoAsync(user.Id, productoId);

        if (result.IsFailure)
            TempData["Error"] = result.Error.Message;
        else
            TempData["Success"] = "Producto añadido al carrito";

        return RedirectToPage("./Index");
    }

    public async Task<IActionResult> OnPostRemoveAsync(long itemId) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var result = await carritoService.RemoveFromCarritoAsync(itemId);
        
        if (result.IsFailure) {
            TempData["Error"] = result.Error;
        } else {
            TempData["Success"] = "Producto eliminado del carrito";
        }

        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostClearAsync() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var result = await carritoService.ClearCarritoAsync(user.Id);
        
        if (result.IsFailure) {
            TempData["Error"] = result.Error;
        } else {
            TempData["Success"] = "Carrito vaciado";
        }

        return RedirectToPage();
    }
}
