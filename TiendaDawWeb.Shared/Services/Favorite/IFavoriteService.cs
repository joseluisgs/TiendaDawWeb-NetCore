using CSharpFunctionalExtensions;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Shared.Services.Favorite;

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
