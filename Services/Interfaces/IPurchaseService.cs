using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Interfaces;

/// <summary>
/// Servicio para la gestión de compras
/// </summary>
public interface IPurchaseService
{
    /// <summary>
    /// Crea una compra a partir del carrito del usuario
    /// </summary>
    Task<Result<Purchase, DomainError>> CreatePurchaseFromCarritoAsync(long usuarioId);

    /// <summary>
    /// Obtiene una compra por ID
    /// </summary>
    Task<Result<Purchase, DomainError>> GetByIdAsync(long id);

    /// <summary>
    /// Obtiene todas las compras de un usuario (paginadas)
    /// </summary>
    Task<Result<IEnumerable<Purchase>, DomainError>> GetByUserAsync(long usuarioId, int page = 1, int pageSize = 10);

    /// <summary>
    /// Genera un PDF de factura para una compra
    /// </summary>
    Task<Result<byte[], DomainError>> GeneratePdfAsync(long purchaseId);

    /// <summary>
    /// Obtiene todas las compras (para admin)
    /// </summary>
    Task<Result<IEnumerable<Purchase>, DomainError>> GetAllAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// Obtiene compras filtradas por fecha
    /// </summary>
    Task<Result<IEnumerable<Purchase>, DomainError>> GetByDateRangeAsync(DateTime desde, DateTime hasta, int page = 1, int pageSize = 10);
}
