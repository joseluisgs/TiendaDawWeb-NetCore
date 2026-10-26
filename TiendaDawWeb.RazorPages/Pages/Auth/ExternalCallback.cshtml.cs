using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.RazorPages.Pages.Auth;

/// <summary>
///     Callback del proveedor OAuth 2.0 para Razor Pages.
///     Identity procesa la respuesta, vincula/autentica y crea la cookie.
/// </summary>
[IgnoreAntiforgeryToken]
public class ExternalCallbackModel(
    SignInManager<User> signInManager,
    UserManager<User> userManager,
    ILogger<ExternalCallbackModel> logger
) : PageModel
{
    /// <summary>
    ///     Procesa el callback del proveedor OAuth y autentica al usuario.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(string? returnUrl = null) {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null) {
            logger.LogWarning("Login externo falló: sin información del proveedor");
            TempData["Error"] = "No se pudo autenticar con el proveedor externo";
            return RedirectToPage("/Auth/Login");
        }

        var result = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);

        if (result.Succeeded) {
            logger.LogInformation("Login externo exitoso con {Provider}", info.LoginProvider);
            return LocalRedirect(returnUrl ?? "/");
        }

        // Primera vez: crear cuenta local vinculada
        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email)) {
            TempData["Error"] = "El proveedor no devolvió email";
            return RedirectToPage("/Auth/Login");
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null) {
            var nameParts = (info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "").Split(' ', 2);
            user = new User {
                UserName = email,
                Email = email,
                Nombre = nameParts[0] ?? "",
                Apellidos = nameParts.Length > 1 ? nameParts[1] : "",
                Rol = "USER",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded) {
                TempData["Error"] = "Error creando usuario: " +
                    string.Join(", ", createResult.Errors.Select(e => e.Description));
                return RedirectToPage("/Auth/Login");
            }
            logger.LogInformation("Usuario OAuth creado: {Email} ({Provider})", email, info.LoginProvider);
        }

        var addLoginResult = await userManager.AddLoginAsync(user, info);
        if (addLoginResult.Succeeded) {
            await signInManager.SignInAsync(user, isPersistent: false);
            logger.LogInformation("Login externo completado: {Email}", email);
            return LocalRedirect(returnUrl ?? "/");
        }

        TempData["Error"] = "Error vinculando cuenta externa";
        return RedirectToPage("/Auth/Login");
    }
}
