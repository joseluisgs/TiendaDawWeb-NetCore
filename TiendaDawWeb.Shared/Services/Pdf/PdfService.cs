using CSharpFunctionalExtensions;
using QuestPDF;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.Pdf;

namespace TiendaDawWeb.Shared.Services.Pdf;

/// <summary>
///     Servicio de generación de facturas en formato PDF usando QuestPDF
/// </summary>
public class PdfService : IPdfService {
    private const decimal IvaRate = 1.21m;
    private readonly ILogger<PdfService> _logger;

    /// <summary>
    ///     Inicializa el servicio PDF con licencia comunitaria.
    /// </summary>
    /// <param name="logger">Logger para errores</param>
    public PdfService(ILogger<PdfService> logger) {
        _logger = logger;
        Settings.License = LicenseType.Community;
    }

    /// <summary>
    ///     Genera un PDF con la factura de una compra.
    ///     Incluye información del cliente, productos comprados y desglose de IVA.
    /// </summary>
    /// <param name="purchase">Datos de la compra</param>
    /// <returns>Bytes del PDF generado o error</returns>
    public async Task<Result<byte[], DomainError>> GenerateInvoicePdfAsync(Models.Purchase purchase) {
        try {
            var pdfBytes = Document.Create(container => {
                container.Page(page => {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Height(100).Background(Colors.Grey.Lighten3)
                        .Padding(20).Column(column => {
                            column.Item().AlignCenter().Text("WALADAW").FontSize(24).Bold().FontColor(Colors.Blue.Darken2);
                        });

                    page.Content().PaddingVertical(20).Column(column => {
                        column.Item().Text($"FACTURA #{purchase.Id}").FontSize(20).Bold();
                        column.Item().Text($"Fecha: {purchase.FechaCompra:dd/MM/yyyy}");
                        column.Item().PaddingTop(10).Text($"Cliente: {purchase.Comprador?.Nombre ?? "N/A"}");
                        
                        column.Item().PaddingTop(20).Table(table => {
                            table.ColumnsDefinition(c => { c.RelativeColumn(3); c.RelativeColumn(1); });
                            table.Header(h => {
                                h.Cell().Text("Producto").Bold();
                                h.Cell().Text("Precio").Bold();
                            });
                            var i = 1;
                            foreach (var prod in purchase.Products) {
                                var bg = i++ % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                                table.Cell().Background(bg).Text(prod.Nombre);
                                table.Cell().Background(bg).Text($"{prod.Precio:C}");
                            }
                        });

                        var subtotal = purchase.Total / IvaRate;
                        var iva = purchase.Total - subtotal;
                        column.Item().PaddingTop(20).AlignRight().Text(text => {
                            text.Span($"Subtotal: {subtotal:C}\nIVA: {iva:C}\nTOTAL: {purchase.Total:C}").Bold();
                        });
                    });

                    page.Footer().Height(50).AlignCenter().Text("Gracias por su compra en WalaDaw").FontSize(10);
                });
            }).GeneratePdf();

            return Result.Success<byte[], DomainError>(pdfBytes);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error al generar PDF");
            return Result.Failure<byte[], DomainError>(PurchaseError.PdfGenerationFailed(ex.Message));
        }
    }
}
