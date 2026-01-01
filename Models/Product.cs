using System.ComponentModel.DataAnnotations;
using TiendaDawWeb.Models.Enums;

namespace TiendaDawWeb.Models;

/// <summary>
/// Entidad de producto del marketplace
/// </summary>
public class Product
{
    public long Id { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [StringLength(200)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La descripción es obligatoria")]
    [StringLength(1000)]
    public string Descripcion { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero")]
    public decimal Precio { get; set; }

    public string? Imagen { get; set; }

    [Required]
    public ProductCategory Categoria { get; set; }

    public bool Reservado { get; set; } = false;
    public DateTime? ReservadoHasta { get; set; }

    public bool Deleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Relaciones
    public long PropietarioId { get; set; }
    public virtual User Propietario { get; set; } = null!;

    public long? CompraId { get; set; }
    public virtual Purchase? Compra { get; set; }

    public virtual ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public virtual ICollection<Rating> Ratings { get; set; } = new List<Rating>();

    // Propiedades calculadas
    public string ImagenOrDefault => Imagen ?? "/images/default-product.jpg";
    public double RatingPromedio => Ratings.Any() ? Ratings.Average(r => r.Puntuacion) : 0;
}
