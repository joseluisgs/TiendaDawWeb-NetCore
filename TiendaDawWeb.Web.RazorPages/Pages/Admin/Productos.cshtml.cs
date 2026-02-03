using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.Web.RazorPages.Pages.Admin;

[Authorize(Roles = "ADMIN")]
public class ProductosModel(ApplicationDbContext context) : PageModel {
    public List<ProductModel> Productos { get; set; } = new();

    public async Task OnGetAsync(int page = 1, int pageSize = 20, string? categoria = null) {
        var skip = (page - 1) * pageSize;

        var query = context.Products
            .Include(p => p.Propietario)
            .Where(p => !p.Deleted);

        if (!string.IsNullOrEmpty(categoria))
            if (Enum.TryParse<ProductCategory>(categoria, out var cat))
                query = query.Where(p => p.Categoria == cat);

        Productos = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalProductos"] = await query.CountAsync();
        ViewData["CategoriaSeleccionada"] = categoria;
    }
}
