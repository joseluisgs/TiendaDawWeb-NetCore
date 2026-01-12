using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Pdf;

/// <summary>
/// Servicio para la generación de PDFs
/// </summary>
public interface IPdfService
{
    Task<Result<byte[], DomainError>> GenerateInvoicePdfAsync(Models.Purchase purchase);
}
