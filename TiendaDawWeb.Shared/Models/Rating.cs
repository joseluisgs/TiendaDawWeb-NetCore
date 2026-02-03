using System.ComponentModel.DataAnnotations;

namespace TiendaDawWeb.Shared.Models;

/// <summary>
/// Entidad de valoración de un producto por un usuario.
/// Representa una review con puntuación y comentario opcional.
/// </summary>
public class Rating : AuditableEntity
{
    /// <summary>ID único de la valoración (PK en SQLite).</summary>
    public long Id { get; set; }

    /// <summary>Puntuación del 1 al 5 (obligatoria).</summary>
    [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5")]
    public int Puntuacion { get; set; }

    /// <summary>Comentario opcional de la valoración (0-500 caracteres).</summary>
    [StringLength(500)]
    public string? Comentario { get; set; }

    /// <summary>ID del usuario que realiza la valoración (FK).</summary>
    public long UsuarioId { get; set; }

    /// <summary>Usuario que realiza la valoración.</summary>
    public virtual User Usuario { get; set; } = null!;

    /// <summary>ID del producto valorado (FK).</summary>
    public long ProductoId { get; set; }

    /// <summary>Producto valorado.</summary>
    public virtual Product Producto { get; set; } = null!;
}
