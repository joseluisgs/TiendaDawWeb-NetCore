using System.Globalization;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Dto.Stats;

namespace TiendaDawWeb.Shared.Services.Stats;

/// <summary>
/// Servicio de estadísticas para el panel de administración.
/// Calcula ventas por categoría, mensuales, y tops de compradores/vendedores.
/// </summary>
public class StatisticsService(ApplicationDbContext context) : IStatisticsService
{
    /// <summary>
    /// Obtiene las ventas agrupadas por categoría con porcentaje.
    /// </summary>
    public async Task<IEnumerable<CategorySalesDto>> GetSalesByCategoryAsync()
    {
        var data = await context.Products
            .Where(p => p.CompraId != null)
            .GroupBy(p => p.Categoria)
            .Select(g => new
            {
                Categoria = g.Key,
                Cantidad = g.Count()
            })
            .OrderByDescending(x => x.Cantidad)
            .ToListAsync();

        var total = data.Sum(x => x.Cantidad);

        return data.Select(x => new CategorySalesDto
        {
            Categoria = x.Categoria,
            Cantidad = x.Cantidad,
            Porcentaje = total > 0 ? (double)x.Cantidad * 100 / total : 0
        });
    }

    /// <summary>
    /// Obtiene las ventas mensuales de los últimos X meses.
    /// </summary>
    /// <param name="months">Número de meses hacia atrás (por defecto 12).</param>
    public async Task<IEnumerable<MonthlySalesDto>> GetMonthlySalesAsync(int months = 12)
    {
        var haceMeses = DateTime.UtcNow.AddMonths(-months);
        var culture = new CultureInfo("es-ES");

        var data = await context.Purchases
            .Where(p => p.FechaCompra >= haceMeses)
            .GroupBy(p => new { p.FechaCompra.Year, p.FechaCompra.Month })
            .Select(g => new
            {
                Año = g.Key.Year,
                Mes = g.Key.Month,
                TotalVentas = g.Sum(p => p.Total),
                NumeroCompras = g.Count()
            })
            .OrderBy(x => x.Año)
            .ThenBy(x => x.Mes)
            .ToListAsync();

        return data.Select(x => new MonthlySalesDto
        {
            Año = x.Año,
            Mes = x.Mes,
            NombreMes = culture.DateTimeFormat.GetMonthName(x.Mes),
            TotalVentas = x.TotalVentas,
            NumeroCompras = x.NumeroCompras
        });
    }

    /// <summary>
    /// Obtiene los mejores compradores orderedados por total de compras.
    /// </summary>
    /// <param name="top">Número de compradores a devolver (por defecto 10).</param>
    public async Task<IEnumerable<TopBuyerDto>> GetTopBuyersAsync(int top = 10)
    {
        return await context.Purchases
            .GroupBy(p => new { p.CompradorId, p.Comprador.UserName })
            .Select(g => new TopBuyerDto
            {
                CompradorId = g.Key.CompradorId,
                Nombre = g.Key.UserName ?? $"Usuario #{g.Key.CompradorId}",
                TotalCompras = g.Count(),
                TotalGastado = g.Sum(p => p.Total)
            })
            .OrderByDescending(x => x.TotalCompras)
            .Take(top)
            .ToListAsync();
    }

    /// <summary>
    /// Obtiene los mejores vendedores ordenados por productos vendidos.
    /// </summary>
    /// <param name="top">Número de vendedores a devolver (por defecto 10).</param>
    public async Task<IEnumerable<TopSellerDto>> GetTopSellersAsync(int top = 10)
    {
        return await context.Products
            .Where(p => p.CompraId != null)
            .GroupBy(p => new { p.PropietarioId, p.Propietario.UserName })
            .Select(g => new TopSellerDto
            {
                PropietarioId = g.Key.PropietarioId,
                Nombre = g.Key.UserName ?? $"Usuario #{g.Key.PropietarioId}",
                ProductosVendidos = g.Count()
            })
            .OrderByDescending(x => x.ProductosVendidos)
            .Take(top)
            .ToListAsync();
    }
}
