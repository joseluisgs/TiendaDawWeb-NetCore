using System.ComponentModel.DataAnnotations;
using TiendaDawWeb.Shared.Models.Enums;

namespace TiendaDawWeb.Shared.Models;

/// <summary>
/// Entidad de producto del marketplace.
/// Representa un artículo en venta con información de precio, categoría y estado.
/// </summary>
public class Product : AuditableEntity
{
    /// <summary>ID único del producto (PK en SQLite).</summary>
    public long Id { get; set; }

    /// <summary>Nombre del producto (3-200 caracteres, obligatorio).</summary>
    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Descripción detallada del producto (1-1000 caracteres, obligatoria).</summary>
    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>Precio unitario en EUR (mayor que 0, obligatorio).</summary>
    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero")]
    public decimal Precio { get; set; }

    /// <summary>URL o ruta de la imagen del producto (null = imagen por defecto).</summary>
    public string? Imagen { get; set; }

    /// <summary>Categoría del producto (obligatoria).</summary>
    [Required]
    public ProductCategory Categoria { get; set; }

    /// <summary>Indica si el producto está reservado por otro usuario.</summary>
    public bool Reservado { get; set; } = false;

    /// <summary>Fecha hasta la cual el producto está reservado.</summary>
    public DateTime? ReservadoHasta { get; set; }

    /// <summary>ID del usuario que tiene reservado el producto.</summary>
    public long? ReservadoPor { get; set; }

    /// <summary>Indica si el producto está eliminado (soft-delete).</summary>
    public bool Deleted { get; set; }

    /// <summary>Fecha de eliminación en UTC (null si no eliminado).</summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>ID del usuario que realizó la eliminación.</summary>
    public string? DeletedBy { get; set; }

    /// <summary>ID del usuario propietario del producto (FK).</summary>
    public long PropietarioId { get; set; }

    /// <summary>Usuario propietario del producto.</summary>
    public virtual User Propietario { get; set; } = null!;

    /// <summary>ID de la compra asociada (null si no vendido).</summary>
    public long? CompraId { get; set; }

    /// <summary>Compra asociada al producto (null si no vendido).</summary>
    public virtual Purchase? Compra { get; set; }

    /// <summary>Lista de favoritos asociados al producto.</summary>
    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();

    /// <summary>Lista de valoraciones del producto.</summary>
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    /// <summary>
    /// Obtiene la URL de la imagen con valor por defecto.
    /// </summary>
    /// <returns>URL absoluta, relativa o por defecto.</returns>
    public string ImagenOrDefault => string.IsNullOrEmpty(Imagen)
        ? "/images/default-product.svg"
        : Imagen.StartsWith("http") || Imagen.StartsWith("/")
            ? Imagen
            : $"/uploads/{Imagen}";

    /// <summary>
    /// Calcula el promedio de valoraciones del producto.
    /// </summary>
    /// <returns>Promedio de puntuación (0 si no hay valoraciones).</returns>
    public double RatingPromedio => Ratings.Any() ? Ratings.Average(r => r.Puntuacion) : 0;

    /// <summary>
    /// Realiza un soft-delete del producto.
    /// </summary>
    /// <param name="deletedBy">ID del usuario que elimina el producto.</param>
    public void SoftDelete(string deletedBy)
    {
        Deleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }
}
