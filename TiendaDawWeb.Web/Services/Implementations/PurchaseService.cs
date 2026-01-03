using System.Data;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
///     Implementación del servicio de compras con control de concurrencia
/// </summary>
public class PurchaseService(
    ApplicationDbContext context,
    ICarritoService carritoService,
    IPdfService pdfService,
    IMemoryCache cache,
    ILogger<PurchaseService> logger
) : IPurchaseService {
    private const string ProductsCacheKey = "all_products";
    /// <summary>
    ///     Crea una compra a partir del carrito con control de concurrencia SERIALIZABLE
    /// </summary>
    public async Task<Result<Purchase, DomainError>> CreatePurchaseFromCarritoAsync(long usuarioId) {
        // Usar estrategia de ejecución para reintentar en caso de deadlocks
        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () => {
            using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try {
                // 1. Obtener items del carrito
                var carritoResult = await carritoService.GetCarritoByUsuarioIdAsync(usuarioId);
                if (carritoResult.IsFailure) return Result.Failure<Purchase, DomainError>(carritoResult.Error);

                var carritoItems = carritoResult.Value.ToList();
                if (!carritoItems.Any()) return Result.Failure<Purchase, DomainError>(PurchaseError.EmptyCarrito);

                // 2. Validar disponibilidad de todos los productos DENTRO de la transacción
                var productIds = carritoItems.Select(ci => ci.ProductoId).ToList();
                var productos = await context.Products
                    .Where(p => productIds.Contains(p.Id) && !p.Deleted)
                    .ToListAsync();

                // Verificar que todos los productos existen y están disponibles
                foreach (var item in carritoItems) {
                    var producto = productos.FirstOrDefault(p => p.Id == item.ProductoId);
                    if (producto == null) {
                        await transaction.RollbackAsync();
                        return Result.Failure<Purchase, DomainError>(
                            ProductError.NotFound(item.ProductoId));
                    }

                    // Verificar que no esté ya vendido
                    if (producto.CompraId != null) {
                        await transaction.RollbackAsync();
                        return Result.Failure<Purchase, DomainError>(
                            PurchaseError.ProductNotAvailable(producto.Nombre));
                    }

                    // Verificar que no esté reservado por otro usuario
                    // Si está reservado por el usuario actual, permitir la compra
                    if (producto.Reservado && producto.ReservadoPor != usuarioId &&
                        producto.ReservadoHasta > DateTime.UtcNow) {
                        await transaction.RollbackAsync();
                        return Result.Failure<Purchase, DomainError>(
                            PurchaseError.ProductNotAvailable(producto.Nombre));
                    }
                }

                // 3. Calcular total - ahora suma el precio de cada item
                var total = carritoItems.Sum(ci => ci.Precio);

                // 4. Crear la compra
                var purchase = new Purchase {
                    CompradorId = usuarioId,
                    FechaCompra = DateTime.UtcNow,
                    Total = total
                };

                context.Purchases.Add(purchase);
                await context.SaveChangesAsync();

                // 5. Asignar productos a la compra (marcar como vendidos) y liberar reservas
                foreach (var producto in productos) {
                    producto.CompraId = purchase.Id;
                    producto.Reservado = false;
                    producto.ReservadoHasta = null;
                    producto.ReservadoPor = null;
                }

                await context.SaveChangesAsync();

                // 6. INVALIDAR CACHÉ: Productos vendidos ya no deben aparecer en listados
                cache.Remove(ProductsCacheKey);
                foreach (var producto in productos)
                    cache.Remove($"product_details_{producto.Id}");

                // 7. Vaciar el carrito
                var clearResult = await carritoService.ClearCarritoAsync(usuarioId);
                if (clearResult.IsFailure)
                    logger.LogWarning("Error al vaciar carrito después de compra: {Error}",
                        clearResult.Error.Message);

                // 8. Commit de la transacción
                await transaction.CommitAsync();

                logger.LogInformation("Compra {PurchaseId} creada exitosamente para usuario {UserId}",
                    purchase.Id, usuarioId);

                // 9. Cargar la compra con sus relaciones
                var purchaseWithDetails = await context.Purchases
                    .Include(p => p.Comprador)
                    .Include(p => p.Products)
                    .ThenInclude(prod => prod.Propietario)
                    .FirstOrDefaultAsync(p => p.Id == purchase.Id);

                return Result.Success<Purchase, DomainError>(purchaseWithDetails!);
            }
            catch (DbUpdateConcurrencyException ex) {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Error de concurrencia al crear compra para usuario {UserId}", usuarioId);
                return Result.Failure<Purchase, DomainError>(
                    GenericError.ConcurrencyError(
                        "Otro usuario ha modificado los datos. Por favor, intenta de nuevo."));
            }
            catch (Exception ex) {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Error al crear compra para usuario {UserId}", usuarioId);
                return Result.Failure<Purchase, DomainError>(
                    GenericError.DatabaseError($"Error al procesar la compra: {ex.Message}"));
            }
        });
    }

    public async Task<Result<Purchase, DomainError>> GetByIdAsync(long id) {
        try {
            var purchase = await context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                .ThenInclude(prod => prod.Propietario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null) return Result.Failure<Purchase, DomainError>(PurchaseError.NotFound(id));

            return Result.Success<Purchase, DomainError>(purchase);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al obtener compra {PurchaseId}", id);
            return Result.Failure<Purchase, DomainError>(
                GenericError.DatabaseError("Error al obtener la compra"));
        }
    }

    public async Task<Result<IEnumerable<Purchase>, DomainError>> GetByUserAsync(long usuarioId, int page = 1,
        int pageSize = 10) {
        try {
            var purchases = await context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                .Where(p => p.CompradorId == usuarioId)
                .OrderByDescending(p => p.FechaCompra)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result.Success<IEnumerable<Purchase>, DomainError>(purchases);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al obtener compras del usuario {UserId}", usuarioId);
            return Result.Failure<IEnumerable<Purchase>, DomainError>(
                GenericError.DatabaseError("Error al obtener las compras"));
        }
    }

    public async Task<Result<IEnumerable<Purchase>, DomainError>> GetAllAsync(int page = 1, int pageSize = 10) {
        try {
            var purchases = await context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                .OrderByDescending(p => p.FechaCompra)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result.Success<IEnumerable<Purchase>, DomainError>(purchases);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al obtener todas las compras");
            return Result.Failure<IEnumerable<Purchase>, DomainError>(
                GenericError.DatabaseError("Error al obtener las compras"));
        }
    }

    public async Task<Result<IEnumerable<Purchase>, DomainError>> GetByDateRangeAsync(
        DateTime desde, DateTime hasta, int page = 1, int pageSize = 10) {
        try {
            var purchases = await context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                .Where(p => p.FechaCompra >= desde && p.FechaCompra <= hasta)
                .OrderByDescending(p => p.FechaCompra)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result.Success<IEnumerable<Purchase>, DomainError>(purchases);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al obtener compras por rango de fechas");
            return Result.Failure<IEnumerable<Purchase>, DomainError>(
                GenericError.DatabaseError("Error al obtener las compras"));
        }
    }

    public async Task<Result<byte[], DomainError>> GeneratePdfAsync(long purchaseId) {
        try {
            var purchaseResult = await GetByIdAsync(purchaseId);
            if (purchaseResult.IsFailure) return Result.Failure<byte[], DomainError>(purchaseResult.Error);

            var purchase = purchaseResult.Value;

            // Generar PDF usando el servicio de PDF
            return await pdfService.GenerateInvoicePdfAsync(purchase);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al generar PDF para compra {PurchaseId}", purchaseId);
            return Result.Failure<byte[], DomainError>(
                PurchaseError.PdfGenerationFailed(ex.Message));
        }
    }
}