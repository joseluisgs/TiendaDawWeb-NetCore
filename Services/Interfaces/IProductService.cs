using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Interfaces;

/// <summary>
/// Interfaz de servicio para gestión de productos
/// </summary>
public interface IProductService
{
    Task<Result<Product, DomainError>> GetByIdAsync(long id);
    Task<Result<IEnumerable<Product>, DomainError>> GetAllAsync();
    Task<Result<IEnumerable<Product>, DomainError>> SearchAsync(string? nombre, string? categoria);
    Task<Result<Product, DomainError>> CreateAsync(Product product);
    Task<Result<Product, DomainError>> UpdateAsync(long id, Product product, long userId);
    Task<Result<bool, DomainError>> DeleteAsync(long id, long userId, bool isAdmin = false);
}
