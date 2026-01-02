# 01. La Forja de .NET: Arquitectura, Middlewares y DI (La Ultimate Master Edition)

Bienvenido al viaje al centro de la arquitectura de una aplicación .NET 10. Aquí no solo verás dónde van los archivos, sino *por qué* y *cómo* cada pieza encaja para construir un sistema robusto, escalable y mantenible. Prepárate para entender el cerebro de tu aplicación.

## 1. El Viaje Inesperado de una Petición (The Middleware Pipeline)

Imagina tu aplicación ASP.NET Core como una fábrica de coches. Cada estación de trabajo (Middleware) realiza una tarea específica en el coche (petición HTTP) antes de pasarlo a la siguiente. El orden es CRÍTICO:

```text
Usuario (Navegador)
       ↓ (Petición HTTP)
┌────────────────────────────────┐
│           Kestrel (Servidor Web)        │
└────────────────────────────────┘
       ↓
┌────────────────────────────────┐
│ 1. `app.UseExceptionHandler`   │  <- Si algo falla, el primero en capturarlo.
│                                │
│ 2. `app.UseHttpsRedirection`   │  <- ¿Es HTTP? Redirigir a HTTPS.
│                                │
│ 3. `app.UseStaticFiles`        │  <- ¿Es un CSS/JS/Imagen? Servir y FIN de la petición.
│                                │
│ 4. `app.UseRouting`            │  <- Mira la URL, ¿a dónde va esta petición?
│                                │
│ 5. `app.UseRequestLocalization`│  <- ¿En qué idioma quiere la web el usuario?
│                                │
│ 6. `app.UseAuthentication`     │  <- ¿Quién eres? (Lee la cookie de sesión).
│                                │
│ 7. `app.UseAuthorization`      │  <- ¿Tienes permiso para hacer esto?
│                                │
│ 8. `app.UseSession`            │  <- ¿Necesitamos datos temporales para este usuario?
│                                │
│ 9. `app.MapControllerRoute`    │  <- ¡Aquí está el Controlador! Ejecuta el código.
│                                │
│ 10. `app.MapRazorPages`        │  <- O quizás una Razor Page.
│                                │
│ 11. `app.MapBlazorHub`         │  <- O la conexión con Blazor Server.
└────────────────────────────────┘
       ↓ (Respuesta HTTP)
Usuario (Navegador)
```

**La Regla de Oro**: El orden de `app.Use...` importa MUCHO. Un `UseAuthentication` después de `MapControllerRoute` significa que tus controladores se ejecutarán antes de saber quién es el usuario.

## 3. Inyección de Dependencias (DI): El Contenedor Maestro de Objetos

La Inyección de Dependencias es el patrón estrella de .NET Core. Te permite construir aplicaciones "desacopladas". En lugar de que tus clases creen sus dependencias (`new MiServicio()`), las piden a un "Contenedor" (el sistema de DI).

### 3.1. ¿Por qué usar DI? (Beneficios Clave)
-   **Testabilidad**: Puedes "simular" dependencias (Mocks) en tus pruebas sin tocar la base de datos real.
-   **Mantenibilidad**: Si cambias la implementación de `IProductService` (ej. de SQL a NoSQL), solo modificas una línea en `Program.cs`.
-   **Extensibilidad**: Fácilmente puedes añadir nuevas funcionalidades sin modificar código existente (Open/Closed Principle).

### 3.2. Los 3 Tiempos de Vida (Lifetimes) - Una Metáfora Avanzada
Imagina el servidor como un gran centro comercial.

1.  **Transient (`AddTransient`)**:
    *   **Metáfora**: Un café desechable. Cada vez que entras a una tienda (una clase pide el servicio), te dan un café nuevo.
    *   **Comportamiento**: Se crea una nueva instancia del objeto cada vez que se pide.
    *   **Uso**: Servicios ligeros, sin estado, que no necesitan ser compartidos. Ejemplos: Procesadores de cálculos puntuales.

2.  **Scoped (`AddScoped`)**:
    *   **Metáfora**: Una pulsera para un evento. Te la dan al entrar (cuando llega una petición HTTP), y te la quitan al salir (cuando la petición termina). Mientras la tienes, puedes acceder a las atracciones (otros servicios) usando la misma pulsera.
    *   **Comportamiento**: Se crea una instancia por cada petición HTTP. Todos los objetos que pidan este servicio durante la misma petición recibirán la **misma instancia**.
    *   **Uso**: Es el ciclo de vida más común y seguro para la mayoría de los servicios de negocio, especialmente aquellos que interactúan con una base de datos (`ApplicationDbContext`). Asegura la coherencia transaccional.

3.  **Singleton (`AddSingleton`)**:
    *   **Metáfora**: El edificio del centro comercial. Solo hay uno.
    *   **Comportamiento**: Se crea una única instancia del objeto la primera vez que se pide (o al iniciar la aplicación) y se reutiliza para todas las peticiones y todos los usuarios durante toda la vida de la aplicación.
    *   **Uso**: Servicios de configuración, cachés globales, Background Services.
    *   **Peligro**: Si guardas datos específicos de un usuario en un Singleton, ¡todos los usuarios verán los datos del primero! Es crucial que los Singletons sean "thread-safe" y no mantengan estado por usuario.

```csharp
// Program.cs - Configuración de DI
builder.Services.AddScoped<IProductService, ProductService>(); // Servicio de negocio
builder.Services.AddSingleton<ITimeService, RealTimeService>(); // Ejemplo de Singleton
builder.Services.AddTransient<IRandomNumberGenerator, GuidRandomNumberGenerator>(); // Ejemplo de Transient
```

## 4. Constructores Primarios (C# 14): La Elegancia del Código Moderno

C# evoluciona. La sintaxis de los constructores primarios, disponible en .NET 8+ y plenamente adoptada en .NET 10/C# 14, reduce el "ruido" visual en tus clases.

**Antes (C# antiguo):**
```csharp
public class MyService : IMyService
{
    private readonly ILogger<MyService> _logger;
    private readonly IRepository _repository;

    public MyService(ILogger<MyService> logger, IRepository repository)
    {
        _logger = logger;
        _repository = repository;
    }

    public void DoSomething() { _logger.LogInformation("Doing something."); }
}
```

**Ahora (C# 14 con Constructores Primarios):**
```csharp
public class MyService(ILogger<MyService> logger, IRepository repository) : IMyService
{
    // 'logger' y 'repository' están automáticamente disponibles en toda la clase
    // No necesitas declarar los campos privados ni asignarlos en el constructor.
    public void DoSomething() { logger.LogInformation("Doing something."); }
}
```
**Beneficio**: Código más conciso, legible y menos propenso a errores (ej. olvidarse de asignar un campo). Es un sello de calidad en tu código.

## 5. ViewModels: El Escudo de Seguridad y la Flexibilidad del Diseño

Un "ViewModel" es un modelo diseñado específicamente para una vista (`.cshtml`).

### 5.1. ¿Por qué ViewModels y no el Modelo de Dominio directo?
1.  **Seguridad (Overposting)**: Imagina que tu `Product` tiene una propiedad `bool IsApproved` y un hacker la añade oculta a tu formulario. Si tu `ActionResult` recibe un `Product`, podría actualizar `IsApproved` sin que lo sepas. Con un `ProductViewModel` que NO tenga `IsApproved`, el ataque es imposible.
2.  **Abstracción de la UI**: La vista puede necesitar datos de 3 modelos diferentes (ej. `Product`, `User`, `Category`) o solo un subconjunto de propiedades. El ViewModel es una clase plana que agrupa SOLO lo que la vista necesita.
3.  **Validación Específica**: Las reglas de validación de la base de datos (Ej. Longitud máxima del texto) pueden ser diferentes de las reglas de la interfaz de usuario (Ej. Un campo es obligatorio en la UI pero opcional en la BD). Las `Data Annotations` se aplican en el ViewModel.

### 5.2. Ejemplo de Uso (`RegisterViewModel.cs`)
```csharp
// TiendaDawWeb.Web/ViewModels/RegisterViewModel.cs
public class RegisterViewModel
{
    [Required(ErrorMessage = "Validacion.EmailRequerido")]
    [EmailAddress(ErrorMessage = "Validacion.EmailInvalido")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Validacion.PasswordRequerido")]
    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "Validacion.PasswordLongitud", MinimumLength = 4)]
    public string Password { get; set; }

    [DataType(DataType.Password)]
    [Compare("Password", ErrorMessage = "Validacion.PasswordConfirmacion")]
    public string ConfirmPassword { get; set; }
    
    // ... otros campos como Nombre, Apellidos que la UI necesita
}
```

---

## 6. Patrón Result: Elegancia Funcional para la Gestión de Errores

Las excepciones (`throw`) son costosas en rendimiento y rompen el flujo de la aplicación. Para errores de negocio previsibles (ej. "Producto no encontrado", "Usuario sin permiso"), usamos el **Patrón Result** con la librería `CSharpFunctionalExtensions`.

### 6.1. ¿Cómo funciona?
Una función que puede fallar no devuelve `T` directamente, sino `Result<T, E>`, donde `T` es el valor de éxito y `E` es el valor de error.

**Ejemplo en un Servicio (`IProductService.cs`):**
```csharp
public interface IProductService
{
    // Devuelve un Producto o un DomainError
    Task<Result<Product, DomainError>> GetByIdAsync(long id);
    Task<Result<bool, DomainError>> DeleteAsync(long id, long userId, bool isAdmin = false);
}
```

**6.2. Uso en el Controlador (`ProductController.cs`):**
```csharp
// Mal: Usando excepciones
try {
    var product = await _productService.GetByIdAsync(id); // Podría lanzar ProductNotFoundException
    return View(product);
} catch (ProductNotFoundException) {
    return NotFound();
}

// Bien: Usando el Patrón Result
var result = await _productService.GetByIdAsync(id);
return result.Match(
    onSuccess: (product) => View(product),       // Si hay éxito, renderiza la vista
    onFailure: (error) => {                      // Si hay fallo, gestiona el error
        // 'error' es nuestro objeto DomainError tipado
        if (error.Code == ProductError.NotFound(id).Code) {
            return NotFound();
        }
        // ... otros tipos de errores
        return BadRequest(error.Message); // O un error genérico
    }
);
```
**Ventaja**: El compilador te obliga a gestionar ambos caminos (éxito y fallo). Esto hace tu código más robusto, predecible y fácil de depurar. El controlador no tiene que saber cómo se produce el error, solo cómo reaccionar a él.

---

## 7. Gestión de Mensajes Temporales (Flash Messages con TempData)

¿Cómo informar al usuario de que "El producto se creó correctamente" después de una redirección? Las variables normales se pierden tras el `return RedirectToAction()`.

### 7.1. `TempData` al Rescate
`TempData` es un diccionario que guarda datos en una cookie de sesión cifrada. La magia es que los datos solo sobreviven a **una redirección HTTP**. Después de leerse una vez, se eliminan automáticamente.

**Flujo de Ejemplo:**
1.  **Controlador (`ProductController.cs`)**:
    ```csharp
    [HttpPost]
    public IActionResult Create(ProductViewModel model)
    {
        // ... lógica para guardar el producto ...
        TempData["SuccessMessage"] = "¡El producto se ha creado con éxito!";
        return RedirectToAction("Index"); // Redirecciona a la lista de productos
    }
    ```
2.  **Vista (`_Notifications.cshtml` - Parcial Global)**:
    Este archivo se incluye en `_Layout.cshtml` y contiene la lógica para mostrar las alertas.
    ```razor
    @* TiendaDawWeb.Web/Views/Shared/_Notifications.cshtml *@
    @if (TempData["SuccessMessage"] is string successMessage)
    {
        <div class="alert alert-success alert-dismissible fade show" role="alert">
            @successMessage
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    }
    @if (TempData["ErrorMessage"] is string errorMessage)
    {
        <div class="alert alert-danger alert-dismissible fade show" role="alert">
            @errorMessage
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    }
    ```
**Beneficio**: Feedback claro y conciso al usuario sin lógica duplicada en cada vista.

---

## 8. Troubleshooting: El misterio del `ContentRoot` y `WebRoot`

Uno de los desafíos más sutiles en proyectos con estructuras complejas o al ejecutar desde diferentes ubicaciones es asegurar que ASP.NET Core sepa dónde están sus archivos.

### 8.1. `ContentRootPath` vs `WebRootPath`
-   **`ContentRootPath`**: Es el directorio base de tu aplicación. Aquí se buscan las vistas (`.cshtml`), `appsettings.json`, y las DLLs de tu proyecto.
-   **`WebRootPath`**: Es la carpeta donde se encuentran tus archivos estáticos (`wwwroot`). CSS, JavaScript, imágenes.

### 8.2. El Fallo al Ejecutar desde la Raíz de la Solución
Si ejecutas `dotnet run --project TiendaDawWeb.Web` desde la raíz de la solución (`TiendaDawWeb-NetCore`), el `ContentRootPath` por defecto será la raíz de la solución. Esto significa que `app.UseStaticFiles()` buscará `wwwroot` en la raíz de la solución (donde no está) y `Views` también fallará.

**La Solución Implementada en `Program.cs`**:
```csharp
// Lógica de detección inteligente al inicio de Program.cs
// Esta lógica asegura que el ContentRootPath y WebRootPath apunten a TiendaDawWeb.Web
// incluso si 'dotnet run' se ejecuta desde la raíz de la solución.
var currentDir = Directory.GetCurrentDirectory();
var isRoot = !Directory.Exists(Path.Combine(currentDir, "wwwroot")) && 
             Directory.Exists(Path.Combine(currentDir, "TiendaDawWeb.Web", "wwwroot"));

var contentRoot = isRoot ? Path.Combine(currentDir, "TiendaDawWeb.Web") : currentDir;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = "wwwroot" // Se asume que 'wwwroot' está dentro del ContentRootPath
});
// ...
// Si estamos en la raíz de la solución y 'wwwroot' no está ahí,
// pero sí en la subcarpeta 'TiendaDawWeb.Web', entonces ajustamos la WebRootPath del builder.
if (isRoot)
{
    builder.Environment.WebRootPath = Path.Combine(contentRoot, "wwwroot"); // Forzar la WebRootPath
    builder.WebHost.UseStaticWebAssets(); // Y activar los Static Web Assets para Blazor
}
```
**Lección de Supervivencia**: Siempre que tengas problemas con rutas (`View not found`, `404` en archivos estáticos), lo primero es verificar el `ContentRootPath` y `WebRootPath` de tu aplicación. El "troubleshooting" en .NET comienza con entender el entorno de ejecución.

---

Este volumen ha sentado las bases de la arquitectura y el flujo de la aplicación. Prepárate para el siguiente nivel.