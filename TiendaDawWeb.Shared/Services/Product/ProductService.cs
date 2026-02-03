using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;

namespace TiendaDawWeb.Shared.Services.Product;

/// <summary>
/// Servicio de productos con caché en memoria y patrón Result.
/// Implementa operaciones CRUD con validaciones y gestión de caché.
/// </summary>
public class ProductService(
    ApplicationDbContext context,
    IMemoryCache cache,
    ILogger<ProductService> logger
) : IProductService
{
    private const string ProductsCacheKey = "all_products";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Obtiene un producto por su ID con caché.
    /// Devuelve: Result.Success(Producto) | Result.Failure(NotFound)
    /// </summary>
    public async Task<Result<Models.Product, DomainError>> GetByIdAsync(long id)
    {
        try
        {
            var cacheKey = ProductDetailsCacheKey(id);
            var cachedProduct = await cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                var product = await context.Products
                    .Include(p => p.Propietario)
                    .Include(p => p.Ratings)
                    .FirstOrDefaultAsync(p => p.Id == id);
                return product;
            });

            if (cachedProduct is null)
            {
                logger.LogWarning("Producto con ID {Id} no encontrado", id);
                return Result.Failure<Models.Product, DomainError>(ProductError.NotFound(id));
            }

            return Result.Success<Models.Product, DomainError>(cachedProduct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo producto {ProductId}", id);
            return Result.Failure<Models.Product, DomainError>(ProductError.InvalidData($"Error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Obtiene todos los productos disponibles con caché.
    /// Devuelve: Result.Success(List) | Result.Failure nunca
    /// </summary>
    public async Task<Result<IEnumerable<Models.Product>, DomainError>> GetAllAsync()
    {
        try
        {
            var products = await cache.GetOrCreateAsync(ProductsCacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return await context.Products
                    .Include(p => p.Propietario)
                    .Include(p => p.Ratings)
                    .Where(p => !p.Deleted && p.CompraId == null)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            });

            return Result.Success<IEnumerable<Models.Product>, DomainError>(
                products ?? new List<Models.Product>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error obteniendo productos");
            return Result.Failure<IEnumerable<Models.Product>, DomainError>(
                ProductError.InvalidData($"Error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Busca productos por nombre y categoría con filtros opcionales.
    /// Devuelve: Result.Success(List) | Result.Failure nunca
    /// </summary>
    public async Task<Result<IEnumerable<Models.Product>, DomainError>> SearchAsync(string? nombre, string? categoria)
    {
        try
        {
            var query = context.Products
                .Include(p => p.Propietario)
                .Include(p => p.Ratings)
                .Where(p => !p.Deleted && p.CompraId == null);

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(p => p.Nombre.Contains(nombre) || p.Descripcion.Contains(nombre));

            if (!string.IsNullOrWhiteSpace(categoria) && Enum.TryParse<ProductCategory>(categoria, out var cat))
                query = query.Where(p => p.Categoria == cat);

            var products = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();

            return Result.Success<IEnumerable<Models.Product>, DomainError>(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error buscando productos");
            return Result.Failure<IEnumerable<Models.Product>, DomainError>(
                ProductError.InvalidData($"Error: {ex.Message}"));
        }
    }

    /// <summary>
    /// Crea un nuevo producto.
    /// Devuelve: Result.Success(Producto) | Result.Failure(InvalidPrice)
    /// </summary>
    public async Task<Result<Models.Product, DomainError>> CreateAsync(Models.Product product)
    {
        if (product.Precio <= 0)
            return Result.Failure<Models.Product, DomainError>(ProductError.InvalidPrice());

        return await Task.Run(() =>
        {
            context.Products.Add(product);
            context.SaveChanges();
            return Result.Success<Models.Product, DomainError>(product);
        })
        .Tap(p =>
        {
            cache.Remove(ProductsCacheKey);
            logger.LogInformation("Producto creado: {ProductId}", p.Id);
        });
    }

    /// <summary>
    /// Actualiza un producto existente (solo el propietario puede actualizar).
    /// Devuelve: Result.Success(Producto) | Result.Failure(NotFound/NotOwner)
    /// </summary>
    public async Task<Result<Models.Product, DomainError>> UpdateAsync(long id, Models.Product updatedProduct, long userId)
    {
        var product = await context.Products
            .Include(p => p.Propietario)
            .Include(p => p.Ratings)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return Result.Failure<Models.Product, DomainError>(ProductError.NotFound(id));

        if (product.PropietarioId != userId)
            return Result.Failure<Models.Product, DomainError>(ProductError.NotOwner(id));

        product.Nombre = updatedProduct.Nombre;
        product.Descripcion = updatedProduct.Descripcion;
        product.Precio = updatedProduct.Precio;
        product.Categoria = updatedProduct.Categoria;
        if (!string.IsNullOrEmpty(updatedProduct.Imagen)) product.Imagen = updatedProduct.Imagen;

        return await Task.Run(() =>
        {
            context.SaveChanges();
            return Result.Success<Models.Product, DomainError>(product);
        })
        .Tap(p =>
        {
            cache.Remove(ProductsCacheKey);
            cache.Remove(ProductDetailsCacheKey(id));
            logger.LogInformation("Producto actualizado: {ProductId}", id);
        });
    }

    /// <summary>
    /// Elimina un producto (soft-delete, solo propietario o admin).
    /// No permite eliminar productos ya vendidos.
    /// Devuelve: Result.Success(true) | Result.Failure(NotFound/NotOwner/CannotDeleteSold)
    /// </summary>
    public async Task<Result<bool, DomainError>> DeleteAsync(long id, long userId, bool isAdmin = false)
    {
        var producto = await context.Products
            .Include(p => p.Compra)
            .FirstOrDefaultAsync(p => p.Id == id && !p.Deleted);

        if (producto == null)
            return Result.Failure<bool, DomainError>(ProductError.NotFound(id));

        if (producto.CompraId.HasValue)
            return Result.Failure<bool, DomainError>(ProductError.CannotDeleteSold());

        if (!isAdmin && producto.PropietarioId != userId)
            return Result.Failure<bool, DomainError>(ProductError.NotOwner(id));

        producto.SoftDelete($"User-{userId}");

        return await Task.Run(() =>
        {
            context.SaveChanges();
            return Result.Success<bool, DomainError>(true);
        })
        .Tap(_ =>
        {
            cache.Remove(ProductsCacheKey);
            cache.Remove(ProductDetailsCacheKey(id));
            logger.LogInformation("Producto {ProductId} eliminado", id);
        });
    }

    private static string ProductDetailsCacheKey(long id) => $"product_details_{id}";
}
