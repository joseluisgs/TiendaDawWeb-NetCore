using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.RazorPages.Pages.Profile;

/// <summary>
///     Zona de gestión de passkeys: lista las guardadas en la cuenta y contiene los
///     handlers que las crean y las eliminan. Los handlers se llaman desde
///     passkeys.js con el parámetro de handler en la URL.
///     El [Authorize] va en la página entera: en Razor Pages la autorización se
///     aplica a la página, no a un handler suelto.
/// </summary>
/// <param name="userManager">Gestor de usuarios: cuenta conectada y passkeys guardadas.</param>
/// <param name="signInManager">Gestor de accesos: opciones y atestación de passkeys.</param>
/// <param name="logger">Logger de la página.</param>
[Authorize]
public class PasskeysModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<PasskeysModel> logger
) : PageModel {
    /// <summary>Passkeys guardadas en la cuenta conectada.</summary>
    public List<UserPasskeyInfo> Passkeys { get; private set; } = [];

    /// <summary>
    ///     Pinta la lista de passkeys de la cuenta.
    /// </summary>
    /// <returns>La propia página, o 302 al acceso si la cookie ya no vale.</returns>
    public async Task<IActionResult> OnGetAsync() {
        User? user = await userManager.GetUserAsync(User);
        if (user == null) {
            return RedirectToPage("/Auth/Login");
        }

        Passkeys = (await userManager.GetPasskeysAsync(user)).ToList();
        return Page();
    }

    /// <summary>
    ///     Primer paso de crear una passkey: devuelve las opciones WebAuthn en JSON.
    /// </summary>
    /// <returns>JSON con las opciones para navigator.credentials.create().</returns>
    public async Task<IActionResult> OnPostCreationOptionsAsync() {
        User? user = await userManager.GetUserAsync(User);
        if (user == null) {
            return NotFound();
        }

        string nombre = user.UserName ?? user.Email ?? "usuario";

        string opciones = await signInManager.MakePasskeyCreationOptionsAsync(new PasskeyUserEntity {
            Id = await userManager.GetUserIdAsync(user),
            Name = nombre,
            DisplayName = nombre
        });

        return Content(opciones, "application/json");
    }

    /// <summary>
    ///     Segundo paso: verifica la atestación de la credencial creada y la guarda
    ///     en AspNetUserPasskeys. El estado del reto nombra a una cuenta y se
    ///     comprueba que es la conectada antes de escribir nada.
    /// </summary>
    /// <param name="credential">Credencial devuelta por el navegador, en JSON.</param>
    /// <returns>JSON con ok y, si falla, el motivo.</returns>
    public async Task<IActionResult> OnPostAttestationAsync(string credential) {
        User? user = await userManager.GetUserAsync(User);
        if (user == null) {
            return NotFound();
        }

        PasskeyAttestationResult result = await signInManager.PerformPasskeyAttestationAsync(credential);
        if (!result.Succeeded) {
            return StatusCode(StatusCodes.Status400BadRequest, new {
                ok = false,
                mensaje = result.Failure?.Message ?? "No se pudo verificar la passkey"
            });
        }

        string userId = await userManager.GetUserIdAsync(user);
        if (!string.Equals(result.UserEntity?.Id, userId, StringComparison.Ordinal)) {
            return StatusCode(StatusCodes.Status400BadRequest, new {
                ok = false,
                mensaje = "La passkey no pertenece a la cuenta que está conectada"
            });
        }

        IdentityResult guardado = await userManager.AddOrUpdatePasskeyAsync(user, result.Passkey);
        if (!guardado.Succeeded) {
            return StatusCode(StatusCodes.Status400BadRequest, new {
                ok = false,
                mensaje = "No se pudo guardar la passkey"
            });
        }

        logger.LogInformation("Passkey creada para usuario {UserId}", user.Id);
        return new JsonResult(new { ok = true });
    }

    /// <summary>
    ///     Elimina una passkey de la cuenta. El id llega en base64url, la forma en
    ///     que WebAuthn lo maneja en el navegador; la base de datos lo guarda como bytes.
    /// </summary>
    /// <param name="id">Identificador de la credencial en base64url.</param>
    /// <returns>302 a la propia página con el aviso en TempData.</returns>
    public async Task<IActionResult> OnPostEliminarAsync(string id) {
        User? user = await userManager.GetUserAsync(User);
        if (user == null) {
            return RedirectToPage("/Auth/Login");
        }

        try {
            byte[] credencial = DeBase64Url(id);
            IdentityResult result = await userManager.RemovePasskeyAsync(user, credencial);
            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Passkey eliminada"
                : "No se pudo eliminar la passkey";

            if (result.Succeeded) {
                logger.LogInformation("Passkey eliminada para usuario {UserId}", user.Id);
            }
        } catch (FormatException) {
            TempData["Error"] = "El identificador de la passkey no es válido";
        }

        return RedirectToPage();
    }

    /// <summary>De base64url (WebAuthn) a los bytes que guarda Identity.</summary>
    private static byte[] DeBase64Url(string id) {
        string base64 = id.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + (4 - base64.Length % 4) % 4, '=');
        return Convert.FromBase64String(base64);
    }
}
