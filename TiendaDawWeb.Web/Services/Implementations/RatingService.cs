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
    /// <summary>
    ///     Añade una nueva valoración a un producto validando las reglas de negocio.
    /// </summary>
    /// <param name="usuarioId">ID del usuario que valora.</param>
    /// <param name="productoId">ID del producto a valorar.</param>
    /// <param name="puntuacion">Puntuación entre 1 y 5.</param>
    /// <param name="comentario">Comentario opcional.</param>
    /// <returns>La valoración creada o un error de dominio.</returns>
    public async Task<Result<Rating, DomainError>> AddRatingAsync(
        long usuarioId, long productoId, int puntuacion, string? comentario) {
        logger.LogInformation(
            "Intentando añadir valoración: Usuario {UserId}, Producto {ProductId}, Puntuación {Puntuacion}",
            usuarioId, productoId, puntuacion);
        try {
            // 1. Validar puntuación
            if (puntuacion < 1 || puntuacion > 5) {
                logger.LogWarning("Validación fallida: Puntuación {Puntuacion} fuera de rango", puntuacion);
                return Result.Failure<Rating, DomainError>(RatingError.InvalidRating);
            }

            // 2. Verificar que el producto existe
            var producto = await context.Products
                .FirstOrDefaultAsync(p => p.Id == productoId && !p.Deleted);

            if (producto == null) {
                logger.LogWarning("Validación fallida: Producto {ProductId} no encontrado o eliminado", productoId);
                return Result.Failure<Rating, DomainError>(RatingError.ProductNotFound(productoId));
            }

            // 4. Verificar que no haya valorado ya el producto
            var existingRating = await context.Ratings
                .FirstOrDefaultAsync(r => r.UsuarioId == usuarioId && r.ProductoId == productoId);

            if (existingRating != null) {
                logger.LogWarning("Validación fallida: El usuario {UserId} ya ha valorado el producto {ProductId}",
                    usuarioId, productoId);
                return Result.Failure<Rating, DomainError>(RatingError.AlreadyRated);
            }

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

            logger.LogInformation("✅ Valoración guardada con éxito. ID: {RatingId}", rating.Id);

            // Cargar las relaciones
            await context.Entry(rating).Reference(r => r.Usuario).LoadAsync();
            await context.Entry(rating).Reference(r => r.Producto).LoadAsync();

            return Result.Success<Rating, DomainError>(rating);
        }
        catch (Exception ex) {
            logger.LogError(ex, "❌ Error crítico al añadir valoración para producto {ProductId}", productoId);
            return Result.Failure<Rating, DomainError>(
                GenericError.DatabaseError("Error al guardar la valoración"));
        }
    }

    /// <summary>
    ///     Obtiene todas las valoraciones asociadas a un producto.
    /// </summary>
    public async Task<Result<IEnumerable<Rating>, DomainError>> GetByProductoIdAsync(long productoId) {
        logger.LogDebug("Obteniendo valoraciones para el producto {ProductId}", productoId);
        try {
            var ratings = await context.Ratings
                .Include(r => r.Usuario)
                .Where(r => r.ProductoId == productoId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            logger.LogDebug("Se han recuperado {Count} valoraciones para el producto {ProductId}", ratings.Count,
                productoId);
            return Result.Success<IEnumerable<Rating>, DomainError>(ratings);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al obtener valoraciones del producto {ProductId}", productoId);
            return Result.Failure<IEnumerable<Rating>, DomainError>(
                GenericError.DatabaseError("Error al obtener valoraciones"));
        }
    }

    /// <summary>
    ///     Obtiene una valoración específica por su identificador.
    /// </summary>
    public async Task<Result<Rating, DomainError>> GetByIdAsync(long id) {
        logger.LogDebug("Buscando valoración por ID: {RatingId}", id);
        try {
            var rating = await context.Ratings
                .Include(r => r.Usuario)
                .Include(r => r.Producto)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (rating == null) {
                logger.LogWarning("Valoración {RatingId} no encontrada", id);
                return Result.Failure<Rating, DomainError>(RatingError.NotFound(id));
            }

            return Result.Success<Rating, DomainError>(rating);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al obtener valoración {RatingId}", id);
            return Result.Failure<Rating, DomainError>(
                GenericError.DatabaseError("Error al obtener la valoración"));
        }
    }

    /// <summary>
    ///     Actualiza una valoración existente validando la propiedad del usuario.
    /// </summary>
    public async Task<Result<Rating, DomainError>> UpdateRatingAsync(
        long ratingId, long usuarioId, int puntuacion, string? comentario) {
        logger.LogInformation("Intentando actualizar valoración {RatingId} por usuario {UserId}", ratingId, usuarioId);
        try {
            if (puntuacion < 1 || puntuacion > 5) return Result.Failure<Rating, DomainError>(RatingError.InvalidRating);

            var rating = await context.Ratings
                .Include(r => r.Usuario)
                .Include(r => r.Producto)
                .FirstOrDefaultAsync(r => r.Id == ratingId);

            if (rating == null) return Result.Failure<Rating, DomainError>(RatingError.NotFound(ratingId));

            if (rating.UsuarioId != usuarioId) {
                logger.LogWarning(
                    "Intento de acceso no autorizado: Usuario {UserId} intentó editar valoración de {OwnerId}",
                    usuarioId, rating.UsuarioId);
                return Result.Failure<Rating, DomainError>(RatingError.Unauthorized);
            }

            rating.Puntuacion = puntuacion;
            rating.Comentario = comentario;

            await context.SaveChangesAsync();
            logger.LogInformation("✅ Valoración {RatingId} actualizada correctamente", ratingId);

            return Result.Success<Rating, DomainError>(rating);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al actualizar valoración {RatingId}", ratingId);
            return Result.Failure<Rating, DomainError>(
                GenericError.DatabaseError("Error al actualizar la valoración"));
        }
    }

    /// <summary>
    ///     Elimina una valoración del sistema.
    /// </summary>
    public async Task<Result<bool, DomainError>>
        DeleteRatingAsync(long ratingId, long usuarioId, bool isAdmin = false) {
        logger.LogInformation("Intentando eliminar valoración {RatingId}. Usuario: {UserId}, IsAdmin: {IsAdmin}",
            ratingId, usuarioId, isAdmin);
        try {
            var rating = await context.Ratings
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(r => r.Id == ratingId);

            if (rating == null) return Result.Failure<bool, DomainError>(RatingError.NotFound(ratingId));

            if (!isAdmin && rating.UsuarioId != usuarioId) {
                logger.LogWarning("Intento de borrado no autorizado: Usuario {UserId}", usuarioId);
                return Result.Failure<bool, DomainError>(RatingError.Unauthorized);
            }

            context.Ratings.Remove(rating);
            await context.SaveChangesAsync();
            logger.LogInformation("✅ Valoración {RatingId} eliminada", ratingId);

            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al eliminar valoración {RatingId}", ratingId);
            return Result.Failure<bool, DomainError>(
                GenericError.DatabaseError("Error al eliminar la valoración"));
        }
    }

    /// <summary>
    ///     Calcula el promedio de estrellas de un producto.
    /// </summary>
    public async Task<Result<double, DomainError>> GetAverageRatingAsync(long productoId) {
        logger.LogDebug("Calculando promedio para producto {ProductId}", productoId);
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

    /// <summary>
    ///     Verifica si un usuario tiene permisos para valorar un producto.
    /// </summary>
    public async Task<Result<bool, DomainError>> CanUserRateProductAsync(long usuarioId, long productoId) {
        logger.LogDebug("Verificando permisos de valoración: Usuario {UserId}, Producto {ProductId}", usuarioId,
            productoId);
        try {
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