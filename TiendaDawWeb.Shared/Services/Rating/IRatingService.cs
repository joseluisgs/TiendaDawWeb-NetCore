using CSharpFunctionalExtensions;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Shared.Services.Rating;

/// <summary>
///     Servicio para la gestión de valoraciones de productos
/// </summary>
public interface IRatingService {
    Task<Result<Models.Rating, DomainError>> AddRatingAsync(long usuarioId, long productoId, int puntuacion, string? comentario);
    Task<Result<IEnumerable<Models.Rating>, DomainError>> GetByProductoIdAsync(long productoId);
    Task<Result<Models.Rating, DomainError>> GetByIdAsync(long id);
    Task<Result<Models.Rating, DomainError>> UpdateRatingAsync(long ratingId, long usuarioId, int puntuacion, string? comentario);
    Task<Result<bool, DomainError>> DeleteRatingAsync(long ratingId, long usuarioId, bool isAdmin = false);
    Task<Result<double, DomainError>> GetAverageRatingAsync(long productoId);
    Task<Result<bool, DomainError>> CanUserRateProductAsync(long usuarioId, long productoId);
}
