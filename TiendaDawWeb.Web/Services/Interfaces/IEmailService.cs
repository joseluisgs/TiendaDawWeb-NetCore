using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;

namespace TiendaDawWeb.Services.Interfaces;

/// <summary>
/// Servicio para el envío de emails
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un email de bienvenida al registrarse
    /// </summary>
    Task<Result<bool, DomainError>> SendWelcomeEmailAsync(string toEmail, string userName);

    /// <summary>
    /// Envía un email de confirmación de compra con PDF adjunto
    /// </summary>
    Task<Result<bool, DomainError>> SendPurchaseConfirmationEmailAsync(string toEmail, Purchase purchase, byte[]? pdfAttachment = null);

    /// <summary>
    /// Envía un email genérico
    /// </summary>
    Task<Result<bool, DomainError>> SendEmailAsync(string toEmail, string subject, string body, byte[]? attachment = null, string? attachmentName = null);
}
