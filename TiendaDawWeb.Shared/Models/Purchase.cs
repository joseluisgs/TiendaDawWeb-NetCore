namespace TiendaDawWeb.Shared.Models;

/// <summary>
/// Entidad de compra realizada por un usuario.
/// Representa una transacción con fecha, total y lista de productos comprados.
/// </summary>
public class Purchase : AuditableEntity
{
    /// <summary>ID único de la compra (PK en SQLite).</summary>
    public long Id { get; set; }

    /// <summary>Fecha y hora de la compra en UTC.</summary>
    public DateTime FechaCompra { get; set; } = DateTime.UtcNow;

    /// <summary>Total de la compra en EUR.</summary>
    public decimal Total { get; set; }

    /// <summary>ID del usuario comprador (FK).</summary>
    public long CompradorId { get; set; }

    /// <summary>Usuario que realizó la compra.</summary>
    public virtual User Comprador { get; set; } = null!;

    /// <summary>Lista de productos incluidos en la compra.</summary>
    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
