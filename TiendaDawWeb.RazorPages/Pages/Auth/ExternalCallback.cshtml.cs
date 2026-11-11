using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

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
) : PageModel {
    /// <summary>
    ///     Procesa el callback del proveedor OAuth y autentica al usuario.
    /// </summary>
    public async Task<IActionResult> OnGetAsync(string? returnUrl = null) {
        ExternalLoginInfo? info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null) {
            logger.LogWarning("Login externo falló: sin información del proveedor");
            TempData["Error"] = "No se pudo autenticar con el proveedor externo";
            return RedirectToPage("/Auth/Login");
        }

        SignInResult result = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, false);

        if (result.Succeeded) {
            logger.LogInformation("Login externo exitoso con {Provider}", info.LoginProvider);
            return LocalRedirect(returnUrl ?? "/");
        }

        // Primera vez: crear cuenta local vinculada
        string? email = info.Principal.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email)) {
            TempData["Error"] = "El proveedor no devolvió email";
            return RedirectToPage("/Auth/Login");
        }

        User? user = await userManager.FindByEmailAsync(email);
        if (user is null) {
            string[] nameParts = (info.Principal.FindFirst(ClaimTypes.Name)?.Value ?? "").Split(' ', 2);
            user = new User {
                UserName = email,
                Email = email,
                Nombre = nameParts[0] ?? "",
                Apellidos = nameParts.Length > 1 ? nameParts[1] : "",
                Rol = "USER",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            IdentityResult createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded) {
                TempData["Error"] = "Error creando usuario: " +
                                    string.Join(", ", createResult.Errors.Select(e => e.Description));
                return RedirectToPage("/Auth/Login");
            }

            logger.LogInformation("Usuario OAuth creado: {Email} ({Provider})", email, info.LoginProvider);
        }

        IdentityResult addLoginResult = await userManager.AddLoginAsync(user, info);
        if (addLoginResult.Succeeded) {
            await signInManager.SignInAsync(user, false);
            logger.LogInformation("Login externo completado: {Email}", email);
            return LocalRedirect(returnUrl ?? "/");
        }

        TempData["Error"] = "Error vinculando cuenta externa";
        return RedirectToPage("/Auth/Login");
    }
}
