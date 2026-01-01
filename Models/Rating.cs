using System.ComponentModel.DataAnnotations;

namespace TiendaDawWeb.Models;

/// <summary>
/// Valoración de un producto por un usuario
/// </summary>
public class Rating
{
    public long Id { get; set; }

    [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5")]
    public int Puntuacion { get; set; }

    [StringLength(500)]
    public string? Comentario { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public long UsuarioId { get; set; }
    public virtual User Usuario { get; set; } = null!;

    public long ProductoId { get; set; }
    public virtual Product Producto { get; set; } = null!;
}
