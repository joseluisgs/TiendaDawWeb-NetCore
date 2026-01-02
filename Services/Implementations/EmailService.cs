using CSharpFunctionalExtensions;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
///     Implementación del servicio de emails usando MailKit
/// </summary>
public class EmailService(
    IConfiguration configuration,
    ILogger<EmailService> logger
) : IEmailService {
    public async Task<Result<bool, DomainError>> SendWelcomeEmailAsync(string toEmail, string userName) {
        var subject = "¡Bienvenido a WalaDaw! 🛒";
        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #0d6efd; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f8f9fa; }}
                    .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>¡Bienvenido a WalaDaw!</h1>
                    </div>
                    <div class='content'>
                        <h2>Hola {userName},</h2>
                        <p>Gracias por registrarte en WalaDaw, tu marketplace de confianza para productos de segunda mano.</p>
                        <p>Ahora puedes:</p>
                        <ul>
                            <li>Comprar productos de calidad a precios increíbles</li>
                            <li>Vender tus productos usados</li>
                            <li>Valorar tus compras</li>
                            <li>Gestionar tu perfil</li>
                        </ul>
                        <p>¡Esperamos que disfrutes de la experiencia!</p>
                    </div>
                    <div class='footer'>
                        <p>Este es un email automático, por favor no responder.</p>
                        <p>&copy; 2025 WalaDaw - Todos los derechos reservados</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        return await SendEmailAsync(toEmail, subject, body);
    }

    public async Task<Result<bool, DomainError>> SendPurchaseConfirmationEmailAsync(
        string toEmail, Purchase purchase, byte[]? pdfAttachment = null) {
        var subject = $"Confirmación de compra #{purchase.Id} - WalaDaw";

        var productsList = string.Join("", purchase.Products.Select(p =>
            $"<li>{p.Nombre} - {p.Precio:C}</li>"));

        var body = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
                    .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
                    .header {{ background-color: #198754; color: white; padding: 20px; text-align: center; }}
                    .content {{ padding: 20px; background-color: #f8f9fa; }}
                    .order-details {{ background-color: white; padding: 15px; margin: 20px 0; border-radius: 5px; }}
                    .total {{ font-size: 18px; font-weight: bold; color: #198754; }}
                    .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>¡Compra Confirmada! ✅</h1>
                    </div>
                    <div class='content'>
                        <h2>Gracias por tu compra, {purchase.Comprador?.Nombre ?? "Cliente"}!</h2>
                        <p>Tu pedido ha sido procesado exitosamente.</p>
                        
                        <div class='order-details'>
                            <h3>Detalles de la compra:</h3>
                            <p><strong>Número de pedido:</strong> #{purchase.Id}</p>
                            <p><strong>Fecha:</strong> {purchase.FechaCompra:dd/MM/yyyy HH:mm}</p>
                            
                            <h4>Productos:</h4>
                            <ul>
                                {productsList}
                            </ul>
                            
                            <p class='total'>Total: {purchase.Total:C}</p>
                        </div>
                        
                        <p>Adjuntamos tu factura en PDF.</p>
                        <p>Puedes ver los detalles de tu compra en tu perfil.</p>
                    </div>
                    <div class='footer'>
                        <p>Este es un email automático, por favor no responder.</p>
                        <p>&copy; 2025 WalaDaw - Todos los derechos reservados</p>
                    </div>
                </div>
            </body>
            </html>
        ";

        return await SendEmailAsync(toEmail, subject, body, pdfAttachment, $"factura-{purchase.Id}.pdf");
    }

    public async Task<Result<bool, DomainError>> SendEmailAsync(
        string toEmail, string subject, string body, byte[]? attachment = null, string? attachmentName = null) {
        try {
            // Obtener configuración SMTP
            var smtpHost = configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = configuration["Email:SmtpUser"];
            var smtpPass = configuration["Email:SmtpPass"];
            var fromEmail = configuration["Email:FromEmail"] ?? smtpUser;
            var fromName = configuration["Email:FromName"] ?? "WalaDaw";

            // Si no hay configuración SMTP, solo loguear (útil para desarrollo)
            if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser)) {
                logger.LogWarning("Configuración SMTP no disponible. Email no enviado a {Email}: {Subject}",
                    toEmail, subject);
                return Result.Success<bool, DomainError>(true);
            }

            // Crear el mensaje
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;

            // Crear el body builder
            var builder = new BodyBuilder {
                HtmlBody = body
            };

            // Adjuntar archivo si existe
            if (attachment != null && !string.IsNullOrEmpty(attachmentName))
                builder.Attachments.Add(attachmentName, attachment);

            message.Body = builder.ToMessageBody();

            // Enviar el email
            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

            if (!string.IsNullOrEmpty(smtpPass)) await client.AuthenticateAsync(smtpUser, smtpPass);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            logger.LogInformation("Email enviado exitosamente a {Email}: {Subject}", toEmail, subject);

            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error al enviar email a {Email}", toEmail);
            return Result.Failure<bool, DomainError>(
                GenericError.UnexpectedError($"Error al enviar email: {ex.Message}"));
        }
    }
}