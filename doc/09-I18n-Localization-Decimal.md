# 06. La Torre de Babel Conquistada: I18n y Localización (Ultimate Master Edition)

Hacer una web multi-idioma no es solo traducir palabras. Es entender que cada cultura tiene sus propias reglas para fechas, números y monedas. Es una danza compleja entre el servidor, el cliente y el lenguaje.

## 1. El Diccionario Global: Archivos de Recursos (.resx)

Los archivos `.resx` son tus diccionarios de traducción. Contienen pares clave-valor para cada idioma.

### 1.1. Organización Profesional de los `.resx`:
-   **`SharedResource.es.resx`**: Contiene traducciones comunes (Login, Home, Footer, etiquetas genéricas).
-   **`Messages.es.resx`**: Contiene traducciones específicas de mensajes de error o validaciones.
-   **`TiendaDawWeb.Web/Resources/`**: Es la carpeta donde se almacenan.

### 1.2. Clases Generadas Automáticamente:
Cuando compilas, .NET genera internamente clases C# a partir de estos `.resx`. Por ejemplo, `SharedResource.es.resx` se convierte en una clase que te permite acceder a las traducciones: `Localizer["Clave"]`.

### 1.3. Uso en el Código:
-   **En Vistas (`.cshtml`)**:
    ```razor
    @inject IStringLocalizer<SharedResource> Localizer // Inyecta el "diccionario"
    <h1>@Localizer["TituloBienvenida"]</h1> // Accede a la traducción
    ```
-   **En Controladores/Servicios**:
    ```csharp
    // Constructor
    public ProductController(IStringLocalizer<SharedResource> localizer) { ... }
    // En un método
    var mensaje = _localizer["ProductoNoEncontrado"];
    ```

---

## 2. Configuración del Middleware de Localización (`Program.cs`)

El Middleware de Localización es crucial para que ASP.NET Core sepa qué idioma debe usar.

### 2.1. Registro de Servicios:
```csharp
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources"); // Indica dónde están los .resx

builder.Services.AddControllersWithViews()
    .AddViewLocalization() // Habilita la localización para vistas Razor
    .AddDataAnnotationsLocalization(); // Habilita la localización para los atributos de validación
```

### 2.2. El Orden de los Middleware es Vital (¡La Danza Cultural!)
```csharp
var supportedCultures = new[] { 
    new CultureInfo("es-ES"), new CultureInfo("en-US"), 
    // ... otros idiomas
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("es-ES"), // Idioma por defecto
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures,
    // Proveedores: Cómo detectamos el idioma del usuario
    RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new QueryStringRequestCultureProvider(), // Ejemplo: ?lang=en o ?culture=fr
        new CookieRequestCultureProvider(),      // Guarda la preferencia del usuario en una cookie
        new AcceptLanguageHeaderRequestCultureProvider() // Lee la preferencia del navegador
    }
});
```
**Lección de Supervivencia**: El middleware `app.UseRequestLocalization` debe ir **ANTES** de `app.UseRouting()`, `app.UseAuthentication()` y `app.UseAuthorization()`. Si no, la aplicación intentará procesar la ruta o autenticar al usuario antes de saber en qué idioma está, pudiendo causar errores inesperados o mensajes incorrectos.

---

## 3. El Desafío Cultural: Comas, Puntos y Separadores Decimales

Este es un problema clásico que rompe formularios y frustra a los usuarios.
-   **`es-ES`**: `1.000,50 €` (punto para miles, coma para decimales).
-   **`en-US`**: `$1,000.50` (coma para miles, punto para decimales).

### 3.1. El Problema en el `Model Binding`:
Cuando un usuario en España envía "10,50" en un formulario y el servidor está configurado para la cultura `en-US` (o no la detecta bien), ASP.NET Core intenta parsear "10,50" como un número americano, lo que resultará en:
-   Un valor `0`.
-   Un error de validación.
-   Un `FormatException`.

### 3.2. La Solución Definitiva: `DecimalModelBinder` (El "Traductor de Números")

Hemos implementado un `DecimalModelBinder` personalizado en `/Binders`.

**¿Cómo funciona?**
1.  **Intercepta**: Antes de que el Model Binder por defecto actúe, el nuestro toma el control.
2.  **Limpia**: Elimina símbolos de moneda, espacios y otros caracteres no numéricos.
3.  **Detecta Cultura**: Utiliza `CultureInfo.CurrentCulture` (que ya ha sido establecida por el middleware de localización) para saber si debe esperar una coma o un punto como separador decimal.
4.  **Parseo Seguro**: Convierte el texto limpio a un `decimal` de forma segura.

```csharp
// TiendaDawWeb.Web/Binders/DecimalModelBinder.cs
public class DecimalModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
        if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;

        var value = valueProviderResult.FirstValue; // Obtenemos el texto del formulario (ej. "10,50")

        // Aquí se limpia y se parsea usando la cultura del hilo actual
        // ... (lógica de limpieza y decimal.TryParse) ...
        
        bindingContext.Result = ModelBindingResult.Success(parsedValue);
        return Task.CompletedTask;
    }
}
```

### 3.3. Activación del `DecimalModelBinder` (en `Program.cs`):
```csharp
builder.Services.AddControllersWithViews(options =>
{
    // ¡CRÍTICO! Insertamos nuestro binder al principio de la lista.
    // Esto asegura que se ejecute ANTES que los binders por defecto de .NET.
    options.ModelBinderProviders.Insert(0, new DecimalModelBinderProvider());
})
// ... otras configuraciones de localización ...
```
**Lección de Supervivencia**: Cualquier entrada numérica de usuario, especialmente en un entorno global, es una fuente potencial de errores si no se manejan las diferencias culturales de forma explícita. Este `DecimalModelBinder` es un ejemplo perfecto de cómo solucionar un problema real de internacionalización.

---

## 4. El Conflicto con la Etiqueta `<base href>` (Lección de HTML y Blazor)

Este fue un problema que descubrimos en vivo en el proyecto.
-   **`Blazor Server`** requiere la etiqueta `<base href="~/" />` en el `<head>` del HTML. Esta etiqueta le dice al navegador cuál es la "raíz" de la aplicación para que todas las URLs relativas (especialmente las de SignalR) se resuelvan correctamente.
-   **`Selectores de Idioma en el Navbar`**: En nuestro `_Navbar.cshtml`, los enlaces para cambiar de idioma eran del tipo `<a href="?lang=es">Español</a>`.

### 4.1. El Problema:
Cuando el navegador ve `<base href="~/" />`, y estás en la URL `/Products/Details/5`, al pulsar `?lang=es`, el navegador lo interpreta como: `/Details/5?lang=es` (es decir, mantiene el path de la base). **Esto provoca que la URL se construya mal o que el cambio de idioma no se aplique porque no se redirige correctamente.**

### 4.2. La Solución Magistral: Enlaces de Idioma con Rutas Absolutas Dinámicas
Modificamos los enlaces en `_Navbar.cshtml` para que siempre incluyan la ruta completa de la página actual antes de añadir el parámetro de idioma:
```razor
<li>
    <a class="dropdown-item @(currentCulture == "es" ? "active" : "")" 
       href="@(Context.Request.Path)?lang=es">Español</a>
</li>
```
**`Context.Request.Path`**: Esto devuelve la ruta actual de la petición (ej. `/Admin/Dashboard` o `/Products/Details/5`). Al añadirle `?lang=es`, el navegador envía la petición correcta al servidor, y el middleware de localización puede actuar.

**Lección de Supervivencia**: La etiqueta `<base>` puede simplificar la gestión de rutas en Single Page Applications (SPAs) como Blazor, pero puede introducir conflictos con los enlaces HTML tradicionales que usan rutas relativas. Siempre ten esto en cuenta cuando integres SPAs en aplicaciones MVC existentes.

---

Este volumen te ha proporcionado una comprensión profunda de cómo se gestiona la internacionalización en ASP.NET Core, desde la traducción de textos hasta la delicada danza de los formatos numéricos y la resolución de conflictos con otras tecnologías. Eres ahora un experto en hacer que tu aplicación hable todos los idiomas.