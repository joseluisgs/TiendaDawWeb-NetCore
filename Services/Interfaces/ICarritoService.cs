using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Interfaces;

/// <summary>
/// Servicio para la gestión del carrito de compras con control de concurrencia
/// </summary>
public interface ICarritoService
{
    /// <summary>
    /// Obtiene todos los items del carrito de un usuario
    /// </summary>
    Task<Result<IEnumerable<CarritoItem>, DomainError>> GetCarritoByUsuarioIdAsync(long usuarioId);

    /// <summary>
    /// Agrega un producto al carrito (con control de concurrencia)
    /// </summary>
    Task<Result<CarritoItem, DomainError>> AddToCarritoAsync(long usuarioId, long productoId, int cantidad = 1);

    /// <summary>
    /// Actualiza la cantidad de un item en el carrito (con control de concurrencia)
    /// </summary>
    Task<Result<CarritoItem, DomainError>> UpdateCantidadAsync(long itemId, int nuevaCantidad);

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
