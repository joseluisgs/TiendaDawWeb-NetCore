using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Interfaces;

/// <summary>
///     Servicio para la gestión de valoraciones de productos
/// </summary>
public interface IRatingService {
    /// <summary>
    ///     Añade una valoración a un producto
    /// </summary>
    Task<Result<Rating, DomainError>> AddRatingAsync(long usuarioId, long productoId, int puntuacion,
        string? comentario);

    /// <summary>
    ///     Obtiene todas las valoraciones de un producto
    /// </summary>
    Task<Result<IEnumerable<Rating>, DomainError>> GetByProductoIdAsync(long productoId);

    /// <summary>
    ///     Obtiene una valoración por ID
    /// </summary>
    Task<Result<Rating, DomainError>> GetByIdAsync(long id);

    /// <summary>
    ///     Actualiza una valoración (solo el propietario)
    /// </summary>
    Task<Result<Rating, DomainError>> UpdateRatingAsync(long ratingId, long usuarioId, int puntuacion,
        string? comentario);

    /// <summary>
    ///     Elimina una valoración (solo el propietario o admin)
    /// </summary>
    Task<Result<bool, DomainError>> DeleteRatingAsync(long ratingId, long usuarioId, bool isAdmin = false);

    /// <summary>
    ///     Obtiene el promedio de puntuación de un producto
    /// </summary>
    Task<Result<double, DomainError>> GetAverageRatingAsync(long productoId);

    /// <summary>
    ///     Verifica si un usuario puede valorar un producto
    /// </summary>
    Task<Result<bool, DomainError>> CanUserRateProductAsync(long usuarioId, long productoId);
}