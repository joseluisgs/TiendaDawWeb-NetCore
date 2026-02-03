using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Web.RazorPages.Pages.Admin;

[Authorize(Roles = "ADMIN")]
public class UsuariosModel(ApplicationDbContext context) : PageModel {
    public List<User> Usuarios { get; set; } = new();

    public async Task OnGetAsync(int page = 1, int pageSize = 20) {
        var skip = (page - 1) * pageSize;

        Usuarios = await context.Users
            .Include(u => u.Products.Where(p => !p.Deleted))
            .Include(u => u.Purchases)
            .Where(u => !u.Deleted)
            .OrderByDescending(u => u.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalUsuarios"] = await context.Users.CountAsync(u => !u.Deleted);
    }
}
