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
        var subject = $"Confirmación de compra #{purchase.Id} - WalaDaw";
        
        var productosHtml = purchase.Products.Select(p => $@"
            <tr style='background-color: #f8f9fa;'>
                <td style='padding: 12px; border: 1px solid #dee2e6;'>{p.Nombre}</td>
                <td style='padding: 12px; border: 1px solid #dee2e6;'>{p.Categoria}</td>
                <td style='padding: 12px; border: 1px solid #dee2e6; text-align: right;'>{p.Precio:C}</td>
            </tr>").Aggregate("", (acc, next) => acc + next);

        var subtotal = purchase.Total / 1.21m;
        var iva = purchase.Total - subtotal;

        var body = $@"<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #0d6efd, #0a58ca); color: white; padding: 30px; text-align: center; }}
        .content {{ padding: 30px; background: #f8f9fa; }}
        .title {{ color: #0d6efd; font-size: 24px; margin-bottom: 20px; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
        th {{ background: #0d6efd; color: white; padding: 12px; text-align: left; }}
        .totals {{ margin-top: 20px; text-align: right; }}
        .total {{ font-size: 20px; font-weight: bold; color: #198754; }}
        .footer {{ text-align: center; padding: 20px; color: #6c757d; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1 style='margin: 0;'>🛒 WalaDaw</h1>
            <p style='margin: 5px 0 0 0;'>¡Gracias por tu compra!</p>
        </div>
        <div class='content'>
            <h2 class='title'>Confirmación de tu pedido</h2>
            <p>¡Tu pedido ha sido confirmado! Aquí tienes los detalles:</p>
            
            <p><strong>Número de pedido:</strong> #{purchase.Id}</p>
            <p><strong>Fecha:</strong> {purchase.FechaCompra:dd/MM/yyyy HH:mm}</p>
            <p><strong>Cliente:</strong> {purchase.Comprador?.Nombre ?? "N/A"}</p>
            
            <h3 style='color: #0d6efd; margin-top: 30px;'>Productos adquiridos</h3>
            <table>
                <thead>
                    <tr>
                        <th>Producto</th>
                        <th>Categoría</th>
                        <th style='text-align: right;'>Precio</th>
                    </tr>
                </thead>
                <tbody>
                    {productosHtml}
                </tbody>
            </table>
            
            <div class='totals'>
                <p style='margin: 5px 0;'>Subtotal: {subtotal:C}</p>
                <p style='margin: 5px 0;'>IVA (21%): {iva:C}</p>
                <p class='total'>TOTAL: {purchase.Total:C}</p>
            </div>
            
            <p style='margin-top: 30px; padding: 15px; background: #d1e7dd; border-radius: 5px; color: #0f5132;'>
                ✓ Tu pedido está siendo procesado. Recibirás otro email cuando sea enviado.
            </p>
        </div>
        <div class='footer'>
            <p>¿Tienes preguntas? Contáctanos en soporte@waladaw.com</p>
            <p>© 2024 WalaDaw. Todos los derechos reservados.</p>
        </div>
    </div>
</body>
</html>";

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
    /// <returns>True si se enviou o error</returns>
    public async Task<Result<bool, DomainError>> SendEmailAsync(
        string toEmail, string subject, string body, byte[]? attachment = null, string? attachmentName = null)
    {
        logger.LogInformation("Intentando enviar email a {To} - Asunto: {Subject}", toEmail, subject);

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
                logger.LogWarning("Configuracion SMTP no disponible o incompleta para {To}", toEmail);
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

            logger.LogInformation("Email enviado exitosamente a {To}", toEmail);

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
