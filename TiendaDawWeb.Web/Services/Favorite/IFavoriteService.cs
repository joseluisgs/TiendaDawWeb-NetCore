using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Favorite;

/// <summary>
/// Interfaz de servicio para gestión de favoritos
/// </summary>
public interface IFavoriteService
{
    Task<Result<bool, DomainError>> IsFavoriteAsync(long userId, long productId);
    Task<Result<Models.Favorite, DomainError>> AddFavoriteAsync(long userId, long productId);
    Task<Result<bool, DomainError>> RemoveFavoriteAsync(long userId, long productId);
    Task<Result<IEnumerable<Models.Product>, DomainError>> GetUserFavoritesAsync(long userId);
}
