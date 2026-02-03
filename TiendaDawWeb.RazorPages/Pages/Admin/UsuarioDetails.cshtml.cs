using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.RazorPages.Pages.Admin;

[Authorize(Roles = "ADMIN")]
public class UsuarioDetailsModel(
    ApplicationDbContext context,
    UserManager<User> userManager
) : PageModel {
    public User Usuario { get; set; } = default!;

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
}
