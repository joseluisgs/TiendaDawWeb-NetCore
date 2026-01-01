using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Interfaces;

/// <summary>
/// Servicio para la gestión del carrito de compras
/// Sin cantidad - cada producto solo puede añadirse una vez (coincide con Spring Boot original)
/// </summary>
public interface ICarritoService
{
    /// <summary>
    /// Obtiene todos los items del carrito de un usuario
    /// </summary>
    Task<Result<IEnumerable<CarritoItem>, DomainError>> GetCarritoByUsuarioIdAsync(long usuarioId);

    /// <summary>
    /// Agrega un producto al carrito - si ya existe, retorna error
    /// </summary>
    Task<Result<CarritoItem, DomainError>> AddToCarritoAsync(long usuarioId, long productoId);

    /// <summary>
    /// Elimina un item del carrito
    /// </summary>
    Task<Result<bool, DomainError>> RemoveFromCarritoAsync(long itemId);

    /// <summary>
    /// Vacía completamente el carrito de un usuario
    /// </summary>
    Task<Result<bool, DomainError>> ClearCarritoAsync(long usuarioId);

    /// <summary>
    /// Obtiene el total del carrito de un usuario
    /// </summary>
    Task<Result<decimal, DomainError>> GetTotalCarritoAsync(long usuarioId);

    /// <summary>
    /// Obtiene la cantidad de items en el carrito
    /// </summary>
    Task<Result<int, DomainError>> GetCarritoCountAsync(long usuarioId);
}
