using CSharpFunctionalExtensions;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Shared.Services.Purchase;

/// <summary>
/// Servicio para la gestión de compras
/// </summary>
public interface IPurchaseService
{
    Task<Result<Models.Purchase, DomainError>> CreatePurchaseFromCarritoAsync(long usuarioId);
    Task<Result<Models.Purchase, DomainError>> GetByIdAsync(long id);
    Task<Result<IEnumerable<Models.Purchase>, DomainError>> GetByUserAsync(long usuarioId, int page = 1, int pageSize = 10);
    Task<Result<byte[], DomainError>> GeneratePdfAsync(long purchaseId);
    Task<Result<IEnumerable<Models.Purchase>, DomainError>> GetAllAsync(int page = 1, int pageSize = 10);
    Task<Result<IEnumerable<Models.Purchase>, DomainError>> GetByDateRangeAsync(DateTime desde, DateTime hasta, int page = 1, int pageSize = 10);
}
