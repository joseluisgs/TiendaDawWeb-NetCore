using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
/// Servicio de gestión de favoritos con Railway Oriented Programming
/// </summary>
public class FavoriteService : IFavoriteService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<FavoriteService> _logger;

    public FavoriteService(ApplicationDbContext context, ILogger<FavoriteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<bool, DomainError>> IsFavoriteAsync(long userId, long productId)
    {
        try
        {
            var exists = await _context.Favorites
                .AnyAsync(f => f.UsuarioId == userId && f.ProductoId == productId);
            
            return Result.Success<bool, DomainError>(exists);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verificando favorito");
            return Result.Success<bool, DomainError>(false);
        }
    }

    public async Task<Result<Favorite, DomainError>> AddFavoriteAsync(long userId, long productId)
    {
        try
        {
            var existsResult = await IsFavoriteAsync(userId, productId);
            if (existsResult.Value)
                return Result.Failure<Favorite, DomainError>(FavoriteError.AlreadyExists);

            var favorite = new Favorite
            {
                UsuarioId = userId,
                ProductoId = productId
            };

            _context.Favorites.Add(favorite);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Favorito añadido: Usuario {UserId}, Producto {ProductId}", userId, productId);
            return Result.Success<Favorite, DomainError>(favorite);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error añadiendo favorito");
            return Result.Failure<Favorite, DomainError>(
                FavoriteError.ProductNotFound(productId));
        }
    }

    public async Task<Result<bool, DomainError>> RemoveFavoriteAsync(long userId, long productId)
    {
        try
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UsuarioId == userId && f.ProductoId == productId);

            if (favorite == null)
                return Result.Failure<bool, DomainError>(FavoriteError.NotFound);

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Favorito eliminado: Usuario {UserId}, Producto {ProductId}", userId, productId);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando favorito");
            return Result.Failure<bool, DomainError>(FavoriteError.NotFound);
        }
    }

    public async Task<Result<IEnumerable<Product>, DomainError>> GetUserFavoritesAsync(long userId)
    {
        try
        {
            var favorites = await _context.Favorites
                .Where(f => f.UsuarioId == userId)
                .Include(f => f.Producto)
                    .ThenInclude(p => p.Propietario)
                .Include(f => f.Producto.Ratings)
                .Select(f => f.Producto)
                .ToListAsync();

            return Result.Success<IEnumerable<Product>, DomainError>(favorites);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo favoritos del usuario {UserId}", userId);
            return Result.Failure<IEnumerable<Product>, DomainError>(
                FavoriteError.UserNotFound(userId));
        }
    }
}
