using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Product;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.RazorPages.Pages.Admin;

/// <summary>
///     Modelo de página para listar productos (admin)
/// </summary>
[Authorize(Roles = "ADMIN")]
public class ProductosModel(
    ApplicationDbContext context,
    IProductService productService,
    UserManager<User> userManager
) : PageModel {
    public List<ProductModel> Productos { get; set; } = new();

    /// <summary>
    ///     GET /Admin/Productos - Lista productos con paginación
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Elementos por página</param>
    /// <param name="categoria">Filtro por categoría</param>
    /// <returns>Vista con lista de productos</returns>
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

    /// <summary>
    ///     POST /Admin/Productos/Eliminar - Elimina un producto (soft delete)
    /// </summary>
    /// <param name="id">ID del producto</param>
    /// <returns>Redirect a la lista de productos</returns>
    public async Task<IActionResult> OnPostEliminarAsync(long id) {
        var adminUser = await userManager.GetUserAsync(User);
        if (adminUser == null) {
            TempData["Error"] = "Usuario no encontrado";
            return RedirectToPage("/Admin/Productos");
        }

        var result = await productService.DeleteAsync(id, adminUser.Id, true);

        if (result.IsFailure)
            TempData["Error"] = result.Error.Message;
        else
            TempData["Success"] = "Producto eliminado correctamente";

        return RedirectToPage("/Admin/Productos");
    }
}
