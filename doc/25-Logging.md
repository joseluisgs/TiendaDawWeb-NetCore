# 25. Logging y Trazabilidad en WalaDaw

## Índice

[25. Logging y Trazabilidad en WalaDaw](#25-logging-y-trazabilidad-en-waladaw)
  - [25.1. Introducción al Logging](#251-introducción-al-logging)
  - [25.2. Logging en ASP.NET Core](#252-logging-en-aspnet-core)
  - [25.3. Configuración de Niveles de Log](#253-configuración-de-niveles-de-log)
  - [25.4. Logging en Servicios](#254-logging-en-servicios)
  - [25.5. Logging en Controllers](#255-logging-en-controllers)
  - [25.6. Correlación de Peticiones](#256-correlación-de-peticiones)
  - [25.7. Buenas Prácticas](#257-buenas-prácticas)

---

## 25.1. Introducción al Logging

El **logging** es el proceso de registrar eventos, errores y información relevante durante la ejecución de una aplicación.

| Objetivo              | Descripción                                           |
| -------------------- | ---------------------------------------------------- |
| **Debugging**        | Identificar y resolver errores en producción           |
| **Auditoría**        | Registrar acciones de usuarios (login, compras, etc.) |
| **Rendimiento**      | Monitorizar tiempos de respuesta y cuellos de botella |
| **Seguridad**        | Detectar intentos de acceso no autorizado             |
| **Trazabilidad**     | Seguir el flujo de una petición a través del sistema  |

---

## 25.2. Logging en ASP.NET Core

### Niveles de Log

| Nivel     | Uso                                              | Prioridad |
| --------- | ------------------------------------------------ | ---------- |
| **Trace** | Detalles muy específicos                          | 0          |
| **Debug** | Información para debugging                       | 1          |
| **Info**  | Eventos generales de la aplicación               | 2          |
| **Warning** | Situaciones anómalas pero no críticas          | 3          |
| **Error** | Errores que no paran la app                      | 4          |
| **Critical** | Errores graves que paran la app                 | 5          |

### Inyección de Logger

```csharp
public class ProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository repository,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
}
```

---

## 25.3. Configuración de Niveles de Log

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning",
      "TiendaDawWeb": "Debug"
    }
  }
}
```

### Console Provider

```csharp
// Program.cs
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);
```

### Debug Provider

```csharp
builder.Logging.AddDebug();
```

---

## 25.4. Logging en Servicios

```csharp
public class ProductService
{
    private readonly IProductRepository _repository;
    private readonly ILogger<ProductService> _logger;

    public async Task<ProductDto?> GetByIdAsync(long id)
    {
        _logger.LogInformation("Obteniendo producto {ProductId}", id);
        
        try
        {
            var product = await _repository.GetByIdAsync(id);
            
            if (product == null)
            {
                _logger.LogWarning("Producto {ProductId} no encontrado", id);
                return null;
            }
            
            _logger.LogDebug("Producto {ProductId} encontrado: {ProductName}", 
                id, product.Nombre);
            
            return product.ToDto();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto {ProductId}", id);
            throw;
        }
    }
}
```

---

## 25.5. Logging en Controllers

```csharp
public class ProductsController : Controller
{
    private readonly IProductService _service;
    private readonly ILogger<ProductsController> _logger;

    [HttpGet("{id}")]
    public async Task<IActionResult> Details(long id)
    {
        _logger.LogInformation("Petición GET para producto {ProductId}", id);
        
        var result = await _service.GetByIdAsync(id);
        
        return result.Match(
            onSuccess: product => {
                _logger.LogInformation("Producto {ProductId} mostrado correctamente", id);
                return View(product);
            },
            onFailure: error => {
                _logger.LogWarning("Producto {ProductId} no encontrado: {Error}", id, error);
                return NotFound(error);
            }
        );
    }
}
```

---

## 25.6. Correlación de Peticiones

### Correlation ID Middleware

```csharp
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Response.Headers["X-Correlation-ID"] = correlationId;
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
```

### Uso del Correlation ID

```csharp
// Incluir en todos los logs
_logger.LogInformation("Petición procesada");

// Incluir en excepciones
_logger.LogError(ex, "Error durante la petición {CorrelationId}", correlationId);
```

---

## 25.7. Buenas Prácticas

| Práctica                          | Descripción                                      |
| -------------------------------- | ------------------------------------------------ |
| **No loggear datos sensibles**    | Passwords, tokens, datos personales               |
| **Usar el nivel correcto**       | Error para excepciones, Info para eventos        |
| **Incluir contexto**             | IDs de usuario, petición, operación              |
| **No sobre-loggear**             | Evitar logs excesivos que dificulten el análisis |
| **Estructurar los logs**         | Usar scopes y propiedades estructuradas           |

---

## Resumen

| Concepto           | Descripción                                              |
| ------------------ | -------------------------------------------------------- |
| **ILogger**        | Interface de logging en ASP.NET Core                     |
| **Niveles**        | Trace, Debug, Info, Warning, Error, Critical           |
| **Scopes**         | Contexto compartido entre logs                          |
| **Correlation ID** | Identificador único para seguir una petición             |

---

**Anterior**: [24. Docker y Producción](../24-Docker.md)  
**Próximo**: [26. Clean Architecture](../26-Infrastructure.md)
