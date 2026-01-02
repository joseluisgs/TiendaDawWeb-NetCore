# 08. Operaciones y Producción: Docker, Ficheros y Despliegue (La Hora de la Verdad)

Tu aplicación funciona en local. Ahora, ¿cómo la preparas para el mundo real, donde miles de usuarios la usarán, con un rendimiento óptimo y una seguridad férrea?

## 1. Docker: El Contenedor Indestructible

Docker empaqueta tu aplicación y todas sus dependencias en un "contenedor" aislado. Así, funciona igual en tu máquina que en el servidor de producción.

### 1.1. El Dockerfile Multi-Stage: La Dieta Extrema de tu Aplicación

Un Dockerfile profesional no solo "pone tu código en una caja". Optimiza el tamaño de la imagen para que sea ligera, rápida de desplegar y más segura.

```dockerfile
# TiendaDawWeb-NetCore/TiendaDawWeb.Web/Dockerfile

# Stage 1: build - Aquí usamos el SDK de .NET (la "caja de herramientas" grande)
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia el archivo .csproj y restaura las dependencias para aprovechar el cacheado de capas
COPY ["TiendaDawWeb.Web/TiendaDawWeb.csproj", "TiendaDawWeb.Web/"]
RUN dotnet restore "TiendaDawWeb.Web/TiendaDawWeb.csproj"

# Copia todo el código fuente y compila la aplicación
COPY . .
WORKDIR "/src/TiendaDawWeb.Web" # Mueve el WORKDIR al directorio del proyecto web
RUN dotnet build "TiendaDawWeb.csproj" -c Release -o /app/build

# Stage 2: publish - Genera los binarios listos para ejecutar
FROM build AS publish
RUN dotnet publish "TiendaDawWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: final - Aquí usamos solo el Runtime de .NET (la "caja de herramientas" pequeña)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080 # Puerto que escucha la aplicación dentro del contenedor
EXPOSE 8081 # Puerto para HTTPS

# Copia los archivos publicados desde la fase 'publish'
COPY --from=publish /app/publish .

# Crea el directorio de uploads y asigna permisos (muy importante en Linux/Docker)
RUN mkdir -p wwwroot/uploads && chmod 777 wwwroot/uploads

# Variables de entorno
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Comando para arrancar la aplicación
ENTRYPOINT ["dotnet", "TiendaDawWeb.dll"]
```

**Explicación Detallada:**
-   **Stage 1 (`AS build`)**: Usa la imagen `dotnet/sdk` (que incluye todas las herramientas de desarrollo y compilación, pesando cientos de MB). Se copia solo el `.csproj` para que Docker cachee la restauración de paquetes. Luego se copia el resto del código y se compila.
-   **Stage 2 (`AS publish`)**: Genera una versión auto-contenida y optimizada de tu aplicación.
-   **Stage 3 (`AS final`)**: La magia final. Usa la imagen `dotnet/aspnet` (solo el runtime, mucho más ligera). Copia **solo** el resultado de la etapa `publish`. La imagen final no contiene código fuente ni herramientas de compilación, lo que la hace más pequeña, más rápida de desplegar y más segura.

---

## 2. Persistencia en Contenedores: Los Volúmenes de Docker

Los contenedores Docker son efímeros. Si apagas o borras un contenedor, ¡todo lo que guardaste dentro desaparece!

### 2.1. El Problema de `wwwroot/uploads`:
Si tu aplicación permite subir fotos de productos, y estas fotos se guardan dentro del contenedor (`/app/wwwroot/uploads`), se perderán cada vez que el contenedor se reinicie o se elimine.

### 2.2. La Solución: Mapeo de Volúmenes en `docker-compose.yml`
Creamos un "volumen" que es una carpeta especial en el disco duro del servidor (fuera del contenedor) y la "mapeamos" a una carpeta dentro del contenedor.
```yaml
# TiendaDawWeb-NetCore/docker-compose.yml
services:
  waladaw:
    # ... otras configuraciones ...
    volumes:
      - waladaw-uploads:/app/wwwroot/uploads # Mapea el volumen externo al path interno del contenedor
    # ...
volumes:
  waladaw-uploads: # Define el volumen nombrado
    driver: local
```
**Lección de Supervivencia**: Siempre que tu aplicación necesite almacenar datos que deban sobrevivir al ciclo de vida del contenedor (bases de datos, archivos subidos por usuarios, logs), usa volúmenes persistentes.

---

## 3. Procesamiento Profesional de Archivos: El Caso de las Imágenes

Nunca confíes en los usuarios. Si te suben una imagen de 10 MB, tu servidor puede colapsar o tu web ir lenta.

### 3.1. `SixLabors.ImageSharp`: El Cuchillo Suizo de las Imágenes
Esta librería se usa en `StorageService` para:
-   **Redimensionar**: Limitar el tamaño máximo de las imágenes (ej. a 800px).
-   **Comprimir**: Reducir el tamaño del archivo sin perder calidad visible.
-   **Optimizar Formato**: Convertir a formatos más eficientes (como WebP o AVIF si fuera necesario).
-   **Cambio de Nombres**: Guardar la imagen con un `GUID` (un identificador único global) en lugar de su nombre original. Esto evita que dos usuarios suban una imagen con el mismo nombre y se sobreescriban, o que un usuario malicioso adivine nombres de archivos.

### 3.2. Sirviendo Archivos Fuera de `wwwroot` (`PhysicalFileProvider`)
Por defecto, `app.UseStaticFiles()` solo sirve archivos de `wwwroot`. Si tienes archivos en otra carpeta (ej. `/uploads` dentro de `wwwroot` pero con una configuración específica), necesitas un `FileProvider` personalizado:
```csharp
// TiendaDawWeb.Web/Program.cs
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadPath), // Apunta a la carpeta física
    RequestPath = "/uploads"                             // La ruta virtual para acceder a ellos
});
```
Esto permite que, si un usuario accede a `/uploads/mi-foto.jpg`, el servidor sepa dónde buscar esa foto en el sistema de archivos.

---

## 4. Generación de Facturas en PDF (`QuestPDF`)

Generar documentos PDF complejos es una tarea que requiere precisión.

### 4.1. El Desafío de los PDFs:
-   Convertir HTML a PDF es lento, consume muchos recursos y es propenso a errores de formato.
-   Generar PDFs directamente es dibujar el documento.

### 4.2. La Solución: `QuestPDF`
`QuestPDF` es una librería que permite definir la estructura de un documento PDF usando código C# de forma declarativa (parecido a cómo se construye una UI).
-   Define un "layout" (filas, columnas, texto, imágenes).
-   Genera el PDF directamente en memoria.
-   El servidor puede entonces enviarlo al navegador como un `FileStreamResult` (un "chorro de bytes" que el navegador interpreta como un PDF).

```csharp
// TiendaDawWeb.Web/Services/Implementations/PdfService.cs (Simplificado)
public async Task<byte[]> GenerateInvoicePdfAsync(Purchase purchase)
{
    var document = Document.Create(container =>
    {
        // Define el layout del PDF aquí: cabecera, tabla de productos, footer
        container.Page(page =>
        {
            page.Header().Text("Factura #" + purchase.Id);
            page.Content().Column(column =>
            {
                column.Item().Text("Detalles de la compra...");
                // ... tablas de productos, totales ...
            });
            page.Footer().Text(text => text.Span("Página ").CurrentPageNumber().Span(" de ").TotalPages());
        });
    });

    using var stream = new MemoryStream();
    document.GeneratePdf(stream); // Genera el PDF en un stream de memoria
    return stream.ToArray(); // Devuelve el PDF como un array de bytes
}
```
**Lección de Supervivencia**: Cuando necesites generar documentos complejos, busca librerías que trabajen directamente con el formato de destino (ej. PDF) en lugar de convertir un formato intermedio (HTML).

---

Este volumen te ha guiado por los retos de llevar tu aplicación a producción: desde cómo Docker la empaqueta de forma eficiente hasta cómo gestiona los datos que suben los usuarios y los documentos que genera el sistema. Eres ahora un arquitecto de operaciones.