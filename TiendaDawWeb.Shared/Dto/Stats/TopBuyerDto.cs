namespace TiendaDawWeb.Shared.Dto.Stats;

/// <summary>
/// DTO para compradores top.
/// </summary>
public class TopBuyerDto
{
    /// <summary>ID del comprador.</summary>
    public long CompradorId { get; set; }
    /// <summary>Nombre del comprador.</summary>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Total de compras realizadas.</summary>
    public int TotalCompras { get; set; }
    /// <summary>Total gastado en euros.</summary>
    public decimal TotalGastado { get; set; }
}