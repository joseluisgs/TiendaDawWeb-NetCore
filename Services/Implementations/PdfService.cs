using CSharpFunctionalExtensions;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
/// Implementación del servicio de generación de PDFs usando QuestPDF
/// </summary>
public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;
    private const decimal IVA_RATE = 1.21m; // IVA 21% español

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
        
        // Configure QuestPDF license for community use
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<Result<byte[], DomainError>> GenerateInvoicePdfAsync(Purchase purchase)
    {
        try
        {
            var pdfBytes = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header()
                        .Height(120)
                        .Background(Colors.Grey.Lighten3)
                        .Padding(20)
                        .Column(column =>
                        {
                            column.Item().AlignCenter().Text("WALADAW")
                                .FontSize(28)
                                .Bold()
                                .FontColor(Colors.Blue.Darken2);
                            
                            column.Item().AlignCenter().Text("Marketplace de Segunda Mano")
                                .FontSize(12)
                                .Italic()
                                .FontColor(Colors.Grey.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(20)
                        .Column(column =>
                        {
                            // Invoice title and number
                            column.Item().Text("FACTURA")
                                .FontSize(24)
                                .Bold();
                            
                            column.Item().PaddingBottom(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                            
                            column.Item().PaddingTop(10).PaddingBottom(20).Text(text =>
                            {
                                text.Span("Número de Factura: ").Bold();
                                text.Span($"#{purchase.Id}\n");
                                text.Span("Fecha: ");
                                text.Span($"{purchase.FechaCompra:dd/MM/yyyy HH:mm}\n");
                            });

                            // Customer information
                            column.Item().PaddingBottom(10).Text("DATOS DEL CLIENTE")
                                .FontSize(14)
                                .Bold();
                            
                            column.Item().PaddingBottom(20).Text(text =>
                            {
                                text.Line($"Nombre: {purchase.Comprador?.Nombre ?? "N/A"} {purchase.Comprador?.Apellidos ?? ""}");
                                text.Line($"Email: {purchase.Comprador?.Email ?? "N/A"}");
                                text.Line($"Username: {purchase.Comprador?.UserName ?? "N/A"}");
                            });

                            // Products table
                            column.Item().PaddingBottom(10).Text("PRODUCTOS")
                                .FontSize(14)
                                .Bold();

                            column.Item().Table(table =>
                            {
                                // Define columns
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(40);  // #
                                    columns.RelativeColumn(3);   // Producto
                                    columns.RelativeColumn(2);   // Categoría
                                    columns.RelativeColumn(1.5f);  // Precio
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignCenter().Text("#").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Producto").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).Text("Categoría").Bold();
                                    header.Cell().Background(Colors.Grey.Lighten2).Padding(5).AlignRight().Text("Precio").Bold();
                                });

                                // Rows
                                int index = 1;
                                foreach (var producto in purchase.Products)
                                {
                                    var backgroundColor = index % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                                    
                                    table.Cell().Background(backgroundColor).Padding(5).AlignCenter().Text(index.ToString());
                                    table.Cell().Background(backgroundColor).Padding(5).Text(producto.Nombre);
                                    table.Cell().Background(backgroundColor).Padding(5).Text(producto.Categoria.ToString());
                                    table.Cell().Background(backgroundColor).Padding(5).AlignRight().Text($"{producto.Precio:C}");
                                    
                                    index++;
                                }
                            });

                            // Totals
                            var subtotal = purchase.Total / IVA_RATE; // IVA 21%
                            var iva = purchase.Total - subtotal;

                            column.Item().PaddingTop(20).AlignRight().Column(totalsColumn =>
                            {
                                totalsColumn.Item().Text(text =>
                                {
                                    text.Span("Subtotal: ").SemiBold();
                                    text.Span($"{subtotal:C}");
                                });
                                
                                totalsColumn.Item().Text(text =>
                                {
                                    text.Span("IVA (21%): ").SemiBold();
                                    text.Span($"{iva:C}");
                                });
                                
                                totalsColumn.Item().PaddingTop(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);
                                
                                totalsColumn.Item().PaddingTop(5).Text(text =>
                                {
                                    text.Span("TOTAL: ").Bold().FontSize(14);
                                    text.Span($"{purchase.Total:C}").Bold().FontSize(14).FontColor(Colors.Green.Darken2);
                                });
                            });
                        });

                    page.Footer()
                        .Height(60)
                        .AlignCenter()
                        .Column(column =>
                        {
                            column.Item().PaddingTop(20).Text("Gracias por su compra en WalaDaw")
                                .FontSize(10)
                                .Italic()
                                .FontColor(Colors.Grey.Darken1);
                            
                            column.Item().PaddingTop(5).Text("Este documento es una factura simplificada válida para efectos fiscales.")
                                .FontSize(8)
                                .FontColor(Colors.Grey.Medium);
                        });
                });
            }).GeneratePdf();
            
            _logger.LogInformation("PDF generado exitosamente para compra {PurchaseId}, tamaño: {Size} bytes", 
                purchase.Id, pdfBytes.Length);

            return await Task.FromResult(Result.Success<byte[], DomainError>(pdfBytes));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar PDF para compra {PurchaseId}", purchase.Id);
            return await Task.FromResult(Result.Failure<byte[], DomainError>(
                PurchaseError.PdfGenerationFailed(ex.Message)));
        }
    }
}
