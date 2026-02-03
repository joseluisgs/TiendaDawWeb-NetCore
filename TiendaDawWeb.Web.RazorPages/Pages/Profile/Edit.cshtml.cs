using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Storage;

namespace TiendaDawWeb.Web.RazorPages.Pages.Profile;

[Authorize]
public class EditModel(
    UserManager<User> userManager,
    IStorageService storageService,
    ILogger<EditModel> logger
) : PageModel {
    public User UserProfile { get; set; } = default!;
    public User Usuario { get { return UserProfile; } }

    public async Task<IActionResult> OnGetAsync() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        UserProfile = user;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string nombre, string apellidos, IFormFile? avatar) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellidos)) {
            TempData["Error"] = "El nombre y apellidos son obligatorios";
            UserProfile = user;
            return Page();
        }

        user.Nombre = nombre.Trim();
        user.Apellidos = apellidos.Trim();

        if (avatar != null && avatar.Length > 0) {
            if (avatar.Length > 5 * 1024 * 1024) {
                TempData["Error"] = "El archivo es demasiado grande. Máximo 5MB";
                UserProfile = user;
                return Page();
            }

            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(avatar.ContentType.ToLower())) {
                TempData["Error"] = "Solo se permiten imágenes (JPG, PNG, GIF)";
                UserProfile = user;
                return Page();
            }

            try {
                var result = await storageService.SaveFileAsync(avatar, "avatars");
                if (result.IsSuccess) {
                    if (!string.IsNullOrEmpty(user.Avatar)) await storageService.DeleteFileAsync(user.Avatar);
                    user.Avatar = result.Value;
                }
                else {
                    TempData["Error"] = "Error al guardar el avatar: " + result.Error.Message;
                    UserProfile = user;
                    return Page();
                }
            }
            catch (Exception ex) {
                logger.LogError(ex, "Error al procesar avatar para usuario {UserId}", user.Id);
                TempData["Error"] = "Error al procesar la imagen";
                UserProfile = user;
                return Page();
            }
        }

        var updateResult = await userManager.UpdateAsync(user);
        if (updateResult.Succeeded) {
            logger.LogInformation("Perfil actualizado para usuario {UserId}", user.Id);
            TempData["Success"] = "Perfil actualizado correctamente";
            return RedirectToPage("/Profile/Index");
        }

        TempData["Error"] = "Error al actualizar el perfil: " +
                            string.Join(", ", updateResult.Errors.Select(e => e.Description));
        UserProfile = user;
        return Page();
    }
}
