using CSharpFunctionalExtensions;
using TiendaDawWeb.Shared.Errors;

namespace TiendaDawWeb.Shared.Services.Pdf;

/// <summary>
///     Servicio para la generación de PDFs
/// </summary>
public interface IPdfService {
    Task<Result<byte[], DomainError>> GenerateInvoicePdfAsync(Models.Purchase purchase);
}
