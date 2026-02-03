using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Storage;
using TiendaDawWeb.Shared.ViewModels;
using TiendaDawWeb.Shared.Mappers;

namespace TiendaDawWeb.RazorPages.Pages.Product;

[Authorize]
public class EditModel(
    IProductService productService,
    IStorageService storageService,
    UserManager<User> userManager,
    ILogger<EditModel> logger
) : PageModel {
    [BindProperty]
    public ProductViewModel Input { get; set; } = default!;
    public ProductViewModel ProductViewModel { get { return Input; } }

    private async Task LoadProductData(long id) {
        var result = await productService.GetByIdAsync(id);
        if (result.IsSuccess) {
            Input = result.Value.ToViewModel();
        }
    }

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

    public async Task<IActionResult> OnPostAsync() {
        logger.LogInformation("OnPostAsync llamado, ModelState.IsValid: {IsValid}", ModelState.IsValid);
        
        if (!ModelState.IsValid) {
            logger.LogWarning("ModelState inválido: {Errors}", ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)));
            await LoadProductData(Input.Id);
            return Page();
        }

        var user = await userManager.GetUserAsync(User);
        if (user == null) {
            logger.LogWarning("Usuario no encontrado");
            return RedirectToPage("/Auth/Login");
        }

        var imagenUrl = Input.ImagenUrl;
        if (Input.ImagenFile != null) {
            logger.LogInformation("Guardando nueva imagen...");
            var saveResult = await storageService.SaveFileAsync(Input.ImagenFile, "products");
            if (saveResult.IsSuccess) {
                imagenUrl = saveResult.Value;
                logger.LogInformation("Imagen guardada: {Url}", imagenUrl);
            } else {
                logger.LogWarning("Error guardando imagen: {Error}", saveResult.Error);
            }
        }

        var product = Input.ToEntity(user.Id, imagenUrl, includeId: true);

        logger.LogInformation("Actualizando producto {Id}", Input.Id);
        var result = await productService.UpdateAsync(Input.Id, product, user.Id);

        if (result.IsFailure) {
            logger.LogError("Error actualizando producto: {Error}", result.Error.Message);
            TempData["Error"] = result.Error.Message;
            await LoadProductData(Input.Id);
            return Page();
        }

        logger.LogInformation("Producto {Id} actualizado exitosamente", Input.Id);
        TempData["Success"] = "Producto actualizado exitosamente";
        return RedirectToPage("/Product/Details", new { id = Input.Id });
    }
}
