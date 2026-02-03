using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Storage;
using TiendaDawWeb.Shared.ViewModels;
using TiendaDawWeb.Shared.Mappers;

namespace TiendaDawWeb.Web.RazorPages.Pages.Product;

[Authorize]
public class EditModel(
    IProductService productService,
    IStorageService storageService,
    UserManager<User> userManager
) : PageModel {
    [BindProperty]
    public ProductViewModel Input { get; set; } = default!;
    public ProductViewModel ProductViewModel { get { return Input; } }

    public async Task<IActionResult> OnGetAsync(long id) {
        var result = await productService.GetByIdAsync(id);

        if (result.IsFailure) {
            TempData["Error"] = "Producto no encontrado";
            return RedirectToPage("/Public/Index");
        }

        var product = result.Value;
        var user = await userManager.GetUserAsync(User);

        if (user == null || product.PropietarioId != user.Id) {
            TempData["Error"] = "No tienes permiso para editar este producto";
            return RedirectToPage("/Product/Index");
        }

        Input = product.ToViewModel();
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(long id) {
        if (!ModelState.IsValid)
            return Page();

        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        var imagenUrl = Input.ImagenUrl;
        if (Input.ImagenFile != null) {
            var saveResult = await storageService.SaveFileAsync(Input.ImagenFile, "products");
            if (saveResult.IsSuccess) imagenUrl = saveResult.Value;
        }

        var product = Input.ToEntity(user.Id, imagenUrl, includeId: true);

        var result = await productService.UpdateAsync(id, product, user.Id);

        if (result.IsFailure) {
            TempData["Error"] = result.Error.Message;
            return Page();
        }

        TempData["Success"] = "Producto actualizado exitosamente";
        return RedirectToPage("/Product/Details", new { id });
    }
}
