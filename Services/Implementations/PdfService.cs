using CSharpFunctionalExtensions;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Colors;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
/// Implementación del servicio de generación de PDFs usando iText7
/// </summary>
public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<byte[], DomainError>> GenerateInvoicePdfAsync(Purchase purchase)
    {
        try
        {
            using var memoryStream = new MemoryStream();
            
            // Crear el documento PDF
            var writer = new PdfWriter(memoryStream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Configurar márgenes
            document.SetMargins(50, 50, 50, 50);

            // Header - Logo y título
            var header = new Paragraph("WALADAW")
                .SetFontSize(28)
                .SetBold()
                .SetFontColor(ColorConstants.BLUE)
                .SetTextAlignment(TextAlignment.CENTER);
            document.Add(header);

            var subheader = new Paragraph("Marketplace de Segunda Mano")
                .SetFontSize(12)
                .SetItalic()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20);
            document.Add(subheader);

            // Línea separadora
            document.Add(new Paragraph()
                .SetBorderBottom(new iText.Layout.Borders.SolidBorder(ColorConstants.GRAY, 1))
                .SetMarginBottom(20));

            // Información de factura
            var invoiceTitle = new Paragraph("FACTURA")
                .SetFontSize(24)
                .SetBold()
                .SetMarginBottom(10);
            document.Add(invoiceTitle);

            var invoiceInfo = new Paragraph()
                .Add(new Text($"Número de Factura: #{purchase.Id}\n").SetBold())
                .Add(new Text($"Fecha: {purchase.FechaCompra:dd/MM/yyyy HH:mm}\n"))
                .SetMarginBottom(20);
            document.Add(invoiceInfo);

            // Información del cliente
            var clientInfo = new Paragraph("DATOS DEL CLIENTE")
                .SetFontSize(14)
                .SetBold()
                .SetMarginBottom(5);
            document.Add(clientInfo);

            var clientDetails = new Paragraph()
                .Add(new Text($"Nombre: {purchase.Comprador?.Nombre ?? "N/A"} {purchase.Comprador?.Apellidos ?? ""}\n"))
                .Add(new Text($"Email: {purchase.Comprador?.Email ?? "N/A"}\n"))
                .Add(new Text($"Username: {purchase.Comprador?.UserName ?? "N/A"}\n"))
                .SetMarginBottom(20);
            document.Add(clientDetails);

            // Tabla de productos
            var productsTitle = new Paragraph("PRODUCTOS")
                .SetFontSize(14)
                .SetBold()
                .SetMarginBottom(10);
            document.Add(productsTitle);

            // Crear tabla con 4 columnas
            var table = new Table(UnitValue.CreatePercentArray(new[] { 10f, 40f, 25f, 25f }))
                .UseAllAvailableWidth();

            // Headers de la tabla
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph("#").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.CENTER));
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph("Producto").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph("Categoría").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY));
            table.AddHeaderCell(new Cell()
                .Add(new Paragraph("Precio").SetBold())
                .SetBackgroundColor(ColorConstants.LIGHT_GRAY)
                .SetTextAlignment(TextAlignment.RIGHT));

            // Filas de productos
            int index = 1;
            foreach (var producto in purchase.Products)
            {
                table.AddCell(new Cell()
                    .Add(new Paragraph(index.ToString()))
                    .SetTextAlignment(TextAlignment.CENTER));
                table.AddCell(new Cell()
                    .Add(new Paragraph(producto.Nombre)));
                table.AddCell(new Cell()
                    .Add(new Paragraph(producto.Categoria.ToString())));
                table.AddCell(new Cell()
                    .Add(new Paragraph($"{producto.Precio:C}"))
                    .SetTextAlignment(TextAlignment.RIGHT));
                index++;
            }

            document.Add(table);

            // Resumen de totales
            var subtotal = purchase.Total / 1.21m; // Suponiendo IVA 21%
            var iva = purchase.Total - subtotal;

            var totalsTable = new Table(UnitValue.CreatePercentArray(new[] { 70f, 30f }))
                .UseAllAvailableWidth()
                .SetMarginTop(20);

            totalsTable.AddCell(new Cell()
                .Add(new Paragraph("Subtotal:").SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            totalsTable.AddCell(new Cell()
                .Add(new Paragraph($"{subtotal:C}").SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            totalsTable.AddCell(new Cell()
                .Add(new Paragraph("IVA (21%):").SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            totalsTable.AddCell(new Cell()
                .Add(new Paragraph($"{iva:C}").SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));

            totalsTable.AddCell(new Cell()
                .Add(new Paragraph("TOTAL:").SetBold().SetFontSize(14).SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER));
            totalsTable.AddCell(new Cell()
                .Add(new Paragraph($"{purchase.Total:C}").SetBold().SetFontSize(14).SetTextAlignment(TextAlignment.RIGHT))
                .SetBorder(iText.Layout.Borders.Border.NO_BORDER)
                .SetFontColor(ColorConstants.GREEN));

            document.Add(totalsTable);

            // Footer
            var footer = new Paragraph("Gracias por su compra en WalaDaw")
                .SetFontSize(10)
                .SetItalic()
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginTop(40)
                .SetFontColor(ColorConstants.GRAY);
            document.Add(footer);

            var footerDetails = new Paragraph("Este documento es una factura simplificada válida para efectos fiscales.")
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontColor(ColorConstants.GRAY);
            document.Add(footerDetails);

            // Cerrar documento
            document.Close();

            var pdfBytes = memoryStream.ToArray();
            
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
