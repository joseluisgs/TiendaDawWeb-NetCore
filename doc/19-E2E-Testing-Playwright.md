# 13 - Pruebas de Extremo a Extremo (E2E) con Playwright (Deep Dive)

En este volumen profundizamos en el uso de **Playwright**, la herramienta de automatización más potente del mercado, integrada nativamente en nuestra solución **.NET 10**. Aprenderás cómo configurar el entorno, dominar los selectores y testear flujos complejos de Blazor Server.

---

## 1. Configuración y Filosofía

A diferencia de otras herramientas, Playwright no inyecta código en la app; la controla desde fuera mediante protocolos de depuración.

### Requisitos de Instalación
Tras compilar el proyecto de tests, es vital descargar los motores de renderizado:
```bash
dotnet build
npx playwright install chromium --with-deps
```

### Configuración del Contexto (`BrowserNewContextOptions`)
En Playwright, cada test corre en un **Browser Context** (una sesión de incógnito aislada). Podemos personalizarla sobrescribiendo el método `ContextOptions()` en nuestras clases de test:

```csharp
public override BrowserNewContextOptions ContextOptions()
{
    return new BrowserNewContextOptions
    {
        Locale = "es-ES",             // Fuerza el formato de fechas y moneda (€)
        TimezoneId = "Europe/Madrid", // Sincroniza horas con el servidor
        ViewportSize = new() { Width = 1280, Height = 720 },
        AcceptDownloads = true        // Necesario para testear facturas PDF
    };
}
```

---

## 2. El Arte de los Selectores (Locators)

Playwright promueve el uso de **selectores orientados al usuario** (Accesibilidad) en lugar de clases CSS frágiles que cambian con el diseño.

### Mejores Prácticas:
1.  **`GetByRole`**: El más robusto. Busca por la función del elemento (botón, enlace, etc.).
    ```csharp
    await Page.GetByRole(AriaRole.Button, new() { Name = "Iniciar Sesión" }).ClickAsync();
    ```
2.  **`GetByPlaceholder` / `GetByLabel`**: Ideal para formularios.
    ```csharp
    await Page.GetByPlaceholder("tu@email.com").FillAsync("user@test.com");
    ```
3.  **`Locator` con CSS/Text**: Para casos específicos.
    ```csharp
    await Page.Locator(".card").GetByText("Ver Detalle").First.ClickAsync();
    ```

### 🚨 La Regla del Modo Estricto (Strict Mode)
Si un selector devuelve más de un elemento, Playwright lanzará un error para evitar ambigüedad. 
**Solución**: Refinar el selector o usar `.First` / `.Nth(index)`.

---

### 2.1 data-testid: El Superpoder del Mantenimiento de Tests

**¿Por qué usar `data-testid` en lugar de selectores CSS/ID?**

En proyectos reales, los selectores CSS tradicionales (`.card-body form button[type='submit']`) son **frágiles**. Cuando un diseñador cambia las clases CSS o refactoriza el HTML, **tus tests se rompen sin que el código de negocio falle**.

**`data-testid`** es un atributo dedicado exclusivamente para testing:

| Enfoque | Ejemplo | Problema |
|---------|---------|----------|
| CSS frágil | `.card-body form button[type='submit']` | Se rompe si cambias clases |
| ID HTML | `#Email` | Funciona, pero no semántico |
| **data-testid** | `data-testid="email-input"` | **Inmutable**, solo para tests |

#### La Función de Extensión

Centralizamos el acceso a `data-testid` con un método de extensión limpio:

```csharp
// Extensions/PlaywrightExtensions.cs
public static class PlaywrightExtensions
{
    /// <summary>
    /// Busca un elemento por su atributo data-testid en la página
    /// </summary>
    public static ILocator TestId(this IPage page, string id) 
        => page.GetByTestId(id);

    /// <summary>
    /// Busca un elemento por su atributo data-testid dentro de un locator
    /// </summary>
    public static ILocator TestId(this ILocator locator, string id) 
        => locator.GetByTestId(id);
}
```

#### Ejemplo de Uso

**Antes (selectores frágil):**
```csharp
await Page.FillAsync("#Email", "admin@waladaw.com");
await Page.FillAsync("#Password", "admin");
await Page.ClickAsync(".card-body form button[type='submit']");
```

**Después (con data-testid y extensión):**
```csharp
await Page.TestId("email-input").FillAsync("admin@waladaw.com");
await Page.TestId("password-input").FillAsync("admin");
await Page.TestId("submit-button").ClickAsync();
```

**Encadenamiento para formularios complejos:**
```csharp
// Busca el formulario, luego busca el input dentro de él
await Page.TestId("login-form").TestId("email-input").FillAsync("admin@waladaw.com");
```

#### Beneficios de Esta Estrategia

1. **Tests más legibles**: `TestId("email-input")` es autoexplicativo
2. **Resistente a refactorización**: Cambiar clases CSS no rompe tests
3. **Separación de concerns**: El atributo `data-testid` está en la vista, no en el test
4. **Documentación viva**: Los `data-testid` sirven como documentación de elementos clave de UI
5. **Debugging fácil**: Buscas en HTML por `data-testid` y encuentras el elemento

#### Convenciones de Nombrado

| Patrón | Ejemplo | Uso |
|--------|---------|-----|
| `{elemento}-{tipo}` | `email-input`, `password-input` | Campos de formulario |
| `{elemento}-{acción}` | `submit-button`, `search-button` | Botones |
| `{sección}-{elemento}` | `login-form`, `cart-items` | Contenedores |
| `{propósito}` | `product-title`, `seller-info` | Elementos clave |

#### ¿Dónde añadir `data-testid`?

No en todos los elementos, solo en los **claves para testing**:

```html
<!-- ✅ SÍ - elementos complejos o dinámicos -->
<input asp-for="Email" data-testid="email-input" />
<form data-testid="login-form">...</form>
<button data-testid="submit-button">...</button>

<!-- ❌ NO - elementos simples y estables -->
<div class="card">...</div>        <!-- clase CSS suficiente -->
<span>@producto.Precio</span>        <!-- texto visible estable -->
```


## 3. Aserciones Inteligentes (Web First Assertions)

Playwright incluye un motor de re-intento automático en sus aserciones. Si un elemento tarda 2 segundos en aparecer por una llamada AJAX, el test esperará automáticamente antes de fallar.

```csharp
// El test no falla inmediatamente; espera hasta 5s (por defecto) a que el texto aparezca
await Expect(Page.Locator(".navbar")).ToContainTextAsync("Bienvenido");

// Negación robusta
await Expect(Page.GetByText("Cargando...")).Not.ToBeVisibleAsync();
```

---

## 4. Testeando Blazor Server e Interactividad AJAX

Blazor Server mantiene un túnel SignalR abierto. Playwright es capaz de detectar cuándo el DOM cambia tras un evento de C# en el navegador.

### Sincronización en Valoraciones (Ratings):
```csharp
// 1. Clic en un componente Blazor (C# procesa el evento en el servidor)
await Page.Locator(".star-item").Nth(3).ClickAsync();

// 2. Playwright detecta el cambio de estado en el DOM instantáneamente
await Expect(Page.Locator(".toast-body")).ToBeVisibleAsync();
```

### Gestión de Descargas (Facturas PDF):
Para validar que el servicio `IPdfService` genera un archivo real:
```csharp
var download = await Page.RunAndWaitForDownloadAsync(async () =>
{
    await Page.GetByText("Factura").First.ClickAsync();
});
Assert.That(download.SuggestedFilename, Does.EndWith(".pdf"));
```

---

## 5. Estrategias de Supervivencia en Windows

### Evitar el Bloqueo de SQLite (`database is locked`)
Dado que usamos **SQLite In-Memory**, si lanzamos tests en paralelo, varios navegadores intentarán escribir en la misma RAM. 
**Solución**: Forzamos la ejecución secuencial en `AssemblyInfo.cs`:
```csharp
[assembly: LevelOfParallelism(1)]
```

### Depuración Visual: Trace Viewer
Si un test falla en CI/CD, Playwright puede grabar una traza completa. Puedes inspeccionarla con:
```bash
npx playwright show-trace path/to/trace.zip
```
Permite ver el DOM, la red y la consola en cada milisegundo del test.

---

## 6. Videos y Screenshots por Test

Nuestra clase base `E2ETestBase` configura automáticamente la grabación de video y captura de pantallas:

```csharp
public abstract class E2ETestBase : PageTest
{
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            RecordVideoDir = "TestVideos",
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid"
        };
    }

    protected async Task CaptureScreenshotAsync(string stepName)
    {
        var path = $"TestScreenshots/{TestContext.CurrentContext.Test.Name}/{stepName}.png";
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        await Page.ScreenshotAsync(new() { Path = path, FullPage = true });
    }
}
```

**Estructura de archivos generada:**
```
TestScreenshots/
└── AuthTests/
    └── AdminLogin_ShouldSucceed/
        ├── 01-login-page-loaded.png
        ├── 02-validation-errors.png
        └── 03-credentials-filled.png

TestVideos/
└── PurchaseIntentTests.SearchSpecificProduct_AndVerifySeller_ShouldShowPurchaseOption.webm
```

Esto permite:
- Debugging visual instantáneo al fallar un test
- Documentación de flujos de usuario
- Videos para presentaciones o reportes

---

## 7. Conclusión Maestro

Los tests E2E son tu **seguro de vida**. Mientras los tests unitarios te dicen que el código es correcto, Playwright te dice que **el usuario puede comprar**. Mantén tus selectores legibles, tus datos aislados y tus aserciones orientadas a la UI.