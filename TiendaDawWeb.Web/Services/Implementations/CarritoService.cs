using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
///     Implementación del servicio de carrito
///     Sin cantidad - cada producto solo puede añadirse una vez (coincide con Spring Boot original)
/// </summary>
public class CarritoService(
    ApplicationDbContext context,
    IMemoryCache cache,
    ILogger<CarritoService> logger
) : ICarritoService {
    private const string ProductsCacheKey = "all_products";
    private static string ProductDetailsCacheKey(long id) => $"product_details_{id}";

    public async Task<Result<IEnumerable<CarritoItem>, DomainError>> GetCarritoByUsuarioIdAsync(long usuarioId) {
        try {
            var items = await context.CarritoItems
                .Include(c => c.Producto)
                .ThenInclude(p => p.Propietario)
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            return Result.Success<IEnumerable<CarritoItem>, DomainError>(items);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al obtener carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<IEnumerable<CarritoItem>, DomainError>(
                GenericError.DatabaseError("Error al obtener el carrito"));
        }
    }

    public async Task<Result<CarritoItem, DomainError>> AddToCarritoAsync(long usuarioId, long productoId) {
        try {
            // Verificar que el producto existe y está disponible
            var producto = await context.Products
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null || producto.Deleted)
                return Result.Failure<CarritoItem, DomainError>(ProductError.NotFound(productoId));

            // Verificar si está reservado (pero no por el usuario actual) o ya comprado
            if (producto.CompraId != null)
                return Result.Failure<CarritoItem, DomainError>(
                    CarritoError.ProductNotAvailableWithName(producto.Nombre));

            // Si está reservado, verificar si la reserva expiró o es del usuario actual
            // NOTE: En producción con SQL, considerar usar transacciones o locks para evitar race conditions
            // El modelo CarritoItem ya tiene RowVersion para control de concurrencia optimista
            if (producto.Reservado) {
                // Si está reservado por el usuario actual, permitir añadirlo (aunque ya debería estar en carrito)
                if (producto.ReservadoPor == usuarioId) {
                    // El usuario ya tiene este producto reservado, puede continuar
                }
                // Si no tiene fecha de reserva o la fecha es futura, está reservado por otro usuario
                else if (!producto.ReservadoHasta.HasValue || producto.ReservadoHasta.Value > DateTime.UtcNow) {
                    // Producto aún reservado por otro usuario
                    return Result.Failure<CarritoItem, DomainError>(
                        CarritoError.ProductNotAvailableWithName(producto.Nombre));
                }
                else {
                    // Reserva expiró, liberar el producto
                    producto.Reservado = false;
                    producto.ReservadoHasta = null;
                    producto.ReservadoPor = null;
                }
            }

            // Verificar si ya existe en el carrito - si existe, retornar error
            var existingItem = await context.CarritoItems
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.ProductoId == productoId);

            if (existingItem != null)
                return Result.Failure<CarritoItem, DomainError>(
                    CarritoError.ProductAlreadyInCartWithName(producto.Nombre));

            // Marcar producto como reservado (5 minutos) para este usuario
            producto.Reservado = true;
            producto.ReservadoHasta = DateTime.UtcNow.AddMinutes(5);
            producto.ReservadoPor = usuarioId;

            // Crear nuevo item sin cantidad
            var nuevoItem = new CarritoItem {
                UsuarioId = usuarioId,
                ProductoId = productoId,
                Precio = producto.Precio,
                CreatedAt = DateTime.UtcNow
            };

            context.CarritoItems.Add(nuevoItem);
            await context.SaveChangesAsync();

            // INVALIDACIÓN DE CACHÉ
            cache.Remove(ProductsCacheKey);
            cache.Remove(ProductDetailsCacheKey(productoId));

            // Recargar con navegación
            await context.Entry(nuevoItem)
                .Reference(c => c.Producto)
                .LoadAsync();

            logger.LogInformation(
                "Agregado producto {ProductoId} al carrito del usuario {UsuarioId} (reservado hasta {ReservadoHasta})",
                productoId, usuarioId, producto.ReservadoHasta);
            return Result.Success<CarritoItem, DomainError>(nuevoItem);
        }
        catch (DbUpdateConcurrencyException) {
            logger.LogWarning("Conflicto de concurrencia al agregar al carrito");
            return Result.Failure<CarritoItem, DomainError>(CarritoError.ConcurrencyConflict());
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al agregar producto {ProductoId} al carrito del usuario {UsuarioId}", productoId,
                usuarioId);
            return Result.Failure<CarritoItem, DomainError>(
                GenericError.DatabaseError("Error al agregar al carrito"));
        }
    }

    public async Task<Result<bool, DomainError>> RemoveFromCarritoAsync(long itemId) {
        try {
            var item = await context.CarritoItems
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(c => c.Id == itemId);

            if (item == null) return Result.Failure<bool, DomainError>(CarritoError.ItemNotFound(itemId));

            // Liberar la reserva del producto
            if (item.Producto != null) {
                item.Producto.Reservado = false;
                item.Producto.ReservadoHasta = null;
                item.Producto.ReservadoPor = null;
                logger.LogInformation("Liberada reserva del producto {ProductoId}", item.Producto.Id);
            }

            context.CarritoItems.Remove(item);
            await context.SaveChangesAsync();

            // INVALIDACIÓN DE CACHÉ
            cache.Remove(ProductsCacheKey);
            if (item.Producto != null) {
                cache.Remove(ProductDetailsCacheKey(item.Producto.Id));
            }

            logger.LogInformation("Eliminado item {ItemId} del carrito", itemId);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al eliminar item {ItemId} del carrito", itemId);
            return Result.Failure<bool, DomainError>(
                GenericError.DatabaseError("Error al eliminar del carrito"));
        }
    }

    public async Task<Result<bool, DomainError>> ClearCarritoAsync(long usuarioId) {
        try {
            var items = await context.CarritoItems
                .Include(c => c.Producto)
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            if (items.Count == 0) return Result.Success<bool, DomainError>(true);

            // Liberar todas las reservas
            foreach (var item in items)
                if (item.Producto != null) {
                    item.Producto.Reservado = false;
                    item.Producto.ReservadoHasta = null;
                    item.Producto.ReservadoPor = null;
                }

            context.CarritoItems.RemoveRange(items);
            await context.SaveChangesAsync();

            // INVALIDACIÓN DE CACHÉ
            cache.Remove(ProductsCacheKey);
            foreach (var item in items) {
                if (item.Producto != null) {
                    cache.Remove(ProductDetailsCacheKey(item.Producto.Id));
                }
            }

            logger.LogInformation("Vaciado carrito del usuario {UsuarioId} y liberadas {Count} reservas", usuarioId,
                items.Count);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al vaciar carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<bool, DomainError>(
                GenericError.DatabaseError("Error al vaciar el carrito"));
        }
    }

    public async Task<Result<decimal, DomainError>> GetTotalCarritoAsync(long usuarioId) {
        try {
            var total = await context.CarritoItems
                .Where(c => c.UsuarioId == usuarioId)
                .SumAsync(c => c.Precio);

            return Result.Success<decimal, DomainError>(total);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al calcular total del carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<decimal, DomainError>(
                GenericError.DatabaseError("Error al calcular total"));
        }
    }

    public async Task<Result<int, DomainError>> GetCarritoCountAsync(long usuarioId) {
        try {
            var count = await context.CarritoItems
                .Where(c => c.UsuarioId == usuarioId)
                .CountAsync();

            return Result.Success<int, DomainError>(count);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al contar items del carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<int, DomainError>(
                GenericError.DatabaseError("Error al contar items"));
        }
    }
}