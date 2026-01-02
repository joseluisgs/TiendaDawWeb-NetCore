using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Interfaces;

/// <summary>
/// Interfaz de servicio para gestión de favoritos
/// </summary>
public interface IFavoriteService
{
    Task<Result<bool, DomainError>> IsFavoriteAsync(long userId, long productId);
    Task<Result<Favorite, DomainError>> AddFavoriteAsync(long userId, long productId);
    Task<Result<bool, DomainError>> RemoveFavoriteAsync(long userId, long productId);
    Task<Result<IEnumerable<Product>, DomainError>> GetUserFavoritesAsync(long userId);
}
