using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.ViewModels;

namespace TiendaDawWeb.RazorPages.Pages.Admin;

[Authorize(Roles = "ADMIN")]
public class IndexModel(
    ApplicationDbContext context
) : PageModel {
    public AdminDashboardViewModel ViewModel { get; set; } = new();

    public async Task OnGetAsync() {
        ViewModel.TotalUsuarios = await context.Users.CountAsync(u => !u.Deleted);
        ViewModel.TotalProductos = await context.Products.CountAsync(p => !p.Deleted);
        ViewModel.TotalCompras = await context.Purchases.CountAsync();
        ViewModel.TotalVentas = await context.Purchases.SumAsync(p => p.Total);

        ViewModel.UsuariosActivos = ViewModel.TotalUsuarios;
        ViewModel.ProductosDisponibles = await context.Products
            .CountAsync(p => !p.Deleted && p.CompraId == null);

        var now = DateTime.UtcNow;
        var hoy = now.Date;
        var inicioSemana = hoy.AddDays(-(int)hoy.DayOfWeek);
        var inicioMes = new DateTime(now.Year, now.Month, 1);

        ViewModel.ComprasHoy = await context.Purchases.CountAsync(p => p.FechaCompra >= hoy);
        ViewModel.ComprasSemana = await context.Purchases.CountAsync(p => p.FechaCompra >= inicioSemana);
        ViewModel.ComprasMes = await context.Purchases.CountAsync(p => p.FechaCompra >= inicioMes);

        ViewModel.VentasHoy = await context.Purchases
            .Where(p => p.FechaCompra >= hoy)
            .SumAsync(p => (decimal?)p.Total) ?? 0;
        ViewModel.VentasSemana = await context.Purchases
            .Where(p => p.FechaCompra >= inicioSemana)
            .SumAsync(p => (decimal?)p.Total) ?? 0;
        ViewModel.VentasMes = await context.Purchases
            .Where(p => p.FechaCompra >= inicioMes)
            .SumAsync(p => (decimal?)p.Total) ?? 0;
    }
}
