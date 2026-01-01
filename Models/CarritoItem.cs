using System.ComponentModel.DataAnnotations;

namespace TiendaDawWeb.Models;

/// <summary>
/// Item del carrito de compras con control de concurrencia
/// </summary>
public class CarritoItem
{
    public long Id { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1")]
    public int Cantidad { get; set; } = 1;

    public decimal Subtotal { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Token de concurrencia para evitar condiciones de carrera
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Relaciones
    public long UsuarioId { get; set; }
    public virtual User Usuario { get; set; } = null!;

    public long ProductoId { get; set; }
    public virtual Product Producto { get; set; } = null!;
}
