using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Web.RazorPages.Pages.Profile;

[Authorize]
public class ChangePasswordModel(
    UserManager<User> userManager,
    ILogger<ChangePasswordModel> logger
) : PageModel {
    public void OnGet() {
    }

    public async Task<IActionResult> OnPostAsync(string currentPassword, string newPassword, string confirmPassword) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToPage("/Auth/Login");

        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword)) {
            TempData["Error"] = "Todos los campos son obligatorios";
            return Page();
        }

        if (newPassword != confirmPassword) {
            TempData["Error"] = "Las contraseñas no coinciden";
            return Page();
        }

        if (newPassword.Length < 4) {
            TempData["Error"] = "La contraseña debe tener al menos 4 caracteres";
            return Page();
        }

        var changeResult = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (changeResult.Succeeded) {
            logger.LogInformation("Contraseña cambiada para usuario {UserId}", user.Id);
            TempData["Success"] = "Contraseña cambiada correctamente";
            return RedirectToPage("/Profile/Index");
        }

        TempData["Error"] = "Error al cambiar la contraseña: " +
                            string.Join(", ", changeResult.Errors.Select(e => e.Description));
        return Page();
    }
}
