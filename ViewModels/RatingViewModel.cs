using System.ComponentModel.DataAnnotations;

namespace TiendaDawWeb.ViewModels;

/// <summary>
/// ViewModel para crear/editar valoraciones
/// </summary>
public class RatingViewModel
{
    [Required(ErrorMessage = "La puntuación es obligatoria")]
    [Range(1, 5, ErrorMessage = "La puntuación debe estar entre 1 y 5")]
    public int Puntuacion { get; set; }

    [StringLength(500, ErrorMessage = "El comentario no puede superar los 500 caracteres")]
    public string? Comentario { get; set; }

    public long ProductoId { get; set; }
}
