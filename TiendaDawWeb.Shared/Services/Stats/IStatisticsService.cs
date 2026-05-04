using TiendaDawWeb.Shared.Dto.Stats;

namespace TiendaDawWeb.Shared.Services.Stats;

/// <summary>
/// Interfaz del servicio de estadísticas.
/// </summary>
public interface IStatisticsService
{
    /// <summary>Obtiene las ventas agrupadas por categoría.</summary>
    Task<IEnumerable<CategorySalesDto>> GetSalesByCategoryAsync();

    /// <summary>Obtiene las ventas mensuales de los últimos X meses.</summary>
    /// <param name="months">Número de meses hacia atrás (por defecto 12).</param>
    Task<IEnumerable<MonthlySalesDto>> GetMonthlySalesAsync(int months = 12);

    /// <summary>Obtiene los mejores compradores.</summary>
    /// <param name="top">Número de compradores a devolver (por defecto 10).</param>
    Task<IEnumerable<TopBuyerDto>> GetTopBuyersAsync(int top = 10);

    /// <summary>Obtiene los mejores vendedores.</summary>
    /// <param name="top">Número de vendedores a devolver (por defecto 10).</param>
    Task<IEnumerable<TopSellerDto>> GetTopSellersAsync(int top = 10);
}
