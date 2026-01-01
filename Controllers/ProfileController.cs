using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Controllers;

/// <summary>
/// Controlador para la gestión del perfil de usuario
/// </summary>
[Authorize]
[Route("app/perfil")]
public class ProfileController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IStorageService _storageService;
    private readonly ILogger<ProfileController> _logger;

    public ProfileController(
        UserManager<User> userManager,
        IStorageService storageService,
        ILogger<ProfileController> logger)
    {
        _userManager = userManager;
        _storageService = storageService;
        _logger = logger;
    }

    /// <summary>
    /// GET /app/perfil - Vista del perfil del usuario
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        return View(user);
    }

    /// <summary>
    /// GET /app/perfil/editar - Formulario de edición del perfil
    /// </summary>
    [HttpGet("editar")]
    public async Task<IActionResult> Edit()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        return View(user);
    }

    /// <summary>
    /// POST /app/perfil/editar - Actualizar datos del perfil
    /// </summary>
    [HttpPost("editar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string nombre, string apellidos, IFormFile? avatar)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellidos))
        {
            TempData["Error"] = "El nombre y apellidos son obligatorios";
            return View(user);
        }

        // Actualizar datos básicos
        user.Nombre = nombre.Trim();
        user.Apellidos = apellidos.Trim();

        // Procesar avatar si se proporciona
        if (avatar != null && avatar.Length > 0)
        {
            // Validar tamaño (máximo 5MB)
            if (avatar.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "El archivo es demasiado grande. Máximo 5MB";
                return View(user);
            }

            // Validar tipo de archivo
            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(avatar.ContentType.ToLower()))
            {
                TempData["Error"] = "Solo se permiten imágenes (JPG, PNG, GIF)";
                return View(user);
            }

            try
            {
                // Guardar la imagen usando el servicio de storage
                var result = await _storageService.SaveFileAsync(avatar, "avatars");
                if (result.IsSuccess)
                {
                    // Eliminar avatar anterior si existe
                    if (!string.IsNullOrEmpty(user.Avatar))
                    {
                        await _storageService.DeleteFileAsync(user.Avatar);
                    }

                    user.Avatar = result.Value;
                }
                else
                {
                    TempData["Error"] = "Error al guardar el avatar: " + result.Error.Message;
                    return View(user);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al procesar avatar para usuario {UserId}", user.Id);
                TempData["Error"] = "Error al procesar la imagen";
                return View(user);
            }
        }

        // Actualizar usuario
        var updateResult = await _userManager.UpdateAsync(user);
        if (updateResult.Succeeded)
        {
            _logger.LogInformation("Perfil actualizado para usuario {UserId}", user.Id);
            TempData["Success"] = "Perfil actualizado correctamente";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            TempData["Error"] = "Error al actualizar el perfil: " + string.Join(", ", updateResult.Errors.Select(e => e.Description));
            return View(user);
        }
    }

    /// <summary>
    /// POST /app/perfil/eliminar-avatar - Eliminar avatar del usuario
    /// </summary>
    [HttpPost("eliminar-avatar")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAvatar()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!string.IsNullOrEmpty(user.Avatar))
        {
            await _storageService.DeleteFileAsync(user.Avatar);
            user.Avatar = null;
            await _userManager.UpdateAsync(user);
            
            _logger.LogInformation("Avatar eliminado para usuario {UserId}", user.Id);
            TempData["Success"] = "Avatar eliminado correctamente";
        }

        return RedirectToAction(nameof(Edit));
    }

    /// <summary>
    /// GET /app/perfil/cambiar-contraseña - Formulario para cambiar contraseña
    /// </summary>
    [HttpGet("cambiar-contraseña")]
    public IActionResult ChangePassword()
    {
        return View();
    }

    /// <summary>
    /// POST /app/perfil/cambiar-contraseña - Cambiar contraseña
    /// </summary>
    [HttpPost("cambiar-contraseña")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
        {
            TempData["Error"] = "Todos los campos son obligatorios";
            return View();
        }

        if (newPassword != confirmPassword)
        {
            TempData["Error"] = "Las contraseñas no coinciden";
            return View();
        }

        if (newPassword.Length < 4)
        {
            TempData["Error"] = "La contraseña debe tener al menos 4 caracteres";
            return View();
        }

        var changeResult = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
        if (changeResult.Succeeded)
        {
            _logger.LogInformation("Contraseña cambiada para usuario {UserId}", user.Id);
            TempData["Success"] = "Contraseña cambiada correctamente";
            return RedirectToAction(nameof(Index));
        }
        else
        {
            TempData["Error"] = "Error al cambiar la contraseña: " + string.Join(", ", changeResult.Errors.Select(e => e.Description));
            return View();
        }
    }
}
