using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
/// Servicio de gestión de productos con Railway Oriented Programming
/// </summary>
public class ProductService : IProductService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProductService> _logger;

    public ProductService(ApplicationDbContext context, ILogger<ProductService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Product, DomainError>> GetByIdAsync(long id)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.Propietario)
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == id);

            return product != null
                ? Result.Success<Product, DomainError>(product)
                : Result.Failure<Product, DomainError>(ProductError.NotFound(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo producto {ProductId}", id);
            return Result.Failure<Product, DomainError>(
                ProductError.InvalidData($"Error al obtener producto: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<Product>, DomainError>> GetAllAsync()
    {
        try
        {
            var products = await _context.Products
                .Include(p => p.Propietario)
                .Include(p => p.Ratings)
                .Where(p => !p.Deleted && p.CompraId == null) // Ocultar productos ya comprados
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Result.Success<IEnumerable<Product>, DomainError>(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo todos los productos");
            return Result.Failure<IEnumerable<Product>, DomainError>(
                ProductError.InvalidData($"Error al obtener productos: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<Product>, DomainError>> SearchAsync(string? nombre, string? categoria)
    {
        try
        {
            var query = _context.Products
                .Include(p => p.Propietario)
                .Include(p => p.Ratings)
                .Where(p => !p.Deleted && p.CompraId == null); // Ocultar productos ya comprados

            if (!string.IsNullOrWhiteSpace(nombre))
            {
                query = query.Where(p => p.Nombre.Contains(nombre) || p.Descripcion.Contains(nombre));
            }

            if (!string.IsNullOrWhiteSpace(categoria) && Enum.TryParse<ProductCategory>(categoria, out var cat))
            {
                query = query.Where(p => p.Categoria == cat);
            }

            var products = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return Result.Success<IEnumerable<Product>, DomainError>(products);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error buscando productos");
            return Result.Failure<IEnumerable<Product>, DomainError>(
                ProductError.InvalidData($"Error al buscar productos: {ex.Message}"));
        }
    }

    public async Task<Result<Product, DomainError>> CreateAsync(Product product)
    {
        try
        {
            if (product.Precio <= 0)
                return Result.Failure<Product, DomainError>(ProductError.InvalidPrice);

            _context.Products.Add(product);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Producto creado: {ProductId} - {ProductName}", product.Id, product.Nombre);
            return Result.Success<Product, DomainError>(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando producto");
            return Result.Failure<Product, DomainError>(
                ProductError.InvalidData($"Error al crear producto: {ex.Message}"));
        }
    }

    public async Task<Result<Product, DomainError>> UpdateAsync(long id, Product updatedProduct, long userId)
    {
        try
        {
            var productResult = await GetByIdAsync(id);
            
            if (productResult.IsFailure)
                return productResult;

            var product = productResult.Value;

            if (product.PropietarioId != userId)
                return Result.Failure<Product, DomainError>(ProductError.NotOwner);

            product.Nombre = updatedProduct.Nombre;
            product.Descripcion = updatedProduct.Descripcion;
            product.Precio = updatedProduct.Precio;
            product.Categoria = updatedProduct.Categoria;
            
            if (!string.IsNullOrEmpty(updatedProduct.Imagen))
                product.Imagen = updatedProduct.Imagen;

            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Producto actualizado: {ProductId}", id);
            return Result.Success<Product, DomainError>(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error actualizando producto {ProductId}", id);
            return Result.Failure<Product, DomainError>(
                ProductError.InvalidData($"Error al actualizar producto: {ex.Message}"));
        }
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(long id, long userId)
    {
        try
        {
            var productResult = await GetByIdAsync(id);
            
            if (productResult.IsFailure)
                return Result.Failure<bool, DomainError>(productResult.Error);

            var product = productResult.Value;

            if (product.PropietarioId != userId)
                return Result.Failure<bool, DomainError>(ProductError.NotOwner);

            if (product.CompraId != null)
                return Result.Failure<bool, DomainError>(ProductError.AlreadySold);

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Producto eliminado: {ProductId}", id);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando producto {ProductId}", id);
            return Result.Failure<bool, DomainError>(
                ProductError.InvalidData($"Error al eliminar producto: {ex.Message}"));
        }
    }
}
