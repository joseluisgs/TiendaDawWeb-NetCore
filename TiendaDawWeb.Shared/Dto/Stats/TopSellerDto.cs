namespace TiendaDawWeb.Shared.Dto.Stats;

/// <summary>
/// DTO para vendedores top.
/// </summary>
public class TopSellerDto
{
    /// <summary>ID del vendedor.</summary>
    public long PropietarioId { get; set; }
    /// <summary>Nombre del vendedor.</summary>
    public string Nombre { get; set; } = string.Empty;
    /// <summary>Número de productos vendidos.</summary>
    public int ProductosVendidos { get; set; }
}