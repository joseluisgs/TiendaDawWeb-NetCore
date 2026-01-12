# 04. Blazor Server: La Magia de C# en el Navegador (Real-Time y Híbrido)

Blazor Server es el "killer app" para desarrolladores .NET que quieren interactividad web sin tocar JavaScript. Permite que tu código C# se ejecute en el servidor, pero controle y actualice el HTML del navegador en tiempo real a través de un túnel de comunicación **SignalR** (WebSockets).

## 1. El Túnel de SignalR: El Cerebro de Blazor Server

Imagina que tu navegador y el servidor están conectados por un "cable invisible".
- Cuando haces clic en un botón Blazor, tu navegador envía un pequeño mensaje por ese cable.
- El servidor ejecuta el código C# asociado.
- El servidor calcula qué partes EXACTAS del HTML deben cambiar.
- Envía solo esos cambios (un "diff" binario) de vuelta al navegador por el mismo cable.
- Tu navegador actualiza el DOM sin recargar la página. ¡Todo esto con C#!

---

## 2. El Apocalipsis del Error 404 en `blazor.server.js` (Lecciones de Supervivencia)

Este fue uno de los problemas más persistentes. El navegador no encontraba `blazor.server.js`.

### 2.1. ¿Por qué `blazor.server.js` es tan especial?
-   **No es un archivo físico**: No lo encontrarás en tu carpeta `wwwroot`. Es un recurso **virtual**, embebido dentro de las librerías del framework de .NET.
-   **Servido por el Framework**: ASP.NET Core tiene una lógica interna (`Static Web Assets`) para exponer estos archivos virtuales a través de rutas como `_framework/blazor.server.js`.

### 2.2. La Causa Raíz del 404 en el Proyecto
Al mover el proyecto a una subcarpeta (`TiendaDawWeb.Web`), el servidor se "confundía" sobre su `ContentRootPath` y `WebRootPath` si se ejecutaba desde la raíz de la solución. Esto impedía que el sistema de "Static Web Assets" se inicializara correctamente.

### 2.3. La Solución Magistral (en `Program.cs`):
```csharp
// TiendaDawWeb.Web/Program.cs
// Esta lógica asegura que el ContentRootPath y WebRootPath apunten a TiendaDawWeb.Web
// incluso si 'dotnet run' se ejecuta desde la raíz de la solución.
var currentDir = Directory.GetCurrentDirectory();
var isRoot = !Directory.Exists(Path.Combine(currentDir, "wwwroot")) && 
             Directory.Exists(Path.Combine(currentDir, "TiendaDawWeb.Web", "wwwroot"));

var contentRoot = isRoot ? Path.Combine(currentDir, "TiendaDawWeb.Web") : currentDir;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot, // Se establece el ContentRoot correcto
    WebRootPath = "wwwroot"       // La carpeta 'wwwroot' dentro del ContentRoot
});

// ¡LA CLAVE! Activa la carga de los archivos estáticos de las librerías
// Sin esta línea, los JS de Blazor (y otros) no se encuentran y dan 404.
builder.WebHost.UseStaticWebAssets(); 
```
**Lección de Supervivencia**: Cuando tienes problemas de 404 con archivos del framework (como `blazor.server.js`) en un proyecto con subcarpetas, es una señal de que el `ContentRootPath` no está bien configurado o que `UseStaticWebAssets()` no se está llamando correctamente.

---

## 3. Integración Híbrida: Las "Islas de Blazor" en tu Web MVC

En **WalaDaw**, no toda la web es Blazor. Usamos MVC para el catálogo de productos y una "Isla de Blazor" para el widget de estadísticas del Administrador.

### 3.1. Requisitos para Inyectar Blazor en Razor:
1.  **En tu `_Layout.cshtml` (el Maestro del HTML):**
    *   **Etiqueta `<base href="~/" />`**: Es *obligatoria*. Le dice a Blazor cuál es la raíz de tu aplicación para que pueda construir correctamente las URLs para el hub de SignalR. Sin ella, Blazor se "desorienta" y la conexión puede fallar.
    *   **Script de Blazor**: Incluye `<script src="~/_framework/blazor.server.js"></script>` justo antes del cierre de `</body>`. Este script es el "agente" en el navegador que establece y mantiene la conexión SignalR.
2.  **En `Program.cs` (el Configurador del Servidor):**
    *   **Registro de Servicios**: `builder.Services.AddServerSideBlazor().AddCircuitOptions(options => { options.DetailedErrors = true; });` registra los servicios necesarios para Blazor Server. `DetailedErrors = true` es vital para depuración.
    *   **Mapeo del Hub**: `app.MapBlazorHub();` configura el endpoint `/_blazor` que usa SignalR para comunicarse con el cliente.
3.  **En la Vista Razor (`Admin/Index.cshtml`):**
    *   **Tag Helper `<component>`**: Para incrustar el componente Blazor.
        ```razor
        <component type="typeof(AdminStatsWidget)" render-mode="ServerPrerendered" />
        ```
        *   **`type="typeof(AdminStatsWidget)"`**: Indica qué componente Blazor quieres renderizar.
        *   **`render-mode="ServerPrerendered"`**:
            *   **Ventaja**: El servidor genera el HTML inicial del componente y lo envía al navegador (rápido para el usuario). Luego, el JavaScript de Blazor lo "hidrata" (lo hace interactivo).
            *   **Lección Aprendida**: Si la conexión JS falla, el componente aparece como una "foto estática" (el HTML prerenderizado pero sin interactividad). Para depurar, a veces cambiamos a `render-mode="Server"` (no prerenderizado) para que si la conexión falla, la zona quede en blanco, indicando un problema de conexión.

---

## 4. El Componente Blazor: `AdminStatsWidget.razor` (C# en Acción)

### 4.1. Inyección de Dependencias
Dentro de un componente Blazor, puedes inyectar servicios del contenedor de DI igual que en un controlador o servicio:
```razor
@inject IProductService ProductService // Inyecta el servicio de productos
@inject UserManager<User> UserManager    // Para gestionar usuarios
```
Esto permite al componente acceder directamente a la lógica de negocio y datos de tu aplicación.

### 4.2. El Ciclo de Vida del Componente (Momentos Clave)
-   **`OnInitializedAsync()`**: Se ejecuta al inicializarse el componente. Es el lugar ideal para cargar datos por primera vez.
    ```csharp
    protected override async Task OnInitializedAsync()
    {
        Logger.LogInformation("Blazor: AdminStatsWidget inicializado");
        await LoadData(); // Carga los datos iniciales
        // Configura el timer aquí
    }
    ```
-   **`StateHasChanged()`**: Es el corazón de la reactividad. Cada vez que cambias una variable que afecta a la UI, debes llamar a `StateHasChanged()` para que Blazor sepa que debe repintar el HTML.
-   **`IDisposable` y `Dispose()`**:
    *   Los componentes Blazor Server viven en la memoria del servidor. Si el componente usa recursos (como un `System.Timers.Timer`), y el usuario navega a otra página, el componente es destruido en el servidor.
    *   Si no liberas esos recursos en `Dispose()`, tendrás una "fuga de memoria" (`Memory Leak`).
    ```csharp
    public void Dispose()
    {
        Logger.LogInformation("Blazor: AdminStatsWidget eliminado (Dispose)");
        _timer?.Dispose(); // Libera el timer para que no siga consumiendo recursos
    }
    ```
    **Lección de Supervivencia**: Si tu aplicación con Blazor empieza a consumir mucha RAM en el servidor con el tiempo, investiga si tienes recursos (`Timer`, suscripciones a eventos) que no se están liberando en `Dispose()`.

---

## 5. El Desafío del Multi-Hilo: Hilos y `InvokeAsync`

Nuestro widget de estadísticas usa un `System.Timers.Timer` para actualizarse automáticamente cada 15 segundos.

### 5.1. El Problema del Hilo Incorrecto
Los `System.Timers.Timer` ejecutan su evento `Elapsed` en un hilo secundario (de background). La interfaz de usuario de Blazor solo puede ser actualizada desde el "hilo principal" (Dispatcher) de Blazor. Si intentas actualizarla directamente desde el Timer, obtendrás un error: *"The current thread is not associated with the Dispatcher"*.

### 5.2. La Solución: `InvokeAsync`
```csharp
_timer.Elapsed += async (sender, e) => await InvokeAsync(AutoRefresh);

private async Task AutoRefresh()
{
    Logger.LogInformation("Blazor: Refresco AUTOMÁTICO por Timer");
    await LoadData();
    StateHasChanged(); // Forzar el renderizado desde el hilo de la UI
}
```
`InvokeAsync()` es un método que te permite ejecutar código en el hilo de la UI de Blazor de forma segura, incluso si lo llamas desde otro hilo. Es esencial para cualquier lógica asíncrona o de background que necesite interactuar con la interfaz.

---

## 6. Integración Blazor y JavaScript (Interoperabilidad JS)

Aunque Blazor te permite hacer mucho con C#, a veces necesitas interactuar con librerías JavaScript o código JS existente.

### 6.1. Llamar a JS desde C#
Puedes inyectar `IJSRuntime` en tu componente y usarlo para llamar a funciones JavaScript globales.
```csharp
@inject IJSRuntime JSRuntime;

// Dentro de un método C#
await JSRuntime.InvokeVoidAsync("myJavaScriptFunction", "param1", "param2");
```

### 6.2. Llamar a C# desde JS
También puedes exponer métodos de C# a JavaScript, lo que permite que tu JS existente interactúe con la lógica del backend de Blazor.

---

Este volumen te ha proporcionado una inmersión profunda en Blazor Server, cubriendo desde su configuración básica hasta la resolución de problemas avanzados de archivos estáticos y la gestión de la concurrencia. Ahora eres un experto en hacer que C# cobre vida en el navegador.