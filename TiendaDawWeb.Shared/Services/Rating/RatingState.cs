using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Shared.Services.Rating;

/// <summary>
/// Representa el estado inmutable de las valoraciones.
/// </summary>
public record RatingState
{
    /// <summary>
    /// Lista de valoraciones del producto.
    /// </summary>
    public List<Models.Rating> Ratings { get; init; } = new();
    
    /// <summary>
    /// ID del producto actualmente cargado.
    /// </summary>
    public long CurrentProductId { get; init; }
    
    /// <summary>
    /// Promedio de puntuación de las valoraciones.
    /// </summary>
    public double Average => Ratings.Any() ? Ratings.Average(r => r.Puntuacion) : 0;
    
    /// <summary>
    /// Cantidad total de valoraciones.
    /// </summary>
    public int Count => Ratings.Count;
    
    /// <summary>
    /// Indica si hay valoraciones cargadas.
    /// </summary>
    public bool HasRatings => Ratings.Any();
    
    /// <summary>
    /// Constructor por defecto.
    /// </summary>
    public RatingState() { }
    
    /// <summary>
    /// Constructor con valores iniciales.
    /// </summary>
    /// <param name="ratings">Lista de valoraciones.</param>
    /// <param name="productId">ID del producto.</param>
    public RatingState(List<Models.Rating> ratings, long productId)
    {
        Ratings = ratings;
        CurrentProductId = productId;
    }
}
