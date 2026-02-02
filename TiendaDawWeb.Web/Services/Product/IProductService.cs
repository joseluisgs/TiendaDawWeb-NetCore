using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Product;

/// <summary>
/// Interfaz de servicio para gestión de productos del marketplace.
/// Define las operaciones CRUD y búsqueda de productos.
/// </summary>
public interface IProductService
{
    /// <summary>Obtiene un producto por su ID.</summary>
    /// <param name="id">ID del producto.</param>
    /// <returns>Resultado con el producto o error.</returns>
    Task<Result<Models.Product, DomainError>> GetByIdAsync(long id);

    /// <summary>Obtiene todos los productos disponibles (no eliminados y no vendidos).</summary>
    /// <returns>Resultado con colección de productos.</returns>
    Task<Result<IEnumerable<Models.Product>, DomainError>> GetAllAsync();

    /// <summary>Busca productos por nombre y/o categoría.</summary>
    /// <param name="nombre">Texto a buscar en nombre o descripción (null = cualquier).</param>
    /// <param name="categoria">Nombre de categoría a filtrar (null = cualquier).</param>
    /// <returns>Resultado con colección de productos filtrados.</returns>
    Task<Result<IEnumerable<Models.Product>, DomainError>> SearchAsync(string? nombre, string? categoria);

    /// <summary>Crea un nuevo producto.</summary>
    /// <param name="product">Datos del producto a crear.</param>
    /// <returns>Resultado con el producto creado.</returns>
    Task<Result<Models.Product, DomainError>> CreateAsync(Models.Product product);

    /// <summary>Actualiza un producto existente.</summary>
    /// <param name="id">ID del producto.</param>
    /// <param name="product">Nuevos datos del producto.</param>
    /// <param name="userId">ID del usuario que realiza la actualización.</param>
    /// <returns>Resultado con el producto actualizado o error.</returns>
    Task<Result<Models.Product, DomainError>> UpdateAsync(long id, Models.Product product, long userId);

    /// <summary>Elimina un producto (soft-delete).</summary>
    /// <param name="id">ID del producto.</param>
    /// <param name="userId">ID del usuario que elimina.</param>
    /// <param name="isAdmin">Indica si el usuario es administrador.</param>
    /// <returns>Resultado de la operación.</returns>
    Task<Result<bool, DomainError>> DeleteAsync(long id, long userId, bool isAdmin = false);
}
