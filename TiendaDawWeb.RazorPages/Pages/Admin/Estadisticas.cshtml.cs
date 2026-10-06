using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;

namespace TiendaDawWeb.RazorPages.Pages.Admin;

[Authorize(Roles = "ADMIN")]
public class EstadisticasModel(ApplicationDbContext context) : PageModel {
    public async Task OnGetAsync() {
        var productosMasVendidos = await context.Products
            .Where(p => p.CompraId != null)
            .GroupBy(p => p.Categoria)
            .Select(g => new {
                Categoria = g.Key,
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .ToListAsync();

        var compradoresActivos = await context.Purchases
            .GroupBy(p => new { p.CompradorId, p.Comprador.Nombre, p.Comprador.Apellidos })
            .Select(g => new {
                CompradorId = g.Key.CompradorId,
                Nombre = $"{g.Key.Nombre} {g.Key.Apellidos}".Trim(),
                TotalCompras = g.Count(),
                TotalGastado = g.Sum(p => p.Total)
            })
            .OrderByDescending(x => x.TotalCompras)
            .Take(10)
            .ToListAsync();

        var vendedoresActivos = await context.Products
            .Where(p => p.CompraId != null)
            .GroupBy(p => new { p.PropietarioId, p.Propietario.Nombre, p.Propietario.Apellidos })
            .Select(g => new {
                PropietarioId = g.Key.PropietarioId,
                Nombre = $"{g.Key.Nombre} {g.Key.Apellidos}".Trim(),
                ProductosVendidos = g.Count()
            })
            .OrderByDescending(x => x.ProductosVendidos)
            .Take(10)
            .ToListAsync();

        var hace12Meses = DateTime.UtcNow.AddMonths(-12);
        var ventasPorMes = await context.Purchases
            .Where(p => p.FechaCompra >= hace12Meses)
            .GroupBy(p => new { p.FechaCompra.Year, p.FechaCompra.Month })
            .Select(g => new {
                Año = g.Key.Year,
                Mes = g.Key.Month,
                TotalVentas = g.Sum(p => p.Total),
                NumeroCompras = g.Count()
            })
            .OrderBy(x => x.Año)
            .ThenBy(x => x.Mes)
            .ToListAsync();

        ViewData["ProductosMasVendidos"] = productosMasVendidos;
        ViewData["CompradoresActivos"] = compradoresActivos;
        ViewData["VendedoresActivos"] = vendedoresActivos;
        ViewData["VentasPorMes"] = ventasPorMes;
    }
}
