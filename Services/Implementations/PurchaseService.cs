using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;
using System.Data;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
/// Implementación del servicio de compras con control de concurrencia
/// </summary>
public class PurchaseService : IPurchaseService
{
    private readonly ApplicationDbContext _context;
    private readonly ICarritoService _carritoService;
    private readonly IPdfService _pdfService;
    private readonly ILogger<PurchaseService> _logger;

    public PurchaseService(
        ApplicationDbContext context,
        ICarritoService carritoService,
        IPdfService pdfService,
        ILogger<PurchaseService> logger)
    {
        _context = context;
        _carritoService = carritoService;
        _pdfService = pdfService;
        _logger = logger;
    }

    /// <summary>
    /// Crea una compra a partir del carrito con control de concurrencia SERIALIZABLE
    /// </summary>
    public async Task<Result<Purchase, DomainError>> CreatePurchaseFromCarritoAsync(long usuarioId)
    {
        // Usar estrategia de ejecución para reintentar en caso de deadlocks
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                // 1. Obtener items del carrito
                var carritoResult = await _carritoService.GetCarritoByUsuarioIdAsync(usuarioId);
                if (carritoResult.IsFailure)
                {
                    return Result.Failure<Purchase, DomainError>(carritoResult.Error);
                }

                var carritoItems = carritoResult.Value.ToList();
                if (!carritoItems.Any())
                {
                    return Result.Failure<Purchase, DomainError>(PurchaseError.EmptyCarrito);
                }

                // 2. Validar disponibilidad de todos los productos DENTRO de la transacción
                var productIds = carritoItems.Select(ci => ci.ProductoId).ToList();
                var productos = await _context.Products
                    .Where(p => productIds.Contains(p.Id) && !p.Deleted)
                    .ToListAsync();

                // Verificar que todos los productos existen y están disponibles
                foreach (var item in carritoItems)
                {
                    var producto = productos.FirstOrDefault(p => p.Id == item.ProductoId);
                    if (producto == null)
                    {
                        await transaction.RollbackAsync();
                        return Result.Failure<Purchase, DomainError>(
                            ProductError.NotFound(item.ProductoId));
                    }

                    // Verificar que no esté ya vendido
                    if (producto.CompraId != null)
                    {
                        await transaction.RollbackAsync();
                        return Result.Failure<Purchase, DomainError>(
                            PurchaseError.ProductNotAvailable(producto.Nombre));
                    }

                    // Verificar que no esté reservado por otro usuario
                    if (producto.Reservado && producto.ReservadoHasta > DateTime.UtcNow)
                    {
                        await transaction.RollbackAsync();
                        return Result.Failure<Purchase, DomainError>(
                            PurchaseError.ProductNotAvailable(producto.Nombre));
                    }
                }

                // 3. Calcular total
                decimal total = carritoItems.Sum(ci => ci.Subtotal);

                // 4. Crear la compra
                var purchase = new Purchase
                {
                    CompradorId = usuarioId,
                    FechaCompra = DateTime.UtcNow,
                    Total = total
                };

                _context.Purchases.Add(purchase);
                await _context.SaveChangesAsync();

                // 5. Asignar productos a la compra (marcar como vendidos)
                foreach (var producto in productos)
                {
                    producto.CompraId = purchase.Id;
                    producto.Reservado = false;
                    producto.ReservadoHasta = null;
                }

                await _context.SaveChangesAsync();

                // 6. Vaciar el carrito
                var clearResult = await _carritoService.ClearCarritoAsync(usuarioId);
                if (clearResult.IsFailure)
                {
                    _logger.LogWarning("Error al vaciar carrito después de compra: {Error}", 
                        clearResult.Error.Message);
                }

                // 7. Commit de la transacción
                await transaction.CommitAsync();

                _logger.LogInformation("Compra {PurchaseId} creada exitosamente para usuario {UserId}", 
                    purchase.Id, usuarioId);

                // 8. Cargar la compra con sus relaciones
                var purchaseWithDetails = await _context.Purchases
                    .Include(p => p.Comprador)
                    .Include(p => p.Products)
                        .ThenInclude(prod => prod.Propietario)
                    .FirstOrDefaultAsync(p => p.Id == purchase.Id);

                return Result.Success<Purchase, DomainError>(purchaseWithDetails!);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error de concurrencia al crear compra para usuario {UserId}", usuarioId);
                return Result.Failure<Purchase, DomainError>(
                    GenericError.ConcurrencyError("Otro usuario ha modificado los datos. Por favor, intenta de nuevo."));
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error al crear compra para usuario {UserId}", usuarioId);
                return Result.Failure<Purchase, DomainError>(
                    GenericError.DatabaseError($"Error al procesar la compra: {ex.Message}"));
            }
        });
    }

    public async Task<Result<Purchase, DomainError>> GetByIdAsync(long id)
    {
        try
        {
            var purchase = await _context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                    .ThenInclude(prod => prod.Propietario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
            {
                return Result.Failure<Purchase, DomainError>(PurchaseError.NotFound(id));
            }

            return Result.Success<Purchase, DomainError>(purchase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compra {PurchaseId}", id);
            return Result.Failure<Purchase, DomainError>(
                GenericError.DatabaseError("Error al obtener la compra"));
        }
    }

    public async Task<Result<IEnumerable<Purchase>, DomainError>> GetByUserAsync(long usuarioId, int page = 1, int pageSize = 10)
    {
        try
        {
            var purchases = await _context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                .Where(p => p.CompradorId == usuarioId)
                .OrderByDescending(p => p.FechaCompra)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result.Success<IEnumerable<Purchase>, DomainError>(purchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras del usuario {UserId}", usuarioId);
            return Result.Failure<IEnumerable<Purchase>, DomainError>(
                GenericError.DatabaseError("Error al obtener las compras"));
        }
    }

    public async Task<Result<IEnumerable<Purchase>, DomainError>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var purchases = await _context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                .OrderByDescending(p => p.FechaCompra)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result.Success<IEnumerable<Purchase>, DomainError>(purchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener todas las compras");
            return Result.Failure<IEnumerable<Purchase>, DomainError>(
                GenericError.DatabaseError("Error al obtener las compras"));
        }
    }

    public async Task<Result<IEnumerable<Purchase>, DomainError>> GetByDateRangeAsync(
        DateTime desde, DateTime hasta, int page = 1, int pageSize = 10)
    {
        try
        {
            var purchases = await _context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                .Where(p => p.FechaCompra >= desde && p.FechaCompra <= hasta)
                .OrderByDescending(p => p.FechaCompra)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result.Success<IEnumerable<Purchase>, DomainError>(purchases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener compras por rango de fechas");
            return Result.Failure<IEnumerable<Purchase>, DomainError>(
                GenericError.DatabaseError("Error al obtener las compras"));
        }
    }

    public async Task<Result<byte[], DomainError>> GeneratePdfAsync(long purchaseId)
    {
        try
        {
            var purchaseResult = await GetByIdAsync(purchaseId);
            if (purchaseResult.IsFailure)
            {
                return Result.Failure<byte[], DomainError>(purchaseResult.Error);
            }

            var purchase = purchaseResult.Value;
            
            // Generar PDF usando el servicio de PDF
            return await _pdfService.GenerateInvoicePdfAsync(purchase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF para compra {PurchaseId}", purchaseId);
            return Result.Failure<byte[], DomainError>(
                PurchaseError.PdfGenerationFailed(ex.Message));
        }
    }
}
