using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
///     Servicio de gestión de productos con Railway Oriented Programming y Caché de Aplicación.
/// </summary>
public class ProductService(
    ApplicationDbContext context,
    IMemoryCache cache,
    ILogger<ProductService> logger
) : IProductService {
    private const string ProductsCacheKey = "all_products";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<Result<Product, DomainError>> GetByIdAsync(long id) {
        try {
            var productResult = await cache.GetOrCreateAsync(ProductDetailsCacheKey(id), async entry => {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                logger.LogDebug("Cache Miss: Obteniendo producto {ProductId} de la DB", id);

                var product = await context.Products
                    .Include(p => p.Propietario)
                    .Include(p => p.Ratings)
                    .FirstOrDefaultAsync(p => p.Id == id);

                return product != null
                    ? Result.Success<Product, DomainError>(product)
                    : Result.Failure<Product, DomainError>(ProductError.NotFound(id));
            });
            // productResult ya es un Result<Product, DomainError>, no puede ser null.
            return productResult;
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error obteniendo producto {ProductId}", id);
            return Result.Failure<Product, DomainError>(
                ProductError.InvalidData($"Error al obtener producto: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<Product>, DomainError>> GetAllAsync() {
        try {
            var products = await cache.GetOrCreateAsync(ProductsCacheKey, async entry => {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                logger.LogDebug("Cache Miss: Obteniendo todos los productos de la DB");

                return await context.Products
                    .Include(p => p.Propietario)
                    .Include(p => p.Ratings)
                    .Where(p => !p.Deleted && p.CompraId == null)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            });

            return Result.Success<IEnumerable<Product>, DomainError>(products ?? Enumerable.Empty<Product>());
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error obteniendo todos los productos");
            return Result.Failure<IEnumerable<Product>, DomainError>(
                ProductError.InvalidData($"Error al obtener productos: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<Product>, DomainError>> SearchAsync(string? nombre, string? categoria) {
        // La búsqueda es demasiado dinámica para cachearla fácilmente sin complicar la invalidación.
        // Se deja contra DB o se podría cachear con claves compuestas.
        try {
            var query = context.Products
                .Include(p => p.Propietario)
                .Include(p => p.Ratings)
                .Where(p => !p.Deleted && p.CompraId == null);

            if (!string.IsNullOrWhiteSpace(nombre))
                query = query.Where(p => p.Nombre.Contains(nombre) || p.Descripcion.Contains(nombre));

            if (!string.IsNullOrWhiteSpace(categoria) && Enum.TryParse<ProductCategory>(categoria, out var cat))
                query = query.Where(p => p.Categoria == cat);

            var products = await query.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return Result.Success<IEnumerable<Product>, DomainError>(products);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error buscando productos");
            return Result.Failure<IEnumerable<Product>, DomainError>(
                ProductError.InvalidData($"Error al buscar productos: {ex.Message}"));
        }
    }

    public async Task<Result<Product, DomainError>> CreateAsync(Product product) {
        try {
            if (product.Precio <= 0)
                return Result.Failure<Product, DomainError>(ProductError.InvalidPrice);

            context.Products.Add(product);
            await context.SaveChangesAsync();

            // INVALIDACIÓN: Un nuevo producto afecta a la lista general
            cache.Remove(ProductsCacheKey);

            logger.LogInformation("Producto creado: {ProductId} - {ProductName}", product.Id, product.Nombre);
            return Result.Success<Product, DomainError>(product);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error creando producto");
            return Result.Failure<Product, DomainError>(
                ProductError.InvalidData($"Error al crear producto: {ex.Message}"));
        }
    }

    public async Task<Result<Product, DomainError>> UpdateAsync(long id, Product updatedProduct, long userId) {
        try {
            // 🚨 CRÍTICO: Leemos directamente de DB para que la entidad esté trackeada por este DbContext
            // NO usar GetByIdAsync porque devuelve entidades de caché trackeadas por otro contexto
            var product = await context.Products
                .Include(p => p.Propietario)
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return Result.Failure<Product, DomainError>(ProductError.NotFound(id));

            if (product.PropietarioId != userId)
                return Result.Failure<Product, DomainError>(ProductError.NotOwner);

            product.Nombre = updatedProduct.Nombre;
            product.Descripcion = updatedProduct.Descripcion;
            product.Precio = updatedProduct.Precio;
            product.Categoria = updatedProduct.Categoria;

            if (!string.IsNullOrEmpty(updatedProduct.Imagen))
                product.Imagen = updatedProduct.Imagen;

            // INVALIDACIÓN: Limpiar caché antes de guardar
            cache.Remove(ProductsCacheKey);
            cache.Remove(ProductDetailsCacheKey(id));

            await context.SaveChangesAsync();

            logger.LogInformation("Producto actualizado: {ProductId}", id);
            return Result.Success<Product, DomainError>(product);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error actualizando producto {ProductId}", id);
            return Result.Failure<Product, DomainError>(
                ProductError.InvalidData($"Error al actualizar producto: {ex.Message}"));
        }
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(long id, long userId, bool isAdmin = false) {
        try {
            var producto = await context.Products
                .Include(p => p.Compra)
                .FirstOrDefaultAsync(p => p.Id == id && !p.Deleted);

            if (producto == null) return Result.Failure<bool, DomainError>(ProductError.NotFound(id));

            if (producto.CompraId.HasValue) {
                logger.LogWarning("❌ Intento de eliminar producto vendido {ProductId}", id);
                return Result.Failure<bool, DomainError>(ProductError.CannotDeleteSold);
            }

            if (!isAdmin && producto.PropietarioId != userId)
                return Result.Failure<bool, DomainError>(ProductError.NotOwner);

            producto.SoftDelete($"User-{userId}");
            await context.SaveChangesAsync();

            // INVALIDACIÓN: Borrado físico/lógico afecta a todo
            cache.Remove(ProductsCacheKey);
            cache.Remove(ProductDetailsCacheKey(id));

            logger.LogInformation("✅ Producto {ProductId} eliminado por usuario {UserId}", id, userId);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error eliminando producto {ProductId}", id);
            return Result.Failure<bool, DomainError>(
                ProductError.InvalidData($"Error al eliminar producto: {ex.Message}"));
        }
    }

    private static string ProductDetailsCacheKey(long id) {
        return $"product_details_{id}";
    }
}