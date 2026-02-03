namespace TiendaDawWeb.Shared.Services.Email;

/// <summary>
/// Mensaje de email para la cola de procesamiento asíncrono.
/// </summary>
public class EmailMessage
{
    public required string To { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsHtml { get; init; }
    public byte[]? Attachment { get; init; }
    public string? AttachmentName { get; init; }
}
