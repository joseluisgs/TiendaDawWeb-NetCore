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
) : IFavoriteService
{
    public async Task<Result<bool, DomainError>> IsFavoriteAsync(long userId, long productId)
    {
        try
        {
            var exists = await context.Favorites
                .AnyAsync(f => f.UsuarioId == userId && f.ProductoId == productId);

            return Result.Success<bool, DomainError>(exists);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error verificando favorito");
            return Result.Success<bool, DomainError>(false);
        }
    }

    public async Task<Result<Models.Favorite, DomainError>> AddFavoriteAsync(long userId, long productId)
    {
        var existsResult = await IsFavoriteAsync(userId, productId);
        if (existsResult.Value)
            return Result.Failure<Models.Favorite, DomainError>(FavoriteError.AlreadyExists());

        var favorite = new Models.Favorite
        {
            UsuarioId = userId,
            ProductoId = productId
        };

        return await Task.Run(() =>
        {
            context.Favorites.Add(favorite);
            context.SaveChanges();
            return Result.Success<Models.Favorite, DomainError>(favorite);
        })
        .Tap(_ =>
        {
            logger.LogInformation("Favorito añadido: Usuario {UserId}, Producto {ProductId}", userId, productId);
        });
    }

    public async Task<Result<bool, DomainError>> RemoveFavoriteAsync(long userId, long productId)
    {
        var favorite = await context.Favorites
            .FirstOrDefaultAsync(f => f.UsuarioId == userId && f.ProductoId == productId);

        if (favorite == null)
            return Result.Failure<bool, DomainError>(FavoriteError.NotFound());

        return await Task.Run(() =>
        {
            context.Favorites.Remove(favorite);
            context.SaveChanges();
            return Result.Success<bool, DomainError>(true);
        })
        .Tap(_ =>
        {
            logger.LogInformation("Favorito eliminado: Usuario {UserId}, Producto {ProductId}", userId, productId);
        });
    }

    public async Task<Result<IEnumerable<Models.Product>, DomainError>> GetUserFavoritesAsync(long userId)
    {
        try
        {
            var favorites = await context.Favorites
                .Where(f => f.UsuarioId == userId)
                .Include(f => f.Producto)
                .ThenInclude(p => p.Propietario)
                .Include(f => f.Producto.Ratings)
                .Select(f => f.Producto)
                .ToListAsync();

            return Result.Success<IEnumerable<Models.Product>, DomainError>(favorites);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo favoritos del usuario {UserId}", userId);
            return Result.Failure<IEnumerable<Models.Product>, DomainError>(
                FavoriteError.UserNotFound(userId));
        }
    }
}
