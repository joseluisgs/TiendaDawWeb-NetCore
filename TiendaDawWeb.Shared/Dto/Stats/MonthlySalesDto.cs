namespace TiendaDawWeb.Shared.Dto.Stats;

/// <summary>
/// DTO para ventas mensuales.
/// </summary>
public class MonthlySalesDto
{
    /// <summary>Año de la venta.</summary>
    public int Año { get; set; }
    /// <summary>Mes de la venta (1-12).</summary>
    public int Mes { get; set; }
    /// <summary>Nombre del mes en español.</summary>
    public string NombreMes { get; set; } = string.Empty;
    /// <summary>Total de ventas en euros.</summary>
    public decimal TotalVentas { get; set; }
    /// <summary>Número de compras en el mes.</summary>
    public int NumeroCompras { get; set; }
    /// <summary>Ticket medio (TotalVentas / NumeroCompras).</summary>
    public decimal TicketMedio => NumeroCompras > 0 ? TotalVentas / NumeroCompras : 0;
}