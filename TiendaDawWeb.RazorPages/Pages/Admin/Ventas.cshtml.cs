using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.RazorPages.Pages.Admin;

[Authorize(Roles = "ADMIN")]
public class VentasModel(ApplicationDbContext context) : PageModel {
    public List<PurchaseModel> Ventas { get; set; } = new();

    public async Task OnGetAsync(int page = 1, int pageSize = 20, DateTime? desde = null, DateTime? hasta = null) {
        var skip = (page - 1) * pageSize;

        var query = context.Purchases
            .Include(p => p.Comprador)
            .Include(p => p.Products)
            .AsQueryable();

        if (desde.HasValue) query = query.Where(p => p.FechaCompra >= desde.Value);
        if (hasta.HasValue) {
            var hastaFinal = hasta.Value.AddDays(1).AddSeconds(-1);
            query = query.Where(p => p.FechaCompra <= hastaFinal);
        }

        Ventas = await query
            .OrderByDescending(p => p.FechaCompra)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();

        ViewData["CurrentPage"] = page;
        ViewData["PageSize"] = pageSize;
        ViewData["TotalCompras"] = await query.CountAsync();
        ViewData["Desde"] = desde;
        ViewData["Hasta"] = hasta;
    }
}
