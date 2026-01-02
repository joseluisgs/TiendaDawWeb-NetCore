namespace TiendaDawWeb.Models;

/// <summary>
/// Entidad de compra realizada por un usuario
/// </summary>
public class Purchase
{
    public long Id { get; set; }
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;
    public decimal Total { get; set; }

    public long CompradorId { get; set; }
    public virtual User Comprador { get; set; } = null!;

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
