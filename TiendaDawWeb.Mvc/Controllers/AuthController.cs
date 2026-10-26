using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.ViewModels;
using TiendaDawWeb.Shared.Mappers;

namespace TiendaDawWeb.Controllers;

/// <summary>
///     Controlador de autenticación (login, registro, logout, OAuth externo)
/// </summary>
public class AuthController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    IAuthenticationSchemeProvider schemeProvider,
    ILogger<AuthController> logger
) : Controller {
    /// <summary>
    ///     Mostrar formulario de login
    /// </summary>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null) {
        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    ///     Procesar login
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model) {
        if (!ModelState.IsValid)
            return View(model);

        var user = await userManager.FindByEmailAsync(model.Email);
        if (user == null) {
            ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
            return View(model);
        }

        var result = await signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            false);

        if (result.Succeeded) {
            logger.LogInformation("Usuario {Email} inició sesión", model.Email);
            return RedirectToLocal(model.ReturnUrl);
        }

        ModelState.AddModelError(string.Empty, "Email o contraseña incorrectos");
        return View(model);
    }

    /// <summary>
    ///     Mostrar formulario de registro
    /// </summary>
    [HttpGet]
    public IActionResult Register() {
        return View();
    }

    /// <summary>
    ///     Procesar registro
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model) {
        if (!ModelState.IsValid)
            return View(model);

        var existingUser = await userManager.FindByEmailAsync(model.Email);
        if (existingUser != null) {
            ModelState.AddModelError(string.Empty, "Ya existe un usuario con este email");
            return View(model);
        }

        // 🧹 REFACTOR: Usamos el Mapper
        var user = model.ToEntity();

        var result = await userManager.CreateAsync(user, model.Password);

        if (result.Succeeded) {
            logger.LogInformation("Nuevo usuario registrado: {Email}", model.Email);
            await signInManager.SignInAsync(user, false);
            return RedirectToAction(nameof(PublicController.Index), "Public");
        }

        foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

        return View(model);
    }

    /// <summary>
    ///     Cerrar sesión
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout() {
        await signInManager.SignOutAsync();
        logger.LogInformation("Usuario cerró sesión");
        return RedirectToAction(nameof(PublicController.Index), "Public");
    }

    /// <summary>
    ///     Página de acceso denegado
    /// </summary>
    public IActionResult AccessDenied() {
        return View();
    }

    /// <summary>
    ///     Inicia login externo con un proveedor OAuth 2.0 (Google, GitHub, Microsoft).
    ///     Redirige al usuario al proveedor para que autentique.
    /// </summary>
    /// <param name="provider">Nombre del proveedor: "Google", "GitHub" o "Microsoft".</param>
    /// <returns>302 Redirect al proveedor OAuth, o 404 si no está configurado.</returns>
    [HttpGet("/Auth/External/{provider}")]
    public async Task<IActionResult> ExternalLogin(string provider) {
        var scheme = await schemeProvider.GetSchemeAsync(provider);
        if (scheme is null) {
            logger.LogWarning("Proveedor OAuth no configurado: {Provider}", provider);
            return NotFound($"Proveedor '{provider}' no está configurado");
        }

        var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Auth");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };

        logger.LogInformation("Iniciando login externo con {Provider}", provider);
        return Challenge(properties, provider);
    }

    /// <summary>
    ///     Callback del proveedor OAuth 2.0. Identity procesa la respuesta,
    ///     vincula/autentica el usuario y crea la sesión con cookie.
    /// </summary>
    /// <returns>Redirect al home o a returnUrl, o error.</returns>
    [HttpGet("/Auth/ExternalCallback")]
    public async Task<IActionResult> ExternalLoginCallback(string? returnUrl = null) {
        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null) {
            logger.LogWarning("Login externo falló: sin información del proveedor");
            TempData["Error"] = "No se pudo autenticar con el proveedor externo";
            return RedirectToAction(nameof(Login));
        }

        var result = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider, info.ProviderKey, isPersistent: false);

        if (result.Succeeded) {
            logger.LogInformation("Login externo exitoso con {Provider}", info.LoginProvider);
            return RedirectToLocal(returnUrl);
        }

        // Primera vez: crear cuenta local vinculada
        var email = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
        if (string.IsNullOrWhiteSpace(email)) {
            TempData["Error"] = "El proveedor no devolvió email";
            return RedirectToAction(nameof(Login));
        }

        var user = await userManager.FindByEmailAsync(email);
        if (user is null) {
            var fullName = info.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
            var nameParts = fullName.Split(' ', 2);
            user = new User {
                UserName = email,
                Email = email,
                Nombre = nameParts[0],
                Apellidos = nameParts.Length > 1 ? nameParts[1] : "",
                Rol = "USER",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            var createResult = await userManager.CreateAsync(user);
            if (!createResult.Succeeded) {
                TempData["Error"] = "Error creando usuario: " +
                    string.Join(", ", createResult.Errors.Select(e => e.Description));
                return RedirectToAction(nameof(Login));
            }
            logger.LogInformation("Usuario OAuth creado: {Email} ({Provider})", email, info.LoginProvider);
        }

        var addLoginResult = await userManager.AddLoginAsync(user, info);
        if (addLoginResult.Succeeded) {
            await signInManager.SignInAsync(user, isPersistent: false);
            logger.LogInformation("Login externo completado: {Email}", email);
            return RedirectToLocal(returnUrl);
        }

        TempData["Error"] = "Error vinculando cuenta externa";
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToLocal(string? returnUrl) {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(PublicController.Index), "Public");
    }
}