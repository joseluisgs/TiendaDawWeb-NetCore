using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
///     Implementación del servicio de valoraciones con validaciones de negocio
/// </summary>
public class RatingService(
    ApplicationDbContext context,
    ILogger<RatingService> logger
) : IRatingService {
    public async Task<Result<Rating, DomainError>> AddRatingAsync(
        long usuarioId, long productoId, int puntuacion, string? comentario) {
        try {
            // 1. Validar puntuación
            if (puntuacion < 1 || puntuacion > 5) return Result.Failure<Rating, DomainError>(RatingError.InvalidRating);

            // 2. Verificar que el producto existe
            var producto = await context.Products
                .FirstOrDefaultAsync(p => p.Id == productoId && !p.Deleted);

            if (producto == null) return Result.Failure<Rating, DomainError>(RatingError.ProductNotFound(productoId));

            // 3. Verificar que el usuario ha comprado el producto
            var canRateResult = await CanUserRateProductAsync(usuarioId, productoId);
            if (canRateResult.IsFailure) return Result.Failure<Rating, DomainError>(canRateResult.Error);

            if (!canRateResult.Value) return Result.Failure<Rating, DomainError>(RatingError.ProductNotPurchased);

            // 4. Verificar que no haya valorado ya el producto
            var existingRating = await context.Ratings
                .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId && r.ProductoId == productoId);

            if (existingRating != null) return Result.Failure<Rating, DomainError>(RatingError.AlreadyRated);

            // 5. Crear la valoración
            var rating = new Rating {
                UsuarioId = usuarioId,
                ProductoId = productoId,
                Puntuacion = puntuacion,
                Comentario = comentario,
                CreatedAt = DateTime.UtcNow
            };

            context.Ratings.Add(rating);
            await context.SaveChangesAsync();

            logger.LogInformation("Usuario {UserId} valoró producto {ProductId} con {Puntuacion} estrellas",
                usuarioId, productoId, puntuacion);

            // Cargar las relaciones
            await context.Entry(rating)
                .Reference(r => r.Usuario)
                .LoadAsync();
            await context.Entry(rating)
                .Reference(r => r.Producto)
                .LoadAsync();

            return Result.Success<Rating, DomainError>(rating);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al añadir valoración para producto {ProductId}", productoId);
            return Result.Failure<Rating, DomainError>(
                GenericError.DatabaseError("Error al guardar la valoración"));
        }
    }

    public async Task<Result<IEnumerable<Rating>, DomainError>> GetByProductoIdAsync(long productoId) {
        try {
            var ratings = await context.Ratings
                .Include(r => r.Usuario)
                .Where(r => r.ProductoId == productoId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return Result.Success<IEnumerable<Rating>, DomainError>(ratings);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al obtener valoraciones del producto {ProductId}", productoId);
            return Result.Failure<IEnumerable<Rating>, DomainError>(
                GenericError.DatabaseError("Error al obtener valoraciones"));
        }
    }

    public async Task<Result<Rating, DomainError>> GetByIdAsync(long id) {
        try {
            var rating = await context.Ratings
                .Include(r => r.Usuario)
                .Include(r => r.Producto)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rating == null) return Result.Failure<Rating, DomainError>(RatingError.NotFound(id));

            return Result.Success<Rating, DomainError>(rating);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al obtener valoración {RatingId}", id);
            return Result.Failure<Rating, DomainError>(
                GenericError.DatabaseError("Error al obtener la valoración"));
        }
    }

    public async Task<Result<Rating, DomainError>> UpdateRatingAsync(
        long ratingId, long usuarioId, int puntuacion, string? comentario) {
        try {
            // 1. Validar puntuación
            if (puntuacion < 1 || puntuacion > 5) return Result.Failure<Rating, DomainError>(RatingError.InvalidRating);

            // 2. Obtener la valoración
            var rating = await context.Ratings
                .Include(r => r.Usuario)
                .Include(r => r.Producto)
                .FirstOrDefaultAsync(r => r.Id == ratingId);

            if (rating == null) return Result.Failure<Rating, DomainError>(RatingError.NotFound(ratingId));

            // 3. Verificar que sea el propietario
            if (rating.UsuarioId != usuarioId) return Result.Failure<Rating, DomainError>(RatingError.Unauthorized);

            // 4. Actualizar
            rating.Puntuacion = puntuacion;
            rating.Comentario = comentario;

            await context.SaveChangesAsync();

            logger.LogInformation("Valoración {RatingId} actualizada por usuario {UserId}",
                ratingId, usuarioId);

            return Result.Success<Rating, DomainError>(rating);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al actualizar valoración {RatingId}", ratingId);
            return Result.Failure<Rating, DomainError>(
                GenericError.DatabaseError("Error al actualizar la valoración"));
        }
    }

    public async Task<Result<bool, DomainError>>
        DeleteRatingAsync(long ratingId, long usuarioId, bool isAdmin = false) {
        try {
            var rating = await context.Ratings
                .IgnoreQueryFilters() // Ignorar filtros globales para permitir eliminar ratings de productos eliminados
                .FirstOrDefaultAsync(r => r.Id == ratingId);

            if (rating == null) return Result.Failure<bool, DomainError>(RatingError.NotFound(ratingId));

            // Verificar permisos - DEBE ir antes que verificar si existe
            if (!isAdmin && rating.UsuarioId != usuarioId)
                return Result.Failure<bool, DomainError>(RatingError.Unauthorized);

            context.Ratings.Remove(rating);
            await context.SaveChangesAsync();

            logger.LogInformation("Valoración {RatingId} eliminada por usuario {UserId}",
                ratingId, usuarioId);

            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al eliminar valoración {RatingId}", ratingId);
            return Result.Failure<bool, DomainError>(
                GenericError.DatabaseError("Error al eliminar la valoración"));
        }
    }

    public async Task<Result<double, DomainError>> GetAverageRatingAsync(long productoId) {
        try {
            var ratings = await context.Ratings
                .Where(r => r.ProductoId == productoId)
                .ToListAsync();

            if (!ratings.Any()) return Result.Success<double, DomainError>(0.0);

            var average = ratings.Average(r => r.Puntuacion);
            return Result.Success<double, DomainError>(average);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al calcular promedio de valoraciones para producto {ProductId}", productoId);
            return Result.Failure<double, DomainError>(
                GenericError.DatabaseError("Error al calcular promedio"));
        }
    }

    public async Task<Result<bool, DomainError>> CanUserRateProductAsync(long usuarioId, long productoId) {
        try {
            // El usuario puede valorar si ha comprado el producto
            var hasPurchased = await context.Purchases
                .Where(p => p.CompradorId == usuarioId)
                .SelectMany(p => p.Products)
                .AnyAsync(prod => prod.Id == productoId);

            return Result.Success<bool, DomainError>(hasPurchased);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al verificar si usuario {UserId} puede valorar producto {ProductId}",
                usuarioId, productoId);
            return Result.Failure<bool, DomainError>(
                GenericError.DatabaseError("Error al verificar permisos"));
        }
    }
}