using System.Data;
using CSharpFunctionalExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Carrito;
using TiendaDawWeb.Shared.Services.Email;
using TiendaDawWeb.Shared.Services.Pdf;
using TiendaDawWeb.Shared.Services.Purchase;

namespace TiendaDawWeb.Shared.Services.Purchase;

/// <summary>
///     Servicio de gestión de compras con soporte para concurrencia y caché
/// </summary>
public class PurchaseService(
    ApplicationDbContext context,
    ICarritoService carritoService,
    IPdfService pdfService,
    IEmailService emailService,
    IMemoryCache cache,
    ILogger<PurchaseService> logger
) : IPurchaseService
{
    private const string ProductsCacheKey = "all_products";
    private const int MaxRetries = 2;

    /// <summary>
    ///     Crea una compra a partir del carrito del usuario.
    ///     Implementa reintento automático por conflictos de concurrencia.
    /// </summary>
    /// <param name="usuarioId">ID del usuario comprador</param>
    /// <returns>La compra creada o un error</returns>
    public async Task<Result<Models.Purchase, DomainError>> CreatePurchaseFromCarritoAsync(long usuarioId)
    {
        var attempt = 0;

        while (attempt <= MaxRetries)
        {
            try
            {
                return await TryPurchaseAsync(usuarioId);
            }
            catch (DbUpdateConcurrencyException ex) when (IsSerializationFailure(ex) && attempt < MaxRetries)
            {
                attempt++;
                logger.LogWarning("Intento {Attempt} fallido por conflicto de concurrencia. Reintentando...", attempt);
                await Task.Delay(50 * attempt);
            }
        }

        return Result.Failure<Models.Purchase, DomainError>(
            PurchaseError.ProductNotAvailable("El producto fue adquirido por otro usuario. Por favor, intenta con otro."));
    }

    private async Task<Result<Models.Purchase, DomainError>> TryPurchaseAsync(long usuarioId)
    {
        var strategy = context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var carritoResult = await carritoService.GetCarritoByUsuarioIdAsync(usuarioId);
                if (carritoResult.IsFailure)
                    return Result.Failure<Models.Purchase, DomainError>(carritoResult.Error);

                var carritoItems = carritoResult.Value.ToList();
                if (!carritoItems.Any())
                    return Result.Failure<Models.Purchase, DomainError>(PurchaseError.EmptyCarrito());

                var productIds = carritoItems.Select(ci => ci.ProductoId).ToList();
                var productos = await context.Products
                    .Where(p => productIds.Contains(p.Id) && !p.Deleted)
                    .ToListAsync();

                foreach (var item in carritoItems)
                {
                    var producto = productos.FirstOrDefault(p => p.Id == item.ProductoId);
                    if (producto == null)
                    {
                        await transaction.RollbackAsync();
                        return Result.Failure<Models.Purchase, DomainError>(ProductError.NotFound(item.ProductoId));
                    }

                    if (producto.CompraId != null)
                    {
                        await transaction.RollbackAsync();
                        return Result.Failure<Models.Purchase, DomainError>(PurchaseError.ProductNotAvailable(producto.Nombre));
                    }

                    if (producto.Reservado && producto.ReservadoPor != usuarioId &&
                        producto.ReservadoHasta > DateTime.UtcNow)
                    {
                        await transaction.RollbackAsync();
                        return Result.Failure<Models.Purchase, DomainError>(PurchaseError.ProductNotAvailable(producto.Nombre));
                    }
                }

                var total = carritoItems.Sum(ci => ci.Precio);
                var purchase = new Models.Purchase
                {
                    CompradorId = usuarioId,
                    FechaCompra = DateTime.UtcNow,
                    Total = total
                };

                context.Purchases.Add(purchase);
                await context.SaveChangesAsync();

                foreach (var producto in productos)
                {
                    producto.CompraId = purchase.Id;
                    producto.Reservado = false;
                    producto.ReservadoHasta = null;
                    producto.ReservadoPor = null;
                }

                await context.SaveChangesAsync();

                var purchaseWithDetails = await context.Purchases
                    .Include(p => p.Comprador)
                    .Include(p => p.Products)
                    .ThenInclude(prod => prod.Propietario)
                    .FirstOrDefaultAsync(p => p.Id == purchase.Id);

                return await Result.Success<Models.Purchase, DomainError>(purchaseWithDetails!)
                    .Tap(_ =>
                    {
                        cache.Remove(ProductsCacheKey);
                        foreach (var producto in productos)
                            cache.Remove($"product_details_{producto.Id}");
                    })
                    .Tap(async _ =>
                    {
                        var clearResult = await carritoService.ClearCarritoAsync(usuarioId);
                        if (clearResult.IsFailure)
                            logger.LogWarning("Error al vaciar carrito: {Error}", clearResult.Error.Message);
                    })
                    .Tap(_ => transaction.CommitAsync());
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Error de concurrencia");
                throw;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "Error al crear compra");
                return Result.Failure<Models.Purchase, DomainError>(GenericError.DatabaseError($"Error: {ex.Message}"));
            }
        });
    }

    private static bool IsSerializationFailure(DbUpdateConcurrencyException ex)
    {
        var message = ex.InnerException?.Message ?? string.Empty;
        return message.Contains("40001") || message.Contains("3960") || message.Contains("serialization");
    }

    /// <summary>
    ///     Obtiene una compra específica por su ID con todos los detalles.
    /// </summary>
    /// <param name="id">ID de la compra</param>
    /// <returns>La compra encontrada o error</returns>
    public async Task<Result<Models.Purchase, DomainError>> GetByIdAsync(long id)
    {
        try
        {
            var purchase = await context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                .ThenInclude(prod => prod.Propietario)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (purchase == null)
                return Result.Failure<Models.Purchase, DomainError>(PurchaseError.NotFound(id));

            return Result.Success<Models.Purchase, DomainError>(purchase);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener compra");
            return Result.Failure<Models.Purchase, DomainError>(GenericError.DatabaseError("Error"));
        }
    }

    /// <summary>
    ///     Obtiene las compras de un usuario con paginación.
    /// </summary>
    /// <param name="usuarioId">ID del usuario</param>
    /// <param name="page">Número de página (1-based)</param>
    /// <param name="pageSize">Cantidad de elementos por página</param>
    /// <returns>Lista paginada de compras o error</returns>
    public async Task<Result<IEnumerable<Models.Purchase>, DomainError>> GetByUserAsync(long usuarioId, int page = 1,
        int pageSize = 10)
    {
        try
        {
            var purchases = await context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                .Where(p => p.CompradorId == usuarioId)
                .OrderByDescending(p => p.FechaCompra)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result.Success<IEnumerable<Models.Purchase>, DomainError>(purchases);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener compras");
            return Result.Failure<IEnumerable<Models.Purchase>, DomainError>(GenericError.DatabaseError("Error"));
        }
    }

    /// <summary>
    ///     Obtiene todas las compras del sistema con paginación (para admin).
    /// </summary>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Cantidad de elementos por página</param>
    /// <returns>Lista paginada de compras o error</returns>
    public async Task<Result<IEnumerable<Models.Purchase>, DomainError>> GetAllAsync(int page = 1, int pageSize = 10)
    {
        try
        {
            var purchases = await context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                .OrderByDescending(p => p.FechaCompra)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result.Success<IEnumerable<Models.Purchase>, DomainError>(purchases);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener compras");
            return Result.Failure<IEnumerable<Models.Purchase>, DomainError>(GenericError.DatabaseError("Error"));
        }
    }

    /// <summary>
    ///     Obtiene compras dentro de un rango de fechas con paginación.
    /// </summary>
    /// <param name="desde">Fecha de inicio</param>
    /// <param name="hasta">Fecha de fin</param>
    /// <param name="page">Número de página</param>
    /// <param name="pageSize">Cantidad de elementos por página</param>
    /// <returns>Lista paginada de compras o error</returns>
    public async Task<Result<IEnumerable<Models.Purchase>, DomainError>> GetByDateRangeAsync(DateTime desde, DateTime hasta,
        int page = 1, int pageSize = 10)
    {
        try
        {
            var purchases = await context.Purchases
                .Include(p => p.Comprador)
                .Include(p => p.Products)
                .Where(p => p.FechaCompra >= desde && p.FechaCompra <= hasta)
                .OrderByDescending(p => p.FechaCompra)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Result.Success<IEnumerable<Models.Purchase>, DomainError>(purchases);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al obtener compras por fecha");
            return Result.Failure<IEnumerable<Models.Purchase>, DomainError>(GenericError.DatabaseError("Error"));
        }
    }

    /// <summary>
    ///     Genera un PDF con la factura de la compra.
    /// </summary>
    /// <param name="purchaseId">ID de la compra</param>
    /// <returns>Bytes del PDF generado o error</returns>
    public async Task<Result<byte[], DomainError>> GeneratePdfAsync(long purchaseId)
    {
        try
        {
            var purchaseResult = await GetByIdAsync(purchaseId);
            if (purchaseResult.IsFailure)
                return Result.Failure<byte[], DomainError>(purchaseResult.Error);

            return await pdfService.GenerateInvoicePdfAsync(purchaseResult.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al generar PDF");
            return Result.Failure<byte[], DomainError>(PurchaseError.PdfGenerationFailed(ex.Message));
        }
    }

    /// <summary>
    ///     Envía un email de confirmación de compra con la factura adjunta.
    ///     No falla si el email no puede enviarse (por ejemplo, si SMTP no está configurado).
    /// </summary>
    /// <param name="purchaseId">ID de la compra</param>
    /// <returns>True si se emitió o success incluso si falló el envío</returns>
    public async Task<Result<bool, DomainError>> SendConfirmationEmailAsync(long purchaseId)
    {
        try
        {
            var purchaseResult = await GetByIdAsync(purchaseId);
            if (purchaseResult.IsFailure)
                return Result.Failure<bool, DomainError>(purchaseResult.Error);

            var purchase = purchaseResult.Value;

            if (string.IsNullOrEmpty(purchase.Comprador?.Email))
            {
                logger.LogWarning("Compra {PurchaseId}: No se puede enviar email, el comprador no tiene email", purchaseId);
                return Result.Success<bool, DomainError>(true);
            }

            logger.LogInformation("Compra {PurchaseId}: Generando PDF para enviar a {Email}", purchaseId, purchase.Comprador.Email);

            var pdfResult = await GeneratePdfAsync(purchaseId);
            var pdfAttachment = pdfResult.IsSuccess ? pdfResult.Value : null;

            logger.LogInformation("Compra {PurchaseId}: Enviando email de confirmación a {Email}", purchaseId, purchase.Comprador.Email);

            var emailResult = await emailService.SendPurchaseConfirmationEmailAsync(
                purchase.Comprador.Email,
                purchase,
                pdfAttachment);

            if (emailResult.IsFailure)
            {
                logger.LogWarning("Compra {PurchaseId}: Error al enviar email: {Error}", purchaseId, emailResult.Error.Message);
            }
            else
            {
                logger.LogInformation("Compra {PurchaseId}: Email de confirmación enviado exitosamente a {Email}", purchaseId, purchase.Comprador.Email);
            }

            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Compra {PurchaseId}: Excepción al enviar email de confirmación", purchaseId);
            return Result.Success<bool, DomainError>(true);
        }
    }
}
