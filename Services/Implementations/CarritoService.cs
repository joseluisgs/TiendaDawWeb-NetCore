using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
/// Implementación del servicio de carrito con control de concurrencia optimista
/// </summary>
public class CarritoService : ICarritoService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CarritoService> _logger;

    public CarritoService(ApplicationDbContext context, ILogger<CarritoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<CarritoItem>, DomainError>> GetCarritoByUsuarioIdAsync(long usuarioId)
    {
        try
        {
            var items = await _context.CarritoItems
                .Include(c => c.Producto)
                .ThenInclude(p => p.Propietario)
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            return Result.Success<IEnumerable<CarritoItem>, DomainError>(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<IEnumerable<CarritoItem>, DomainError>(
                GenericError.DatabaseError("Error al obtener el carrito"));
        }
    }

    public async Task<Result<CarritoItem, DomainError>> AddToCarritoAsync(long usuarioId, long productoId, int cantidad = 1)
    {
        if (cantidad <= 0)
        {
            return Result.Failure<CarritoItem, DomainError>(CarritoError.InvalidQuantity(cantidad));
        }

        try
        {
            // Verificar que el producto existe y está disponible
            var producto = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == productoId);

            if (producto == null || producto.Deleted)
            {
                return Result.Failure<CarritoItem, DomainError>(ProductError.NotFound(productoId));
            }

            if (producto.Reservado || producto.CompraId != null)
            {
                return Result.Failure<CarritoItem, DomainError>(CarritoError.ProductNotAvailable(productoId));
            }

            // Verificar si ya existe en el carrito
            var existingItem = await _context.CarritoItems
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.ProductoId == productoId);

            if (existingItem != null)
            {
                // Actualizar cantidad existente con control de concurrencia
                existingItem.Cantidad += cantidad;
                existingItem.Subtotal = existingItem.Cantidad * producto.Precio;
                existingItem.UpdatedAt = DateTime.UtcNow;

                try
                {
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Actualizado item {ItemId} en carrito del usuario {UsuarioId}", existingItem.Id, usuarioId);
                    return Result.Success<CarritoItem, DomainError>(existingItem);
                }
                catch (DbUpdateConcurrencyException)
                {
                    _logger.LogWarning("Conflicto de concurrencia al actualizar item del carrito");
                    return Result.Failure<CarritoItem, DomainError>(CarritoError.ConcurrencyConflict());
                }
            }
            else
            {
                // Crear nuevo item
                var nuevoItem = new CarritoItem
                {
                    UsuarioId = usuarioId,
                    ProductoId = productoId,
                    Cantidad = cantidad,
                    Subtotal = cantidad * producto.Precio,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.CarritoItems.Add(nuevoItem);
                await _context.SaveChangesAsync();

                // Recargar con navegación
                await _context.Entry(nuevoItem)
                    .Reference(c => c.Producto)
                    .LoadAsync();

                _logger.LogInformation("Agregado producto {ProductoId} al carrito del usuario {UsuarioId}", productoId, usuarioId);
                return Result.Success<CarritoItem, DomainError>(nuevoItem);
            }
        }
        catch (DbUpdateConcurrencyException)
        {
            _logger.LogWarning("Conflicto de concurrencia al agregar al carrito");
            return Result.Failure<CarritoItem, DomainError>(CarritoError.ConcurrencyConflict());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al agregar producto {ProductoId} al carrito del usuario {UsuarioId}", productoId, usuarioId);
            return Result.Failure<CarritoItem, DomainError>(
                GenericError.DatabaseError("Error al agregar al carrito"));
        }
    }

    public async Task<Result<CarritoItem, DomainError>> UpdateCantidadAsync(long itemId, int nuevaCantidad)
    {
        if (nuevaCantidad <= 0)
        {
            return Result.Failure<CarritoItem, DomainError>(CarritoError.InvalidQuantity(nuevaCantidad));
        }

        try
        {
            var item = await _context.CarritoItems
                .Include(c => c.Producto)
                .FirstOrDefaultAsync(c => c.Id == itemId);

            if (item == null)
            {
                return Result.Failure<CarritoItem, DomainError>(CarritoError.ItemNotFound(itemId));
            }

            // Verificar disponibilidad del producto
            if (item.Producto.Reservado || item.Producto.CompraId != null)
            {
                return Result.Failure<CarritoItem, DomainError>(CarritoError.ProductNotAvailable(item.ProductoId));
            }

            item.Cantidad = nuevaCantidad;
            item.Subtotal = nuevaCantidad * item.Producto.Precio;
            item.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
                _logger.LogInformation("Actualizada cantidad del item {ItemId} a {Cantidad}", itemId, nuevaCantidad);
                return Result.Success<CarritoItem, DomainError>(item);
            }
            catch (DbUpdateConcurrencyException)
            {
                _logger.LogWarning("Conflicto de concurrencia al actualizar cantidad del item {ItemId}", itemId);
                return Result.Failure<CarritoItem, DomainError>(CarritoError.ConcurrencyConflict());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar cantidad del item {ItemId}", itemId);
            return Result.Failure<CarritoItem, DomainError>(
                GenericError.DatabaseError("Error al actualizar cantidad"));
        }
    }

    public async Task<Result<bool, DomainError>> RemoveFromCarritoAsync(long itemId)
    {
        try
        {
            var item = await _context.CarritoItems.FindAsync(itemId);

            if (item == null)
            {
                return Result.Failure<bool, DomainError>(CarritoError.ItemNotFound(itemId));
            }

            _context.CarritoItems.Remove(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Eliminado item {ItemId} del carrito", itemId);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar item {ItemId} del carrito", itemId);
            return Result.Failure<bool, DomainError>(
                GenericError.DatabaseError("Error al eliminar del carrito"));
        }
    }

    public async Task<Result<bool, DomainError>> ClearCarritoAsync(long usuarioId)
    {
        try
        {
            var items = await _context.CarritoItems
                .Where(c => c.UsuarioId == usuarioId)
                .ToListAsync();

            if (items.Count == 0)
            {
                return Result.Success<bool, DomainError>(true);
            }

            _context.CarritoItems.RemoveRange(items);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vaciado carrito del usuario {UsuarioId}", usuarioId);
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al vaciar carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<bool, DomainError>(
                GenericError.DatabaseError("Error al vaciar el carrito"));
        }
    }

    public async Task<Result<decimal, DomainError>> GetTotalCarritoAsync(long usuarioId)
    {
        try
        {
            var total = await _context.CarritoItems
                .Where(c => c.UsuarioId == usuarioId)
                .SumAsync(c => c.Subtotal);

            return Result.Success<decimal, DomainError>(total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al calcular total del carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<decimal, DomainError>(
                GenericError.DatabaseError("Error al calcular total"));
        }
    }

    public async Task<Result<int, DomainError>> GetCarritoCountAsync(long usuarioId)
    {
        try
        {
            var count = await _context.CarritoItems
                .Where(c => c.UsuarioId == usuarioId)
                .SumAsync(c => c.Cantidad);

            return Result.Success<int, DomainError>(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al contar items del carrito del usuario {UsuarioId}", usuarioId);
            return Result.Failure<int, DomainError>(
                GenericError.DatabaseError("Error al contar items"));
        }
    }
}
