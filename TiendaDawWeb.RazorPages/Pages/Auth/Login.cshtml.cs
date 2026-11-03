using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.RazorPages.Pages.Auth;

public class LoginModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ILogger<LoginModel> logger
) : PageModel {
    [BindProperty]
    public string Email { get; set; } = string.Empty;

    [BindProperty]
    public string Password { get; set; } = string.Empty;

    [BindProperty]
    public bool RememberMe { get; set; }

    [BindProperty]
    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null) {
        ReturnUrl = returnUrl;
    }

    public async Task<IActionResult> OnPostAsync() {
        if (!ModelState.IsValid)
            return Page();

        var user = await userManager.FindByEmailAsync(Email);
        if (user == null) {
            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
            return Page();
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!,
            Password,
            RememberMe,
            false);

        if (result.Succeeded) {
            logger.LogInformation("Usuario {Email} inició sesión", Email);
            return RedirectToLocal(ReturnUrl);
        }

        ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
        return Page();
    }

    /// <summary>
    ///     Primer paso de entrar con passkey: devuelve las opciones WebAuthn en JSON.
    ///     Si el visitante ha escrito su correo, solo se piden las passkeys de esa
    ///     cuenta; sin correo, el navegador puede ofrecerlas todas (interfaz condicional).
    ///     Se llama desde passkeys.js con el parámetro de handler en la URL.
    /// </summary>
    /// <param name="email">Correo escrito en el formulario, si lo hay.</param>
    /// <returns>JSON con las opciones para navigator.credentials.get().</returns>
    public async Task<IActionResult> OnPostPasskeyRequestOptionsAsync(string? email) {
        User? user = null;

        if (!string.IsNullOrWhiteSpace(email)) {
            user = await userManager.FindByEmailAsync(email) ??
                   await userManager.FindByNameAsync(email);
        }

        var opciones = await signInManager.MakePasskeyRequestOptionsAsync(user);
        return Content(opciones, "application/json");
    }

    /// <summary>
    ///     Segundo paso de entrar con passkey: PasskeySignInAsync verifica la firma
    ///     contra la clave pública guardada y, si es válida, escribe la cookie de
    ///     identidad igual que PasswordSignInAsync.
    /// </summary>
    /// <param name="credential">Afirmación devuelta por el navegador, en JSON.</param>
    /// <param name="returnUrl">Página a la que volver tras entrar.</param>
    /// <returns>JSON con ok y el destino, o el motivo del rechazo.</returns>
    public async Task<IActionResult> OnPostPasskeyAssertionAsync(string credential, string? returnUrl = null) {
        var result = await signInManager.PasskeySignInAsync(credential);

        if (result.Succeeded) {
            logger.LogInformation("Acceso con passkey correcto");
            return new JsonResult(new { ok = true, destino = Destino(returnUrl) });
        }

        return StatusCode(StatusCodes.Status401Unauthorized, new {
            ok = false,
            mensaje = "La passkey no es válida o ha caducado el reto; vuelve a intentarlo"
        });
    }

    /// <summary>
    ///     Versión en cadena de <see cref="RedirectToLocal"/> para el destino que
    ///     devuelve el JSON de passkey: solo destinos locales.
    /// </summary>
    /// <param name="returnUrl">Destino pedido.</param>
    /// <returns>Ruta local, o la portada en caso contrario.</returns>
    private string Destino(string? returnUrl) {
        if (Url.IsLocalUrl(returnUrl))
            return returnUrl!;

        return Url.Content("~/")!;
    }

    private IActionResult RedirectToLocal(string? returnUrl) {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToPage("/Public/Index");
    }
}
