# 03. Razor Masterclass: El Arte de Pintar con C# (La Piel de tu App)

Razor no es solo un lenguaje de plantillas; es un potente motor de renderizado que compila tu HTML con C# en clases ejecutables de .NET. Esto te da seguridad de tipos y un rendimiento excepcional.

## 1. La Sintaxis Fundamental de Razor (El Lenguaje Secreto)

Todo lo que empieza por `@` es código C#. Todo lo demás es HTML.

### 1.1. Directivas Clave:
-   **`@model TipoDeDatos`**: Define el tipo de dato que esta vista espera recibir del controlador. Es la base de las "vistas fuertemente tipadas". Ejemplo: `@model ProductViewModel`.
-   **`@inject TipoDeServicio NombreVariable`**: Permite inyectar directamente servicios del contenedor de DI en tu vista. Ideal para localizadores de idioma (`IStringLocalizer`) o gestores de carritos.
    ```razor
    @inject IStringLocalizer<SharedResource> Localizer
    <h1>@Localizer["TituloBienvenida"]</h1>
    ```
-   **`@{ ... }`**: Un bloque de código C# que se ejecuta en el servidor. Puedes declarar variables, llamar métodos, etc.
    ```razor
    @{
        ViewData["Title"] = "Mi Página"; // Establece el título de la página
        var fechaActual = DateTime.Now;
    }
    <p>La fecha es: @fechaActual.ToShortDateString()</p>
    ```
-   **`@(...)`**: Para incrustar una expresión C# directamente en el HTML.
    ```razor
    <p>El precio es: @(Model.Precio * 1.21)</p>
    ```
    Si es una variable simple o propiedad, el paréntesis es opcional: `<p>@Model.Nombre</p>`.

---

## 2. Tag Helpers: HTML con Superpoderes .NET

Los Tag Helpers son atributos especiales que empiezan por `asp-`. Se procesan en el servidor antes de enviar el HTML al navegador, generando código HTML estándar. Son la forma más segura y productiva de construir interfaces de usuario en ASP.NET Core MVC.

### 2.1. Navegación Inteligente (`asp-controller`, `asp-action`, `asp-route-...`)
```razor
@* Antes (HTML "tonto"): <a href="/Product/Edit?id=5">Editar</a> *@
@* Ahora (HTML "inteligente"): *@
<a asp-controller="Product" asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-primary">
    Editar Producto
</a>
```
**Ventaja**: Si cambias el nombre de tu controlador o acción, o la estructura de la URL en `Program.cs`, el Tag Helper generará la URL correcta automáticamente. Esto evita enlaces rotos.

### 2.2. Formularios Dinámicos (`asp-for`, `asp-validation-for`)
Estos son los Tag Helpers más potentes para construir formularios.

```razor
<form asp-controller="Product" asp-action="Create" method="post">
    <div class="mb-3">
        <label asp-for="Nombre" class="form-label"></label>
        <input asp-for="Nombre" class="form-control" />
        <span asp-validation-for="Nombre" class="text-danger"></span>
    </div>
    <button type="submit" class="btn btn-success">Guardar</button>
</form>
```

**Magia detrás de las escenas:**
-   **`<label asp-for="Nombre"></label>`**: Genera `<label for="Nombre">Nombre</label>` y localiza el texto "Nombre" si está definido en los recursos.
-   **`<input asp-for="Nombre" />`**:
    -   Genera `id="Nombre"` y `name="Nombre"` (crucial para que el Model Binding funcione).
    -   Establece el `value` actual del campo si ya existe (`Model.Nombre`).
    -   Añade atributos `data-val-*` (ej. `data-val-required="true"`) que activan la validación en cliente.
-   **`<span asp-validation-for="Nombre"></span>`**: Genera un `<span>` que, si hay un error de validación para el campo `Nombre`, mostrará el mensaje en rojo.

---

## 3. Estrategia de Triple Validación: Blindando tus Formularios

La validación es tu primera línea de defensa contra datos erróneos o maliciosos. Un "Superdios" valida en tres niveles.

1.  **Capa 1: Atributos del Modelo (C# - La Regla)**
    Define las reglas de negocio en tu `ViewModel` o `Model` usando `Data Annotations`.
    ```csharp
    // TiendaDawWeb.Web/ViewModels/ProductViewModel.cs
    [Required(ErrorMessage = "Validacion.NombreRequerido")] // Mensaje localizado
    [StringLength(100, MinimumLength = 3, ErrorMessage = "Validacion.NombreLongitud")]
    public string Nombre { get; set; }

    [Range(0.01, 99999.99, ErrorMessage = "Validacion.PrecioRango")]
    [Display(Name = "Etiqueta.Precio", ResourceType = typeof(SharedResource))] // Para que la etiqueta se traduzca
    public decimal Precio { get; set; }
    ```
    *Lección*: Estos atributos son el "diccionario de reglas" que Razor y el Model Binding entenderán.

2.  **Capa 2: Validación en Cliente (JavaScript - La UX)**
    Permite al usuario corregir errores antes de enviar el formulario.
    -   **Cómo activarla**: Incluyendo la vista parcial `_ValidationScriptsPartial.cshtml` al final de tu layout o vista.
    -   **Funcionamiento**: Usa JavaScript (jQuery Validation) para leer los atributos `data-val-*` generados por los Tag Helpers y mostrar mensajes de error instantáneamente.
    -   **Ventaja**: Ahorra recursos del servidor y mejora la experiencia de usuario.

3.  **Capa 3: Validación en Servidor (C# - La Seguridad ABSOLUTA)**
    **NUNCA confíes en el cliente**. Un usuario malintencionado puede desactivar JavaScript.
    -   **Cómo activarla**: El Model Binding ejecuta automáticamente las `Data Annotations`.
    -   **Cómo usarla**: Dentro de tu acción `[HttpPost]`:
        ```csharp
        [HttpPost]
        public IActionResult Create(ProductViewModel model)
        {
            if (!ModelState.IsValid) // Si hay UN solo error en el modelo
            {
                // Regresa la vista con los mensajes de error pintados
                // Los <span asp-validation-for> se encargarán de mostrarlos.
                return View(model); 
            }
            // Si ModelState.IsValid es true, podemos procesar los datos
            // ... llamar al servicio ...
            return RedirectToAction("Index");
        }
        ```
    -   **Ventaja**: Es tu último muro de defensa y el más importante.

---

## 4. Layouts, Secciones y Partials: Estructura HTML Reutilizable

-   **`_Layout.cshtml`**: Es la plantilla maestra de tu web. Contiene el HTML base (HEAD, navegación, footer) que se repite en todas las páginas. Usa `@RenderBody()` para indicar dónde debe inyectarse el contenido de la vista específica.
-   **`@RenderSection("Scripts", required: false)`**: Permite que una vista específica inyecte contenido (normalmente JavaScript) en una parte definida del layout. `required: false` significa que no todas las vistas tienen que proporcionar este script.
-   **Partials (`_Navbar.cshtml`, `_Notifications.cshtml`)**: Pequeños trozos de HTML que puedes reutilizar en múltiples vistas. Son ideales para componentes de UI que no tienen mucha lógica (si la tuvieran, usaríamos ViewComponents).

---

## 5. El archivo `_ViewImports.cshtml`: El Cerebro de las Vistas

Este archivo actúa como el `using` global para todas tus vistas.

```razor
@using TiendaDawWeb // Namespace base del proyecto
@using TiendaDawWeb.Models // Para usar clases como Product sin el namespace completo
@using TiendaDawWeb.ViewModels // Para usar ViewModels
@using TiendaDawWeb.Models.Enums // Para acceder a enums como ProductCategory
@using Microsoft.AspNetCore.Identity // Para UserManager, SignInManager
@using System.Globalization // Para formatear fechas, monedas, etc.
@using System.Security.Claims // Para acceder a los Claims del usuario

@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers // Habilita los Tag Helpers oficiales de ASP.NET Core MVC
@addTagHelper *, TiendaDawWeb // Habilita Tag Helpers personalizados si los tuvieras en este proyecto (ej. si ProductViewModel fuera un TagHelper)

@* Nota: Los Tag Helpers de Blazor se gestionan con el mismo <component> TagHelper,
   pero el namespace del componente Blazor se registra en _Imports.razor *//
```
**Lección Clave**: Si tu `@model` no se reconoce, o tus `asp-for` no funcionan, el 90% de las veces es un error en el `_ViewImports.cshtml`. Asegúrate de que todos los namespaces que uses en tus vistas estén aquí.

---

## 6. Gestión de Mensajes Temporales (Flash Messages con `TempData`)

Informar al usuario después de una acción (ej. "¡Producto borrado con éxito!") es crucial para la UX. Pero si haces un `RedirectToAction`, el estado de tu `ViewData` o `ViewBag` se pierde.

### 6.1. ¿Qué es `TempData`?
`TempData` es un diccionario que guarda datos entre dos peticiones HTTP. Utiliza una cookie (o el sistema de sesión) para almacenar un mensaje que solo sobrevive a **una redirección**. Una vez leído, se elimina automáticamente.

### 6.2. Implementación en el Proyecto (`_Notifications.cshtml`)
1.  **En el Controlador**:
    ```csharp
    [HttpPost]
    public IActionResult Delete(int id)
    {
        // ... lógica para borrar ...
        TempData["SuccessMessage"] = "Producto eliminado con éxito.";
        return RedirectToAction("Index"); // La clave es la redirección
    }
    ```
2.  **En el `_Layout.cshtml`**: Incluimos el partial `_Notifications.cshtml` al inicio de nuestro `<main>` para que las notificaciones aparezcan en todas las páginas.
    ```html
    <main role="main" class="container mt-8">
        <partial name="_Notifications" />
        @RenderBody()
    </main>
    ```
3.  **Contenido de `_Notifications.cshtml`**:
    ```razor
    @* TiendaDawWeb.Web/Views/Shared/_Notifications.cshtml *@
    @if (TempData["SuccessMessage"] is string successMessage)
    {
        <div class="alert alert-success alert-dismissible fade show mt-3" role="alert">
            @successMessage
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    }
    @if (TempData["ErrorMessage"] is string errorMessage)
    {
        <div class="alert alert-danger alert-dismissible fade show mt-3" role="alert">
            @errorMessage
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
        </div>
    }
    @* Puedes añadir más tipos de mensajes: Warning, Info, etc. *@
    ```
**Beneficio**: Proporciona un feedback elegante y no intrusivo al usuario, manteniendo la lógica de mensajes centralizada y desacoplada de cada acción del controlador.

---

Este volumen ha desvelado los secretos de la interfaz de usuario en ASP.NET Core, desde cómo Razor interpreta tu código hasta las estrategias de validación y notificación. Es tu lienzo para construir experiencias de usuario excepcionales.