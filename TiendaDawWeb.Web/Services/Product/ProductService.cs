using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Product;

namespace TiendaDawWeb.Services.Product;

public class ProductService(
    ApplicationDbContext context,
    IMemoryCache cache,
    ILogger<ProductService> logger
) : IProductService {
    private const string ProductsCacheKey = "all_products";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

    public async Task<Result<Models.Product, DomainError>> GetByIdAsync(long id) {
        try {
            var productResult = await cache.GetOrCreateAsync(ProductDetailsCacheKey(id), async entry => {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                var product = await context.Products
                    .Include(p => p.Propietario)
                    .Include(p => p.Ratings)
                    .FirstOrDefaultAsync(p => p.Id == id);
                return product != null
                    ? Result.Success<Models.Product, DomainError>(product)
                    : Result.Failure<Models.Product, DomainError>(ProductError.NotFound(id));
            });
            return productResult!;
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error obteniendo producto {ProductId}", id);
            return Result.Failure<Models.Product, DomainError>(ProductError.InvalidData($"Error: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<Models.Product>, DomainError>> GetAllAsync() {
        try {
            var products = await cache.GetOrCreateAsync(ProductsCacheKey, async entry => {
                entry.AbsoluteExpirationRelativeToNow = CacheDuration;
                return await context.Products
                    .Include(p => p.Propietario)
                    .Include(p => p.Ratings)
                    .Where(p => !p.Deleted && p.CompraId == null)
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();
            });
            return Result.Success<IEnumerable<Models.Product>, DomainError>(products ?? new List<Models.Product>());
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error obteniendo productos");
            return Result.Failure<IEnumerable<Models.Product>, DomainError>(ProductError.InvalidData($"Error: {ex.Message}"));
        }
    }

    public async Task<Result<IEnumerable<Models.Product>, DomainError>> SearchAsync(string? nombre, string? categoria) {
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
            return Result.Success<IEnumerable<Models.Product>, DomainError>(products);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error buscando productos");
            return Result.Failure<IEnumerable<Models.Product>, DomainError>(ProductError.InvalidData($"Error: {ex.Message}"));
        }
    }

    public async Task<Result<Models.Product, DomainError>> CreateAsync(Models.Product product) {
        try {
            if (product.Precio <= 0) return Result.Failure<Models.Product, DomainError>(ProductError.InvalidPrice());
            context.Products.Add(product);
            await context.SaveChangesAsync();
            cache.Remove(ProductsCacheKey);
            logger.LogInformation("Producto creado: {ProductId}", product.Id);
            return Result.Success<Models.Product, DomainError>(product);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error creando producto");
            return Result.Failure<Models.Product, DomainError>(ProductError.InvalidData($"Error: {ex.Message}"));
        }
    }

    public async Task<Result<Models.Product, DomainError>> UpdateAsync(long id, Models.Product updatedProduct, long userId) {
        try {
            var product = await context.Products
                .Include(p => p.Propietario)
                .Include(p => p.Ratings)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return Result.Failure<Models.Product, DomainError>(ProductError.NotFound(id));
            if (product.PropietarioId != userId) return Result.Failure<Models.Product, DomainError>(ProductError.NotOwner(id));
            product.Nombre = updatedProduct.Nombre;
            product.Descripcion = updatedProduct.Descripcion;
            product.Precio = updatedProduct.Precio;
            product.Categoria = updatedProduct.Categoria;
            if (!string.IsNullOrEmpty(updatedProduct.Imagen)) product.Imagen = updatedProduct.Imagen;
            cache.Remove(ProductsCacheKey);
            cache.Remove(ProductDetailsCacheKey(id));
            await context.SaveChangesAsync();
            logger.LogInformation("Producto actualizado: {ProductId}", id);
            return Result.Success<Models.Product, DomainError>(product);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error actualizando producto");
            return Result.Failure<Models.Product, DomainError>(ProductError.InvalidData($"Error: {ex.Message}"));
        }
    }

    public async Task<Result<bool, DomainError>> DeleteAsync(long id, long userId, bool isAdmin = false) {
        try {
            var producto = await context.Products.Include(p => p.Compra).FirstOrDefaultAsync(p => p.Id == id && !p.Deleted);
            if (producto == null) return Result.Failure<bool, DomainError>(ProductError.NotFound(id));
            if (producto.CompraId.HasValue) return Result.Failure<bool, DomainError>(ProductError.CannotDeleteSold());
            if (!isAdmin && producto.PropietarioId != userId) return Result.Failure<bool, DomainError>(ProductError.NotOwner(id));
            producto.SoftDelete($"User-{userId}");
            await context.SaveChangesAsync();
            cache.Remove(ProductsCacheKey);
            cache.Remove(ProductDetailsCacheKey(id));
            logger.LogInformation("Producto {ProductId} eliminado", id);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error eliminando producto");
            return Result.Failure<bool, DomainError>(ProductError.InvalidData($"Error: {ex.Message}"));
        }
    }

    private static string ProductDetailsCacheKey(long id) => $"product_details_{id}";
}
