using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Email;

/// <summary>
/// Servicio para el envío de emails.
/// Soporta envío directo y encolado para procesamiento asíncrono.
/// </summary>
public interface IEmailService
{
    Task<Result<bool, DomainError>> SendWelcomeEmailAsync(string toEmail, string userName);
    Task<Result<bool, DomainError>> SendPurchaseConfirmationEmailAsync(string toEmail, Models.Purchase purchase, byte[]? pdfAttachment = null);
    Task<Result<bool, DomainError>> SendEmailAsync(string toEmail, string subject, string body, byte[]? attachment = null, string? attachmentName = null);

    /// <summary>
    /// Encola un email para procesamiento asíncrono en segundo plano.
    /// </summary>
    void EnqueueEmail(EmailMessage emailMessage);
}
