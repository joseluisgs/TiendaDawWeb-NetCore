using Microsoft.Extensions.Configuration;
using System.Threading.Channels;
using CSharpFunctionalExtensions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Email;

namespace TiendaDawWeb.Shared.Services.Email;

/// <summary>
///     Servicio de envío de emails con soporte para cola asíncrona y adjuntos
/// </summary>
public class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger,
    Channel<EmailMessage> emailChannel
) : IEmailService
{
    private readonly Channel<EmailMessage> _emailChannel = emailChannel;

    /// <summary>
    ///     Envía un email de bienvenida a un nuevo usuario.
    /// </summary>
    /// <param name="toEmail">Email del destinatario</param>
    /// <param name="userName">Nombre del usuario</param>
    /// <returns>True si se envió o error</returns>
    public async Task<Result<bool, DomainError>> SendWelcomeEmailAsync(string toEmail, string userName)
    {
        var subject = "Bienvenido a WalaDaw!";
        var body = $@"<html><body><h1>Hola {userName},</h1><p>Gracias por registrarte.</p></body></html>";
        return await SendEmailAsync(toEmail, subject, body);
    }

    /// <summary>
    ///     Envía un email de confirmación de compra con factura adjunta opcional.
    /// </summary>
    /// <param name="toEmail">Email del destinatario</param>
    /// <param name="purchase">Datos de la compra</param>
    /// <param name="pdfAttachment">Bytes del PDF de factura</param>
    /// <returns>True si se envió o error</returns>
    public async Task<Result<bool, DomainError>> SendPurchaseConfirmationEmailAsync(
        string toEmail, Models.Purchase purchase, byte[]? pdfAttachment = null)
    {
        var subject = $"Confirmacion de compra #{purchase.Id} - WalaDaw";
        var body = $@"<html><body><h1>Compra confirmada!</h1><p>Total: {purchase.Total:C}</p></body></html>";
        return await SendEmailAsync(toEmail, subject, body, pdfAttachment, $"factura-{purchase.Id}.pdf");
    }

    /// <summary>
    ///     Envía un email con opciones de adjuntos.
    /// </summary>
    /// <param name="toEmail">Email del destinatario</param>
    /// <param name="subject">Asunto del email</param>
    /// <param name="body">Cuerpo del email (HTML soportado)</param>
    /// <param name="attachment">Bytes del adjunto opcional</param>
    /// <param name="attachmentName">Nombre del adjunto</param>
    /// <returns>True si se envió o error</returns>
    public async Task<Result<bool, DomainError>> SendEmailAsync(
        string toEmail, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
    {
        try
        {
            var smtpHost = configuration["Email:SmtpHost"];
            var smtpPortRaw = configuration["Email:SmtpPort"] ?? "587";
            var smtpUser = configuration["Email:SmtpUser"];
            var smtpPass = configuration["Email:SmtpPass"];
            var fromEmail = configuration["Email:FromEmail"] ?? smtpUser;
            var fromName = configuration["Email:FromName"] ?? "WalaDaw";

            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || 
                smtpHost.StartsWith("${") || smtpPortRaw.StartsWith("${"))
            {
                logger.LogWarning("Configuracion SMTP no disponible o incompleta");
                return Result.Success<bool, DomainError>(true);
            }

            var smtpPort = int.Parse(smtpPortRaw);

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            if (attachment != null && !string.IsNullOrEmpty(attachmentName))
                builder.Attachments.Add(attachmentName, attachment);

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            if (!string.IsNullOrEmpty(smtpPass)) await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error al enviar email a {Email}", toEmail);
            return Result.Failure<bool, DomainError>(
                GenericError.UnexpectedError($"Error al enviar email: {ex.Message}"));
        }
    }

    /// <summary>
    ///     Encola un mensaje de email para procesamiento asíncrono.
    /// </summary>
    /// <param name="emailMessage">Mensaje a encolar</param>
    public void EnqueueEmail(EmailMessage emailMessage)
    {
        _emailChannel.Writer.TryWrite(emailMessage);
        logger.LogDebug("Email encolado para: {To}", emailMessage.To);
    }
}
