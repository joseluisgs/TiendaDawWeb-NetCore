using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.RazorPages.Pages.Admin;

/// <summary>
///     Modelo de página para ver los detalles de un usuario (admin)
/// </summary>
[Authorize(Roles = "ADMIN")]
public class UsuarioDetailsModel(
    ApplicationDbContext context,
    UserManager<User> userManager,
    RoleManager<IdentityRole<long>> roleManager,
    ILogger<UsuarioDetailsModel> logger
) : PageModel {
    public User Usuario { get; set; } = default!;

    /// <summary>
    ///     GET /Admin/UsuarioDetails/{id} - Muestra los detalles de un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <returns>Vista con los detalles del usuario</returns>
    public async Task<IActionResult> OnGetAsync(long id) {
        var usuario = await context.Users
            .Include(u => u.Products)
            .Include(u => u.Purchases)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (usuario == null) {
            TempData["Error"] = "Usuario no encontrado";
            return RedirectToPage("/Admin/Usuarios");
        }

        var roles = await userManager.GetRolesAsync(usuario);
        ViewData["Roles"] = roles.ToList();

        Usuario = usuario;
        return Page();
    }

    /// <summary>
    ///     POST /Admin/UsuarioDetails/CambiarRol - Cambia el rol de un usuario
    /// </summary>
    /// <param name="id">ID del usuario</param>
    /// <param name="nuevoRol">Nuevo rol a asignar</param>
    /// <returns>Redirect a los detalles del usuario</returns>
    public async Task<IActionResult> OnPostCambiarRolAsync(long id, string nuevoRol) {
        var usuario = await userManager.FindByIdAsync(id.ToString());
        if (usuario == null) {
            TempData["Error"] = "Usuario no encontrado";
            return RedirectToPage("/Admin/Usuarios");
        }

        if (!await roleManager.RoleExistsAsync(nuevoRol)) {
            TempData["Error"] = "Rol no válido";
            return RedirectToPage("/Admin/UsuarioDetails", new { id });
        }

        var rolesActuales = await userManager.GetRolesAsync(usuario);
        await userManager.RemoveFromRolesAsync(usuario, rolesActuales);

        var result = await userManager.AddToRoleAsync(usuario, nuevoRol);

        if (result.Succeeded) {
            logger.LogInformation("Rol de usuario {UserId} cambiado a {Role}", id, nuevoRol);
            TempData["Success"] = $"Rol cambiado a {nuevoRol}";
        }
        else {
            TempData["Error"] = "Error al cambiar el rol";
        }

        return RedirectToPage("/Admin/UsuarioDetails", new { id });
    }

    /// <summary>
    ///     POST /Admin/UsuarioDetails/Eliminar - Elimina un usuario (soft delete)
    /// </summary>
    /// <param name="id">ID del usuario a eliminar</param>
    /// <returns>Redirect a la lista de usuarios</returns>
    public async Task<IActionResult> OnPostEliminarAsync(long id) {
        var usuario = await context.Users.FindAsync(id);
        if (usuario == null) {
            TempData["Error"] = "Usuario no encontrado";
            return RedirectToPage("/Admin/Usuarios");
        }

        var hasProductosActivos = await context.Products
            .IgnoreQueryFilters()
            .Where(p => p.PropietarioId == id && !p.Deleted && p.CompraId == null)
            .AnyAsync();

        if (hasProductosActivos) {
            logger.LogWarning("Intento de eliminar usuario {UserId} con productos activos a la venta", id);
            TempData["Error"] = "No se puede eliminar un usuario con productos a la venta";
            return RedirectToPage("/Admin/Usuarios");
        }

        var hasProductosVendidos = await context.Products
            .IgnoreQueryFilters()
            .Where(p => p.PropietarioId == id && p.CompraId != null)
            .AnyAsync();

        if (hasProductosVendidos) {
            logger.LogWarning("Intento de eliminar usuario {UserId} con productos vendidos", id);
            TempData["Error"] = "No se puede eliminar un usuario que ha vendido productos";
            return RedirectToPage("/Admin/Usuarios");
        }

        var hasCompras = await context.Purchases
            .AnyAsync(p => p.CompradorId == id);

        if (hasCompras) {
            logger.LogWarning("Intento de eliminar usuario {UserId} con compras realizadas", id);
            TempData["Error"] = "No se puede eliminar un usuario que ha realizado compras";
            return RedirectToPage("/Admin/Usuarios");
        }

        usuario.Deleted = true;
        usuario.DeletedAt = DateTime.UtcNow;

        var adminUser = await userManager.GetUserAsync(User);
        usuario.DeletedBy = adminUser?.Id.ToString();

        await context.SaveChangesAsync();

        logger.LogInformation("Usuario {UserId} eliminado (soft delete) por admin {AdminId}",
            id, adminUser?.Id);

        TempData["Success"] = "Usuario eliminado correctamente";
        return RedirectToPage("/Admin/Usuarios");
    }
}
