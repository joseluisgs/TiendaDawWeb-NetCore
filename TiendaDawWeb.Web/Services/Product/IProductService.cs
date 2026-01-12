using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Product;

/// <summary>
/// Interfaz de servicio para gestión de productos
/// </summary>
public interface IProductService
{
    Task<Result<Models.Product, DomainError>> GetByIdAsync(long id);
    Task<Result<IEnumerable<Models.Product>, DomainError>> GetAllAsync();
    Task<Result<IEnumerable<Models.Product>, DomainError>> SearchAsync(string? nombre, string? categoria);
    Task<Result<Models.Product, DomainError>> CreateAsync(Models.Product product);
    Task<Result<Models.Product, DomainError>> UpdateAsync(long id, Models.Product product, long userId);
    Task<Result<bool, DomainError>> DeleteAsync(long id, long userId, bool isAdmin = false);
}
