using TiendaDawWeb.Shared.Models.Enums;

namespace TiendaDawWeb.Shared.Models.Stats;

public class CategorySalesDto
{
    public ProductCategory Categoria { get; set; }
    public int Cantidad { get; set; }
    public double Porcentaje { get; set; }
}

public class MonthlySalesDto
{
    public int Año { get; set; }
    public int Mes { get; set; }
    public string NombreMes { get; set; } = string.Empty;
    public decimal TotalVentas { get; set; }
    public int NumeroCompras { get; set; }
    public decimal TicketMedio => NumeroCompras > 0 ? TotalVentas / NumeroCompras : 0;
}

public class TopBuyerDto
{
    public long CompradorId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int TotalCompras { get; set; }
    public decimal TotalGastado { get; set; }
}

public class TopSellerDto
{
    public long PropietarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int ProductosVendidos { get; set; }
}
