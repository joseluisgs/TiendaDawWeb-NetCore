using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Storage;

namespace TiendaDawWeb.Controllers;

/// <summary>
///     Controlador para la gestión del perfil de usuario
/// </summary>
[Authorize]
[Route("app/perfil")]
public class ProfileController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    ApplicationDbContext context,
    IStorageService storageService,
    ILogger<ProfileController> logger
) : Controller {
    /// <summary>
    ///     GET /app/perfil - Vista del perfil del usuario
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        // Cargar las colecciones necesarias para la vista
        await context.Entry(user)
            .Collection(u => u.Products)
            .LoadAsync();
        await context.Entry(user)
            .Collection(u => u.Purchases)
            .LoadAsync();
        await context.Entry(user)
            .Collection(u => u.Favorites)
            .LoadAsync();

        return View(user);
    }

    /// <summary>
    ///     GET /app/perfil/editar - Formulario de edición del perfil
    /// </summary>
    [HttpGet("editar")]
    public async Task<IActionResult> Edit() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        return View(user);
    }

    /// <summary>
    ///     POST /app/perfil/editar - Actualizar datos del perfil
    /// </summary>
    [HttpPost("editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string nombre, string apellidos, IFormFile? avatar) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellidos)) {
            TempData["Error"] = "El nombre y apellidos son obligatorios";
            return View(user);
        }

        // Actualizar datos básicos
        user.Nombre = nombre.Trim();
        user.Apellidos = apellidos.Trim();

        // Procesar avatar si se proporciona
        if (avatar != null && avatar.Length > 0) {
            // Validar tamaño (máximo 5MB)
            if (avatar.Length > 5 * 1024 * 1024) {
                TempData["Error"] = "El archivo es demasiado grande. Máximo 5MB";
                return View(user);
            }

            // Validar tipo de archivo
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(avatar.ContentType.ToLower())) {
                TempData["Error"] = "Solo se permiten imágenes (JPG, PNG, GIF)";
                return View(user);
            }

            // Guardar la imagen usando el servicio de storage (Result pattern)
            var result = await storageService.SaveFileAsync(avatar, "avatars");
            if (result.IsSuccess) {
                // Eliminar avatar anterior si existe
                if (!string.IsNullOrEmpty(user.Avatar)) await storageService.DeleteFileAsync(user.Avatar);
                user.Avatar = result.Value;
            }
            else {
                TempData["Error"] = "Error al guardar el avatar: " + result.Error.Message;
                return View(user);
            }
        }

        // Actualizar usuario
        var updateResult = await userManager.UpdateAsync(user);
        if (updateResult.Succeeded) {
            logger.LogInformation("Perfil actualizado para usuario {UserId}", user.Id);
            TempData["Success"] = "Perfil actualizado correctamente";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = "Error al actualizar el perfil: " +
                            string.Join(", ", updateResult.Errors.Select(e => e.Description));
        return View(user);
    }

    /// <summary>
    ///     POST /app/perfil/eliminar-avatar - Eliminar avatar del usuario
    /// </summary>
    [HttpPost("eliminar-avatar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAvatar() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        if (!string.IsNullOrEmpty(user.Avatar)) {
            await storageService.DeleteFileAsync(user.Avatar);
            user.Avatar = null;
            await userManager.UpdateAsync(user);

            logger.LogInformation("Avatar eliminado para usuario {UserId}", user.Id);
            TempData["Success"] = "Avatar eliminado correctamente";
        }

        return RedirectToAction(nameof(Edit));
    }

    /// <summary>
    ///     GET /app/perfil/cambiar-password - Formulario para cambiar contraseña
    /// </summary>
    [HttpGet("cambiar-password")]
    public IActionResult ChangePassword() {
        return View();
    }

    /// <summary>
    ///     POST /app/perfil/cambiar-password - Cambiar contraseña
    /// </summary>
    [HttpPost("cambiar-password")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult>
        ChangePassword(string currentPassword, string newPassword, string confirmPassword) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword)) {
            TempData["Error"] = "Todos los campos son obligatorios";
            return View();
        }

        if (newPassword != confirmPassword) {
            TempData["Error"] = "Las contraseñas no coinciden";
            return View();
        }

        if (newPassword.Length < 4) {
            TempData["Error"] = "La contraseña debe tener al menos 4 caracteres";
            return View();
        }

        var changeResult = await userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (changeResult.Succeeded) {
            logger.LogInformation("Contraseña cambiada para usuario {UserId}", user.Id);
            TempData["Success"] = "Contraseña cambiada correctamente";
            return RedirectToAction(nameof(Index));
        }

        TempData["Error"] = "Error al cambiar la contraseña: " +
                            string.Join(", ", changeResult.Errors.Select(e => e.Description));
        return View();
    }

    /// <summary>
    ///     GET /app/perfil/passkeys - Zona de gestión de passkeys de la cuenta.
    ///     Una passkey es un par de claves que vive en el dispositivo: aquí solo se
    ///     guarda la pública, y entrar con ella es un gesto (huella, cara o PIN).
    /// </summary>
    /// <returns>Vista con las passkeys guardadas en la cuenta.</returns>
    [HttpGet("passkeys")]
    public async Task<IActionResult> Passkeys() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        var passkeys = await userManager.GetPasskeysAsync(user);
        return View(passkeys.ToList());
    }

    /// <summary>
    ///     POST /app/perfil/passkeys/crear-opciones - Primer paso de crear una passkey.
    ///     Devuelve las opciones WebAuthn en JSON y Identity guarda el estado del
    ///     reto en una cookie que solo puede leer el servidor.
    /// </summary>
    /// <returns>JSON con las opciones para navigator.credentials.create().</returns>
    [HttpPost("passkeys/crear-opciones")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasskeyCreationOptions() {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var nombre = user.UserName ?? user.Email ?? "usuario";

        var opciones = await signInManager.MakePasskeyCreationOptionsAsync(new PasskeyUserEntity {
            Id = await userManager.GetUserIdAsync(user),
            Name = nombre,
            DisplayName = nombre
        });

        return Content(opciones, "application/json");
    }

    /// <summary>
    ///     POST /app/perfil/passkeys/atestacion - Segundo paso: verifica la atestación
    ///     de la credencial creada y la guarda en AspNetUserPasskeys con
    ///     AddOrUpdatePasskeyAsync. El estado del reto nombra a una cuenta y se
    ///     comprueba que es la conectada antes de escribir nada.
    /// </summary>
    /// <param name="credential">Credencial devuelta por el navegador, en JSON.</param>
    /// <returns>JSON con ok y, si falla, el motivo.</returns>
    [HttpPost("passkeys/atestacion")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasskeyAttestation(string credential) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var result = await signInManager.PerformPasskeyAttestationAsync(credential);
        if (!result.Succeeded) {
            return StatusCode(StatusCodes.Status400BadRequest, new {
                ok = false,
                mensaje = result.Failure?.Message ?? "No se pudo verificar la passkey"
            });
        }

        var userId = await userManager.GetUserIdAsync(user);
        if (!string.Equals(result.UserEntity?.Id, userId, StringComparison.Ordinal)) {
            return StatusCode(StatusCodes.Status400BadRequest, new {
                ok = false,
                mensaje = "La passkey no pertenece a la cuenta que está conectada"
            });
        }

        var guardado = await userManager.AddOrUpdatePasskeyAsync(user, result.Passkey);
        if (!guardado.Succeeded) {
            return StatusCode(StatusCodes.Status400BadRequest, new {
                ok = false,
                mensaje = "No se pudo guardar la passkey"
            });
        }

        logger.LogInformation("Passkey creada para usuario {UserId}", user.Id);
        return Json(new { ok = true });
    }

    /// <summary>
    ///     POST /app/perfil/passkeys/eliminar - Elimina una passkey de la cuenta.
    ///     El id llega en base64url, la forma en que WebAuthn lo maneja en el
    ///     navegador; la base de datos lo guarda como bytes.
    /// </summary>
    /// <param name="id">Identificador de la credencial en base64url.</param>
    /// <returns>302 a la zona de passkeys con el aviso.</returns>
    [HttpPost("passkeys/eliminar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PasskeyEliminar(string id) {
        var user = await userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Auth");

        try {
            var credencial = DeBase64Url(id);
            var result = await userManager.RemovePasskeyAsync(user, credencial);
            TempData[result.Succeeded ? "Success" : "Error"] = result.Succeeded
                ? "Passkey eliminada"
                : "No se pudo eliminar la passkey";

            if (result.Succeeded)
                logger.LogInformation("Passkey eliminada para usuario {UserId}", user.Id);
        }
        catch (FormatException) {
            TempData["Error"] = "El identificador de la passkey no es válido";
        }

        return RedirectToAction(nameof(Passkeys));
    }

    /// <summary>De base64url (WebAuthn) a los bytes que guarda Identity.</summary>
    private static byte[] DeBase64Url(string id) {
        var base64 = id.Replace('-', '+').Replace('_', '/');
        base64 = base64.PadRight(base64.Length + ((4 - (base64.Length % 4)) % 4), '=');
        return Convert.FromBase64String(base64);
    }
}