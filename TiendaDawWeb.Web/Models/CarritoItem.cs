using System.ComponentModel.DataAnnotations;

namespace TiendaDawWeb.Models;

/// <summary>
/// Entidad de item del carrito de compras.
/// Cada producto solo puede añadirse una vez al carrito (sin cantidad).
/// </summary>
public class CarritoItem : AuditableEntity
{
    /// <summary>ID único del item (PK en SQLite).</summary>
    public long Id { get; set; }

    /// <summary>Token de concurrencia para evitar condiciones de carrera (byte array).</summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    /// <summary>ID del usuario propietario del carrito (FK).</summary>
    public long UsuarioId { get; set; }

    /// <summary>Usuario propietario del carrito.</summary>
    public virtual User Usuario { get; set; } = null!;

    /// <summary>ID del producto en el carrito (FK).</summary>
    public long ProductoId { get; set; }

    /// <summary>Producto en el carrito.</summary>
    public virtual Product Producto { get; set; } = null!;

    /// <summary>Precio del producto al momento de agregarlo al carrito (valor fijo).</summary>
    public decimal Precio { get; set; }
}
