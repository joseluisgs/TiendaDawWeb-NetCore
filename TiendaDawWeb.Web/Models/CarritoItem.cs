using System.ComponentModel.DataAnnotations;

namespace TiendaDawWeb.Models;

/// <summary>
///     Item del carrito de compras - Sin cantidad, cada producto solo puede añadirse una vez
///     Coincide con implementación original de Spring Boot
/// </summary>
public class CarritoItem : AuditableEntity {
    public long Id { get; set; }

    /// <summary>
    ///     Token de concurrencia para evitar condiciones de carrera
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }

    // Relaciones
    public long UsuarioId { get; set; }
    public virtual User Usuario { get; set; } = null!;

    public long ProductoId { get; set; }
    public virtual Product Producto { get; set; } = null!;

    /// <summary>
    ///     Precio calculado del producto al momento de agregarlo al carrito
    /// </summary>
    public decimal Precio { get; set; }
}