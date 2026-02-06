using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Rating;

namespace TiendaDawWeb.Shared.Services.Rating;

/// <summary>
///     Servicio de gestión de valoraciones de productos con Railway Oriented Programming
/// </summary>
public class RatingService(
    ApplicationDbContext context,
    ILogger<RatingService> logger
) : IRatingService
{
    /// <summary>
    ///     Añade una nueva valoración a un producto.
    /// </summary>
    /// <param name="usuarioId">ID del usuario que valora</param>
    /// <param name="productoId">ID del producto valorado</param>
    /// <param name="puntuacion">Puntuación del 1 al 5</param>
    /// <param name="comentario">Comentario opcional de la valoración</param>
    /// <returns>La valoración creada o un error</returns>
    public async Task<Result<Models.Rating, DomainError>> AddRatingAsync(
        long usuarioId, long productoId, int puntuacion, string? comentario)
    {
        if (puntuacion < 1 || puntuacion > 5)
            return Result.Failure<Models.Rating, DomainError>(RatingError.InvalidRating());

        var producto = await context.Products.FirstOrDefaultAsync(p => p.Id == productoId && !p.Deleted);
        if (producto == null)
            return Result.Failure<Models.Rating, DomainError>(RatingError.ProductNotFound(productoId));

        var existingRating = await context.Ratings
            .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId && r.ProductoId == productoId);

        if (existingRating != null)
            return Result.Failure<Models.Rating, DomainError>(RatingError.AlreadyRated());

        var rating = new Models.Rating
        {
            UsuarioId = usuarioId,
            ProductoId = productoId,
            Puntuacion = puntuacion,
            Comentario = comentario,
            CreatedAt = DateTime.UtcNow
        };

        context.Ratings.Add(rating);
        await context.SaveChangesAsync();

        return Result.Success<Models.Rating, DomainError>(rating);
    }

    /// <summary>
    ///     Obtiene todas las valoraciones de un producto ordenadas por fecha descendente.
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Lista de valoraciones o error</returns>
    public async Task<Result<IEnumerable<Models.Rating>, DomainError>> GetByProductoIdAsync(long productoId)
    {
        try
        {
            var ratings = await context.Ratings
                .Include(r => r.Usuario)
                .Where(r => r.ProductoId == productoId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Result.Success<IEnumerable<Models.Rating>, DomainError>(ratings);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener valoraciones");
            return Result.Failure<IEnumerable<Models.Rating>, DomainError>(GenericError.DatabaseError("Error"));
        }
    }

    /// <summary>
    ///     Obtiene una valoración específica por su ID.
    /// </summary>
    /// <param name="id">ID de la valoración</param>
    /// <returns>La valoración encontrada o error</returns>
    public async Task<Result<Models.Rating, DomainError>> GetByIdAsync(long id)
    {
        try
        {
            var rating = await context.Ratings
                .Include(r => r.Usuario)
                .Include(r => r.Producto)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rating == null)
                return Result.Failure<Models.Rating, DomainError>(RatingError.NotFound(id));

            return Result.Success<Models.Rating, DomainError>(rating);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener valoracion");
            return Result.Failure<Models.Rating, DomainError>(GenericError.DatabaseError("Error"));
        }
    }

    /// <summary>
    ///     Actualiza una valoración existente del usuario.
    /// </summary>
    /// <param name="ratingId">ID de la valoración a actualizar</param>
    /// <param name="usuarioId">ID del usuario propietario</param>
    /// <param name="puntuacion">Nueva puntuación (1-5)</param>
    /// <param name="comentario">Nuevo comentario opcional</param>
    /// <returns>La valoración actualizada o error</returns>
    public async Task<Result<Models.Rating, DomainError>> UpdateRatingAsync(
        long ratingId, long usuarioId, int puntuacion, string? comentario)
    {
        if (puntuacion < 1 || puntuacion > 5)
            return Result.Failure<Models.Rating, DomainError>(RatingError.InvalidRating());

        var rating = await context.Ratings
            .Include(r => r.Usuario)
            .Include(r => r.Producto)
            .FirstOrDefaultAsync(r => r.Id == ratingId);

        if (rating == null)
            return Result.Failure<Models.Rating, DomainError>(RatingError.NotFound(ratingId));

        if (rating.UsuarioId != usuarioId)
            return Result.Failure<Models.Rating, DomainError>(RatingError.Unauthorized());

        rating.Puntuacion = puntuacion;
        rating.Comentario = comentario;

        await context.SaveChangesAsync();

        return Result.Success<Models.Rating, DomainError>(rating);
    }

    /// <summary>
    ///     Elimina una valoración. Solo el propietario o admin pueden eliminarla.
    /// </summary>
    /// <param name="ratingId">ID de la valoración</param>
    /// <param name="usuarioId">ID del usuario que elimina</param>
    /// <param name="isAdmin">Indica si el usuario es administrador</param>
    /// <returns>True si se eliminó o error</returns>
    public async Task<Result<bool, DomainError>> DeleteRatingAsync(long ratingId, long usuarioId, bool isAdmin = false)
    {
        try
        {
            var rating = await context.Ratings.IgnoreQueryFilters().FirstOrDefaultAsync(r => r.Id == ratingId);

            if (rating == null)
                return Result.Failure<bool, DomainError>(RatingError.NotFound(ratingId));

            if (!isAdmin && rating.UsuarioId != usuarioId)
                return Result.Failure<bool, DomainError>(RatingError.Unauthorized());

            context.Ratings.Remove(rating);
            await context.SaveChangesAsync();

            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al eliminar valoracion");
            return Result.Failure<bool, DomainError>(GenericError.DatabaseError("Error"));
        }
    }

    /// <summary>
    ///     Calcula el promedio de puntuación de un producto.
    /// </summary>
    /// <param name="productoId">ID del producto</param>
    /// <returns>Promedio de puntuación o 0 si no hay valoraciones</returns>
    public async Task<Result<double, DomainError>> GetAverageRatingAsync(long productoId)
    {
        try
        {
            var ratings = await context.Ratings.Where(r => r.ProductoId == productoId).ToListAsync();

            if (!ratings.Any())
                return Result.Success<double, DomainError>(0.0);

            return Result.Success<double, DomainError>(ratings.Average(r => r.Puntuacion));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al calcular promedio");
            return Result.Failure<double, DomainError>(GenericError.DatabaseError("Error"));
        }
    }

    /// <summary>
    ///     Verifica si un usuario puede valorar un producto.
    ///     Solo pueden valorar usuarios que han comprado el producto.
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="productoId">ID del producto</param>
    /// <returns>True si puede valorar o error</returns>
    public async Task<Result<bool, DomainError>> CanUserRateProductAsync(long usuarioId, long productoId)
    {
        try
        {
            var hasPurchased = await context.Purchases
                .Where(p => p.CompradorId == usuarioId)
                .SelectMany(p => p.Products)
                .AnyAsync(prod => prod.Id == productoId);

            return Result.Success<bool, DomainError>(hasPurchased);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al verificar permisos");
            return Result.Failure<bool, DomainError>(GenericError.DatabaseError("Error"));
        }
    }
}
