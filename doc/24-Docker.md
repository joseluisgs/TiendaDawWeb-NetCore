# 24. Docker y Producción: Operaciones y Despliegue

## Índice

[24. Docker y Producción: Docker, Ficheros y Despliegue](#24-docker-y-producción-docker-ficheros-y-despliegue)
  - [24.1. Docker: El Contenedor Indestructible](#241-docker-el-contenedor-indestructible)
  - [24.2. Persistencia en Contenedores](#242-persistencia-en-contenedores)
  - [24.3. Procesamiento de Archivos](#243-procesamiento-de-archivos)
  - [24.4. Generación de PDFs](#244-generación-de-pdfs)

---

## 24.1. Docker: El Contenedor Indestructible

Docker empaqueta tu aplicación y todas sus dependencias en un "contenedor" aislado. Así, funciona igual en tu máquina que en el servidor de producción.

### El Dockerfile Multi-Stage

Un Dockerfile profesional no solo "pone tu código en una caja". Optimiza el tamaño de la imagen para que sea ligera, rápida de desplegar y más segura.

```dockerfile
# TiendaDawWeb-NetCore/TiendaDawWeb.Web/Dockerfile

# Etapa 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY *.slnx ./
COPY TiendaDawWeb.Shared/TiendaDawWeb.Shared.csproj TiendaDawWeb.Shared/
COPY TiendaDawWeb.Mvc/TiendaDawWeb.Mvc.csproj TiendaDawWeb.Mvc/
COPY TiendaDawWeb.RazorPages/TiendaDawWeb.RazorPages.csproj TiendaDawWeb.RazorPages/
COPY TiendaDawWeb.Tests/TiendaDawWeb.Tests.csproj TiendaDawWeb.Tests/
RUN dotnet restore
COPY . .
RUN dotnet publish TiendaDawWeb.Web/TiendaDawWeb.Web.csproj -c Release -o /publish

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /publish
COPY --from=build /publish .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "TiendaDawWeb.Web.dll"]
```

### docker-compose.yml

```yaml
version: '3.8'

services:
  web:
    build: 
      context: .
      dockerfile: TiendaDawWeb.Web/Dockerfile
    ports:
      - "5000:5000"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Data Source=app.db
    volumes:
      - uploads_data:/app/wwwroot/uploads
    
  sqlite:
    image: alpine:latest
    volumes:
      - sqlite_data:/data

volumes:
  uploads_data:
  sqlite_data:
```

---

## 24.2. Persistencia en Contenedores

### El Problema de wwwroot/uploads

Por defecto, los archivos subidos a `wwwroot/uploads` se pierden cuando el contenedor se reinicia porque Docker usa un sistema de archivos en capas.

### La Solución: Mapeo de Volúmenes

```yaml
services:
  web:
    build: .
    volumes:
      - ./uploads:/app/wwwroot/uploads:rw
```

### Persistencia de Base de Datos SQLite

```yaml
services:
  app:
    volumes:
      - sqlite_data:/app/data
    
volumes:
  sqlite_data:
```

---

## 24.3. Procesamiento de Archivos

### SixLabors.ImageSharp

```csharp
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

// Redimensionar imagen
public async Task<string> ResizeImageAsync(IFormFile file)
{
    using var image = await Image.LoadAsync(file.OpenReadStream());
    
    image.Mutate(x => x.Resize(new ResizeOptions
    {
        Size = new Size(800, 600),
        Mode = ResizeMode.Stretch
    }));
    
    var outputPath = Path.Combine(_env.ContentRootPath, "uploads", file.FileName);
    
    await image.SaveAsync(outputPath);
    
    return outputPath;
}
```

### PhysicalFileProvider

```csharp
using Microsoft.Extensions.FileProviders;

var uploadsPath = Path.Combine(builder.Environment.ContentRootPath, "uploads");

// Crear proveedor de archivos
var fileProvider = new PhysicalFileProvider(uploadsPath);

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = fileProvider,
    RequestPath = "/uploads"
});
```

---

## 24.4. Generación de PDFs

### QuestPDF

```csharp
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class InvoiceDocument : IDocument
{
    private readonly InvoiceData _data;

    public InvoiceDocument(InvoiceData data)
    {
        _data = data;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Margin(50);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(12));

                page.Header().Element(ComposeHeader);
                page.Content().Element(ComposeContent);
                page.Footer().Element(ComposeFooter);
            });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Text($"Factura #{_data.InvoiceNumber}").Bold().FontSize(24);
            column.Item().Text(_data.Date.ToString("dd/MM/yyyy"));
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Column(column =>
        {
            foreach (var item in _data.Items)
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Text(item.Name);
                    row.ConstantItem(100).AlignRight().Text(item.Price.ToString("C"));
                });
            }
        });
    }
}
```

### Generar PDF

```csharp
[HttpGet("invoice/{id}/pdf")]
public async Task<IActionResult> GenerateInvoicePdf(long id)
{
    var invoice = await _invoiceService.GetInvoiceDataAsync(id);
    
    var document = new InvoiceDocument(invoice);
    var pdfBytes = document.GeneratePdf();
    
    return File(pdfBytes, "application/pdf", $"factura-{id}.pdf");
}
```

---

## Resumen

| Concepto           | Descripción                                              |
| ------------------ | -------------------------------------------------------- |
| **Docker**        | Contenedor con todas las dependencias                   |
| **Dockerfile**    | Receta para construir la imagen                        |
| **Volumes**       | Persistencia de datos fuera del contenedor              |
| **ImageSharp**    | Procesamiento de imágenes                               |
| **QuestPDF**      | Generación de PDFs profesionales                       |

---

**Anterior**: [23. Output Cache](../23-OutputCache.md)  
**Próximo**: [25. Logging](../25-Logging.md)
