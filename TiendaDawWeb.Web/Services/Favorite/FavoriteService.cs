using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Favorite;

namespace TiendaDawWeb.Services.Favorite;

/// <summary>
///     Servicio de gestión de favoritos con Railway Oriented Programming
/// </summary>
public class FavoriteService(
    ApplicationDbContext context,
    ILogger<FavoriteService> logger
) : IFavoriteService {
    public async Task<Result<bool, DomainError>> IsFavoriteAsync(long userId, long productId) {
        try {
            var exists = await context.Favorites
                .AnyAsync(f => f.UsuarioId == userId && f.ProductoId == productId);

            return Result.Success<bool, DomainError>(exists);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error verificando favorito");
            return Result.Success<bool, DomainError>(false);
        }
    }

    public async Task<Result<Models.Favorite, DomainError>> AddFavoriteAsync(long userId, long productId) {
        try {
            var existsResult = await IsFavoriteAsync(userId, productId);
            if (existsResult.Value)
                return Result.Failure<Models.Favorite, DomainError>(FavoriteError.AlreadyExists());

            var favorite = new Models.Favorite {
                UsuarioId = userId,
                ProductoId = productId
            };

            context.Favorites.Add(favorite);
            await context.SaveChangesAsync();

            logger.LogInformation("Favorito añadido: Usuario {UserId}, Producto {ProductId}", userId, productId);
            return Result.Success<Models.Favorite, DomainError>(favorite);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error añadiendo favorito");
            return Result.Failure<Models.Favorite, DomainError>(
                FavoriteError.ProductNotFound(productId));
        }
    }

    public async Task<Result<bool, DomainError>> RemoveFavoriteAsync(long userId, long productId) {
        try {
            var favorite = await context.Favorites
                .FirstOrDefaultAsync(f => f.UsuarioId == userId && f.ProductoId == productId);

            if (favorite == null)
                return Result.Failure<bool, DomainError>(FavoriteError.NotFound());

            context.Favorites.Remove(favorite);
            await context.SaveChangesAsync();

            logger.LogInformation("Favorito eliminado: Usuario {UserId}, Producto {ProductId}", userId, productId);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error eliminando favorito");
            return Result.Failure<bool, DomainError>(FavoriteError.NotFound());
        }
    }

    public async Task<Result<IEnumerable<Models.Product>, DomainError>> GetUserFavoritesAsync(long userId) {
        try {
            var favorites = await context.Favorites
                .Where(f => f.UsuarioId == userId)
                .Include(f => f.Producto)
                .ThenInclude(p => p.Propietario)
                .Include(f => f.Producto.Ratings)
                .Select(f => f.Producto)
                .ToListAsync();

            return Result.Success<IEnumerable<Models.Product>, DomainError>(favorites);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error obteniendo favoritos del usuario {UserId}", userId);
            return Result.Failure<IEnumerable<Models.Product>, DomainError>(
                FavoriteError.UserNotFound(userId));
        }
    }
}
