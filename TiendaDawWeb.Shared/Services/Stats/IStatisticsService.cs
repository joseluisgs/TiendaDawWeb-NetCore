using TiendaDawWeb.Shared.Models.Stats;

namespace TiendaDawWeb.Shared.Services.Stats;

public interface IStatisticsService
{
    Task<IEnumerable<CategorySalesDto>> GetSalesByCategoryAsync();
    Task<IEnumerable<MonthlySalesDto>> GetMonthlySalesAsync(int months = 12);
    Task<IEnumerable<TopBuyerDto>> GetTopBuyersAsync(int top = 10);
    Task<IEnumerable<TopSellerDto>> GetTopSellersAsync(int top = 10);
}
