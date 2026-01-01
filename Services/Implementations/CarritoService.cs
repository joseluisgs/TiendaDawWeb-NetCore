using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
/// Implementación del servicio de carrito
/// Sin cantidad - cada producto solo puede añadirse una vez (coincide con Spring Boot original)
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

    public async Task<Result<CarritoItem, DomainError>> AddToCarritoAsync(long usuarioId, long productoId)
    {
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

            // Verificar si ya existe en el carrito - si existe, retornar error
            var existingItem = await _context.CarritoItems
                .FirstOrDefaultAsync(c => c.UsuarioId == usuarioId && c.ProductoId == productoId);

            if (existingItem != null)
            {
                return Result.Failure<CarritoItem, DomainError>(
                    CarritoError.ProductAlreadyInCart(productoId));
            }

            // Crear nuevo item sin cantidad
            var nuevoItem = new CarritoItem
            {
                UsuarioId = usuarioId,
                ProductoId = productoId,
                Precio = producto.Precio,
                CreatedAt = DateTime.UtcNow
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
                .SumAsync(c => c.Precio);

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
                .CountAsync();

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
