using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Storage;
using TiendaDawWeb.Shared.ViewModels;
using TiendaDawWeb.Shared.Mappers;

namespace TiendaDawWeb.RazorPages.Pages.Product;

[Authorize]
public class CreateModel(
    IProductService productService,
    IStorageService storageService,
    UserManager<User> userManager
) : PageModel {
    [BindProperty]
    public ProductViewModel Input { get; set; } = default!;
    public ProductViewModel ProductViewModel => Input;

    public void OnGet() {
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid)
            return Page();

        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        string? imagenUrl = null;
        if (Input.ImagenFile != null) {
            var saveResult = await storageService.SaveFileAsync(Input.ImagenFile, "products");
            if (saveResult.IsSuccess) imagenUrl = saveResult.Value;
        }

        var product = Input.ToEntity(user.Id, imagenUrl);

        var result = await productService.CreateAsync(product);

        if (result.IsFailure) {
            TempData["Error"] = result.Error.Message;
            return Page();
        }

        TempData["Success"] = "Producto creado exitosamente";
        return RedirectToPage("/Product/Details", new { id = result.Value.Id });
    }
}
