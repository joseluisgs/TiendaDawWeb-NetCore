using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.ViewModels;

namespace TiendaDawWeb.Controllers;

/// <summary>
///     Controlador de autenticación (login, registro, logout)
/// </summary>
public class AuthController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
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

        var user = new User {
            UserName = model.Email,
            Email = model.Email,
            Nombre = model.Nombre,
            Apellidos = model.Apellidos,
            Avatar = string.IsNullOrWhiteSpace(model.Avatar)
                ? $"https://robohash.org/{model.Email}?size=200x200&bgset=bg2"
                : model.Avatar,
            Rol = "USER",
            EmailConfirmed = true
        };

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

    private IActionResult RedirectToLocal(string? returnUrl) {
        if (Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction(nameof(PublicController.Index), "Public");
    }
}