using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Carrito;

namespace TiendaDawWeb.Services.Carrito;

/// <summary>
///     Implementación del servicio de carrito
///     Sin cantidad - cada producto solo puede añadirse una vez (coincide con Spring Boot original)
/// </summary>
public class CarritoService(
    ApplicationDbContext context,
    IMemoryCache cache,
    ILogger<CarritoService> logger
) : ICarritoService
{
    private const string ProductsCacheKey = "all_products";
    private static string ProductDetailsCacheKey(long id) => $"product_details_{id}";

    public async Task<Result<IEnumerable<CarritoItem>, DomainError>> GetCarritoByUsuarioIdAsync(long usuarioId)
    {
        try
        {
            var items = await context.CarritoItems
                .Include(c => c.Producto)
                .ThenInclude(p => p.Propietario)
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            return Result.Success<IEnumerable<CarritoItem>, DomainError>(items);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<IEnumerable<CarritoItem>, DomainError>(
                GenericError.DatabaseError("Error al obtener el carrito"));
        }
    }

    public async Task<Result<CarritoItem, DomainError>> AddToCarritoAsync(long usuarioId, long productoId)
    {
        try
        {
            var producto = await context.Products.FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null || producto.Deleted)
                return Result.Failure<CarritoItem, DomainError>(ProductError.NotFound(productoId));

            if (producto.CompraId != null)
                return Result.Failure<CarritoItem, DomainError>(
                    CarritoError.ProductNotAvailableWithName(producto.Nombre));

            if (producto.Reservado)
            {
                if (producto.ReservadoPor == usuarioId)
                {
                }
                else if (!producto.ReservadoHasta.HasValue || producto.ReservadoHasta.Value > DateTime.UtcNow)
                {
                    return Result.Failure<CarritoItem, DomainError>(
                        CarritoError.ProductNotAvailableWithName(producto.Nombre));
                }
                else
                {
                    producto.Reservado = false;
                    producto.ReservadoHasta = null;
                    producto.ReservadoPor = null;
                }
            }

            var existingItem = await context.CarritoItems
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.ProductoId == productoId);

            if (existingItem != null)
                return Result.Failure<CarritoItem, DomainError>(
                    CarritoError.ProductAlreadyInCartWithName(producto.Nombre));

            producto.Reservado = true;
            producto.ReservadoHasta = DateTime.UtcNow.AddMinutes(5);
            producto.ReservadoPor = usuarioId;

            var nuevoItem = new CarritoItem
            {
                UsuarioId = usuarioId,
                ProductoId = productoId,
                Precio = producto.Precio,
                CreatedAt = DateTime.UtcNow
            };

            return await Task.Run(() =>
            {
                context.CarritoItems.Add(nuevoItem);
                context.SaveChanges();
                return Result.Success<CarritoItem, DomainError>(nuevoItem);
            })
            .Tap(async item =>
            {
                cache.Remove(ProductsCacheKey);
                cache.Remove(ProductDetailsCacheKey(productoId));
                await context.Entry(item).Reference(c => c.Producto).LoadAsync();
            })
            .Tap(item =>
            {
                logger.LogInformation(
                    "Agregado producto {ProductoId} al carrito del usuario {UsuarioId} (reservado hasta {ReservadoHasta})",
                    productoId, usuarioId, producto.ReservadoHasta);
            });
        }
        catch (DbUpdateConcurrencyException)
        {
            logger.LogWarning("Conflicto de concurrencia al agregar al carrito");
            return Result.Failure<CarritoItem, DomainError>(CarritoError.ConcurrencyConflict());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al agregar producto {ProductoId} al carrito del usuario {UsuarioId}", productoId,
                usuarioId);
            return Result.Failure<CarritoItem, DomainError>(
                GenericError.DatabaseError("Error al agregar al carrito"));
        }
    }

    public async Task<Result<bool, DomainError>> RemoveFromCarritoAsync(long itemId)
    {
        try
        {
            var item = await context.CarritoItems
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(c => c.Id == itemId);

            if (item == null)
                return Result.Failure<bool, DomainError>(CarritoError.ItemNotFound(itemId));

            return await Task.Run(() =>
            {
                if (item.Producto != null)
                {
                    item.Producto.Reservado = false;
                    item.Producto.ReservadoHasta = null;
                    item.Producto.ReservadoPor = null;
                    logger.LogInformation("Liberada reserva del producto {ProductoId}", item.Producto.Id);
                }

                context.CarritoItems.Remove(item);
                context.SaveChanges();
                return Result.Success<bool, DomainError>(true);
            })
            .Tap(_ =>
            {
                cache.Remove(ProductsCacheKey);
                if (item.Producto != null)
                    cache.Remove(ProductDetailsCacheKey(item.Producto.Id));
            })
            .Tap(_ => logger.LogInformation("Eliminado item {ItemId} del carrito", itemId));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al eliminar item {ItemId} del carrito", itemId);
            return Result.Failure<bool, DomainError>(
                GenericError.DatabaseError("Error al eliminar del carrito"));
        }
    }

    public async Task<Result<bool, DomainError>> ClearCarritoAsync(long usuarioId)
    {
        try
        {
            var items = await context.CarritoItems
                .Include(c => c.Producto)
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            if (items.Count == 0)
                return Result.Success<bool, DomainError>(true);

            return await Task.Run(() =>
            {
                foreach (var item in items)
                    if (item.Producto != null)
                    {
                        item.Producto.Reservado = false;
                        item.Producto.ReservadoHasta = null;
                        item.Producto.ReservadoPor = null;
                    }

                context.CarritoItems.RemoveRange(items);
                context.SaveChanges();
                return Result.Success<bool, DomainError>(true);
            })
            .Tap(_ =>
            {
                cache.Remove(ProductsCacheKey);
                foreach (var item in items)
                    if (item.Producto != null)
                        cache.Remove(ProductDetailsCacheKey(item.Producto.Id));
            })
            .Tap(_ =>
                logger.LogInformation("Vaciado carrito del usuario {UsuarioId} y liberadas {Count} reservas", usuarioId,
                    items.Count));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al vaciar carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<bool, DomainError>(
                GenericError.DatabaseError("Error al vaciar el carrito"));
        }
    }

    public async Task<Result<decimal, DomainError>> GetTotalCarritoAsync(long usuarioId)
    {
        try
        {
            var total = await context.CarritoItems
                .Where(c => c.UsuarioId == usuarioId)
                .SumAsync(c => c.Precio);

            return Result.Success<decimal, DomainError>(total);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al calcular total del carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<decimal, DomainError>(
                GenericError.DatabaseError("Error al calcular total"));
        }
    }

    public async Task<Result<int, DomainError>> GetCarritoCountAsync(long usuarioId)
    {
        try
        {
            var count = await context.CarritoItems
                .Where(c => c.UsuarioId == usuarioId)
                .CountAsync();

            return Result.Success<int, DomainError>(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al contar items del carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<int, DomainError>(
                GenericError.DatabaseError("Error al contar items"));
        }
    }
}
