# 03. Razor Masterclass: El Arte de Dibujar con C#

Razor es un motor de renderizado que compila tu código HTML+C# en clases de alto rendimiento.

## 1. Directivas Maestras
- **`@model`**: Establece el contrato de datos para la vista. Siempre intenta que tus vistas sean fuertemente tipadas.
- **`@inject`**: Inyecta servicios directamente en la vista. Útil para diccionarios de idiomas (`IStringLocalizer`) o gestores de carritos.
- **`@section`**: Permite inyectar código en el Layout desde la vista hija. Ideal para cargar un JS que solo se usa en una página específica.

## 2. Tag Helpers: Los Superpoderes de Razor
No escribas URLs a mano. Usa:
- `asp-controller` / `asp-action`: Construyen la URL dinámicamente. Si cambias el nombre de la ruta, los enlaces se actualizan solos.
- `asp-for`: El Tag Helper definitivo. Genera `id`, `name`, `type` y atributos de validación leyendo las propiedades de tu clase C#.

## 3. Mensajes Temporales (Flash Messages)
Cuando redireccionas (`RedirectToAction`), las variables de la página se pierden. Usamos **`TempData`**.
- Se guarda en una cookie temporal.
- Solo sobrevive a una redirección.
- Se procesa en el archivo `_Notifications.cshtml` mediante un bucle que genera alertas de Bootstrap de forma elegante.

## 4. ViewImports y ViewStart
- **`_ViewStart.cshtml`**: El archivo que le dice a todas las vistas: "Tu padre es `_Layout.cshtml`".
- **`_ViewImports.cshtml`**: El registro global. Registra aquí tus namespaces y TagHelpers una sola vez para que funcionen en todas las páginas.
