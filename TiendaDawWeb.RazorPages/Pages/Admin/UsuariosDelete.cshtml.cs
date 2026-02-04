using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.RazorPages.Pages.Admin;

[Authorize(Roles = "ADMIN")]
public class UsuariosDeleteModel(
    ApplicationDbContext context,
    UserManager<User> userManager,
    ILogger<UsuariosDeleteModel> logger
) : PageModel {
    public async Task<IActionResult> OnPostAsync(long id) {
        var adminUser = await userManager.GetUserAsync(User);
        if (adminUser == null) {
            TempData["Error"] = "Usuario no autenticado";
            return RedirectToPage("/Admin/Usuarios");
        }

        if (id == adminUser.Id) {
            logger.LogWarning("Admin {AdminId} intentó eliminarse a sí mismo", adminUser.Id);
            TempData["Error"] = "No puedes eliminarte a ti mismo";
            return RedirectToPage("/Admin/Usuarios");
        }

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

        usuario.DeletedBy = adminUser?.Id.ToString();

        await context.SaveChangesAsync();

        logger.LogInformation("Usuario {UserId} eliminado (soft delete) por admin {AdminId}",
            id, adminUser?.Id);

        TempData["Success"] = "Usuario eliminado correctamente";
        return RedirectToPage("/Admin/Usuarios");
    }
}
