using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Interfaces;

/// <summary>
/// Servicio para la generación de PDFs
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Genera un PDF de factura para una compra
    /// </summary>
    Task<Result<byte[], DomainError>> GenerateInvoicePdfAsync(Purchase purchase);
}
