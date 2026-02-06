# 5. Razor Pages: Desarrollo Web con ASP.NET Core

## Índice

[5. Razor Pages: Desarrollo Web con ASP.NET Core](#5-razor-pages-desarrollo-web-con-aspnet-core)
  - [5.1. Introducción a Razor Pages](#51-introducción-a-razor-pages)
  - [5.2. Estructura de una Razor Page](#52-estructura-de-una-razor-page)
  - [5.3. Modelo de Página (PageModel)](#53-modelo-de-página-pagemodel)
  - [5.4. Sintaxis Razor](#54-sintaxis-razor)
  - [5.5. Routing en Razor Pages](#55-routing-en-razor-pages)
  - [5.6. Tag Helpers](#56-tag-helpers)
  - [5.7. Layouts y Secciones](#57-layouts-y-secciones)
  - [5.8. Componentes Parciales](#58-componentes-parciales)
  - [5.9. Validación de Formularios](#59-validación-de-formularios)
  - [5.10. Resumen](#510-resumen)

---

## 5.1. Introducción a Razor Pages

**Razor Pages** es un modelo de programación basado en páginas que simplifica el desarrollo web ASP.NET Core. Cada página es autocontenida con su lógica de presentación y comportamiento, ideal para escenarios donde el código de cada página es relativamente independiente.

```mermaid
flowchart TB
    subgraph "ASP.NET Core MVC"
        M1[Models]
        V1[Views]
        C1[Controllers]
        M1 --> V1
        C1 --> V1
    end
    
    subgraph "Razor Pages"
        P1[Page.cshtml]
        P2[PageModel.cshtml.cs]
        P2 --> P1
    end
    
    P1 -.-> "1:1" -.-> C1 & V1
```

### Comparación: MVC vs Razor Pages

| Aspecto                  | MVC                              | Razor Pages                    |
| ------------------------ | -------------------------------- | ------------------------------ |
| **Unidad básica**         | Controller + Vista               | Página (.cshtml)              |
| **Organización**          | Por funcionalidad (Controllers) | Por página                    |
| **Modelo**               | ViewModels compartidos           | PageModel específico por página|
| **Routing**              | Atributos en Controllers         | Convenciones de carpeta        |
| **Best for**             | APIs, SPAs consumiendo datos    | Apps web tradicionales        |
| **Curva de aprendizaje** | Media                            | Baja                          |

### Cuándo Usar Razor Pages

| ✅ Usar Razor Pages cuando... | ❌ Usar MVC cuando...                   |
| --------------------------- | --------------------------------------- |
| Páginas relativamente independientes | Lógica compartida entre páginas      |
| Equipo pequeño-mediano      | Equipo grande con especialización     |
| Prototipado rápido          | Aplicaciones con muchas APIs           |
| Apps web tradicionales      | SPAs consumiendo datos                 |
| SEO es importante            | Lógica de presentación muy compleja    |

---

## 5.2. Estructura de una Razor Page

Una Razor Page consta de dos archivos:

```
Pages/
├── Products/
│   ├── Index.cshtml          # Vista (HTML + Razor)
│   └── Index.cshtml.cs       # PageModel (Code-behind)
├── Shared/
│   └── _Layout.cshtml        # Layout principal
└── _ViewImports.cshtml       # Imports globales
```

### Archivo .cshtml (Vista)

```html
@page "/products"
@model TiendaDawWeb.RazorPages.Pages.Products.IndexModel
@{
    ViewData["Title"] = "Productos";
}

<h1>@ViewData["Title"]</h1>

@if (Model.Products?.Any() == true)
{
    <div class="row">
        @foreach (var product in Model.Products)
        {
            <div class="col-md-4">
                <partial name="_ProductCard" model="product" />
            </div>
        }
    </div>
}
else
{
    <p>No hay productos disponibles.</p>
}
```

### Archivo .cshtml.cs (PageModel)

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TiendaDawWeb.Shared.Services;

namespace TiendaDawWeb.RazorPages.Pages.Products;

[Authorize]
public class IndexModel : PageModel
{
    private readonly IProductService _productService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(
        IProductService productService,
        ILogger<IndexModel> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    public IEnumerable<ProductDto> Products { get; set; } = Enumerable.Empty<ProductDto>();

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public int PageNumber { get; set; } = 1;

    public async Task OnGetAsync()
    {
        _logger.LogInformation("Cargando productos - Búsqueda: {Search}", Search);
        
        var result = await _productService.GetAllAsync(Search, PageNumber);
        
        Products = result.Match(
            onSuccess: products => products,
            onFailure: error => {
                _logger.LogError("Error: {Message}", error);
                TempData["Error"] = error;
                return Enumerable.Empty<ProductDto>();
            }
        );
    }
}
```

---

## 5.3. Modelo de Página (PageModel)

### Propiedades del PageModel

| Propiedad                  | Descripción                                      |
| -------------------------- | ------------------------------------------------ |
| `PageContext`              | Información de la página actual                  |
| `ModelState`               | Estado de validación del modelo                  |
| `TempData`                 | Datos temporales entre peticiones                 |
| `HttpContext`              | Contexto HTTP completo                           |
| `Url`                      | Utilidades de URL                                |
| `User`                     | Usuario autenticado actual                        |
| `Request`                  | Petición HTTP actual                             |
| `Response`                | Respuesta HTTP                                   |

### Métodos del PageModel

| Método              | Descripción                                      |
| ------------------- | ------------------------------------------------ |
| `OnGet()`           | Se ejecuta en peticiones GET                     |
| `OnGetAsync()`      | Versión asíncrona de OnGet                      |
| `OnPost()`          | Se ejecuta en peticiones POST                    |
| `OnPostAsync()`     | Versión asíncrona de OnPost                     |
| `OnPostHandlerAsync()` | Handlers POST nombrados                        |
| `Page()`            | Renderiza la página actual                       |
| `Redirect()`        | Redirección HTTP                                |
| `RedirectToPage()`  | Redirección a otra página                        |

### Handlers Múltiples

```csharp
public class ProductModel : PageModel
{
    [BindProperty]
    public ProductDto Product { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(long id)
    {
        var result = await _service.GetByIdAsync(id);
        return result.Match(
            onSuccess: p => {
                Product = p;
                return Page();
            },
            onFailure: error => NotFound(error)
        );
    }

    public async Task<IActionResult> OnPostSaveAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var result = await _service.UpdateAsync(Product);
        return result.Match(
            onSuccess: () => RedirectToPage("Index"),
            onFailure: error => {
                ModelState.AddModelError(string.Empty, error);
                return Page();
            }
        );
    }

    public async Task<IActionResult> OnPostDeleteAsync(long id)
    {
        var result = await _service.DeleteAsync(id);
        return result.Match(
            onSuccess: () => RedirectToPage("Index"),
            onFailure: error => {
                TempData["Error"] = error;
                return RedirectToPage("Index");
            }
        );
    }
}
```

---

## 5.4. Sintaxis Razor

### Expresiones

```html
<!-- Expresión simple -->
<h1>@Model.Product.Nombre</h1>
<p>Precio: @Model.Product.Precio.ToString("C")</p>

<!-- Expresión compleja -->
@{ var total = Model.Items.Sum(i => i.Precio); }
<p>Total: @total</p>

<!-- Condicional -->
@if (Model.Product.EnStock)
{
    <span class="badge bg-success">En Stock</span>
}
else
{
    <span class="badge bg-danger">Agotado</span>
}

<!-- Iteración -->
@foreach (var item in Model.CartItems)
{
    <div class="cart-item">
        @item.Nombre - @item.Precio.ToString("C")
    </div>
}

<!-- Switch -->
@switch (Model.Order.Status)
{
    case OrderStatus.Pending:
        <span class="badge bg-warning">Pendiente</span>
        break;
    case OrderStatus.Shipped:
        <span class="badge bg-info">Enviado</span>
        break;
    case OrderStatus.Delivered:
        <span class="badge bg-success">Entregado</span>
        break;
}
```

### Directivas

```html
@page "/ruta/de/pagina"
@model Namespace.PageModel
@namespace MiEspacio.Namespaces
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
@removeTagHelper *, Ensamblado
@tagHelperPrefix "tp:"
@using TiendaDawWeb.Shared.Models
@inject IProductService ProductService
@attribute [Authorize]
@functions {
    public string GetFormattedPrice(decimal price) => price.ToString("C");
}
```

---

## 5.5. Routing en Razor Pages

### Convenciones de Ruta

| Archivo                          | Ruta por Defecto                  |
| -------------------------------- | -------------------------------- |
| `Pages/Index.cshtml`             | `/` o `/Index`                   |
| `Pages/Products/Index.cshtml`    | `/Products` o `/Products/Index`   |
| `Pages/Products/Details.cshtml`  | `/Products/Details`               |
| `Pages/Products/Create.cshtml`   | `/Products/Create`                |

### Ruta Personalizada

```html
@page "/productos/{id:long}"
@page "/p/{slug}"
```

### Parámetros de Ruta

```csharp
public class DetailsModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public long Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Slug { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        // /Products/Details/5 → Id = 5
        // /Products/p/mi-producto → Slug = mi-producto
    }
}
```

### Routing Personalizado en Program.cs

```csharp
builder.Services.AddRazorPages(options =>
{
    options.Conventions.Add(
        new PageRouteTransformerConvention(
            new SlugifyParameterTransformer()));

    options.Conventions.AuthorizePage("/Products/Create");
    options.Conventions.AllowAnonymousToPage("/Products/Index");
});
```

---

## 5.6. Tag Helpers

### Tag Helpers Incorporados

```html
<!-- Formularios -->
<form asp-controller="Products" asp-action="Create" method="post">
    <input asp-for="Product.Nombre" class="form-control" />
    <span asp-validation-for="Product.Nombre" class="text-danger"></span>
</form>

<!-- Links -->
<a asp-page="/Products/Details" asp-route-id="@product.Id">Ver</a>
<a asp-page="/Products/Edit" asp-route-id="@product.Id">Editar</a>
<a asp-controller="Home" asp-action="Index">Inicio</a>

<!-- Entornos -->
<environment include="Development">
    <link rel="stylesheet" href="debug.css" />
</environment>
<environment exclude="Development">
    <link rel="stylesheet" href="prod.css" />
</environment>

<!-- Caché -->
<cache expires-after="TimeSpan.FromMinutes(5)">
    @await Component.InvokeAsync("FeaturedProducts")
</cache>

<!-- Componentes -->
@await Component.InvokeAsync("ProductCategory", new { categoryId = 1 })
```

### Custom Tag Helpers

```csharp
[HtmlTargetElement("rating", Attributes = "max, value")]
public class RatingTagHelper : TagHelper
{
    [HtmlAttributeName("max")]
    public int Max { get; set; } = 5;

    [HtmlAttributeName("value")]
    public double Value { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "div";
        output.Attributes.SetAttribute("class", "rating");
        output.Content.SetHtmlContent(
            $"<span>{Value}/{Max}</span>"
        );
    }
}
```

---

## 5.7. Layouts y Secciones

### Layout Principal

```html
<!DOCTYPE html>
<html>
<head>
    <title>@ViewData["Title"] - Tienda</title>
    <link rel="stylesheet" href="~/css/site.css" />
</head>
<body>
    <header>
        <nav>@await Html.PartialAsync("_NavBar")</nav>
    </header>

    <main>
        @RenderBody()
    </main>

    <footer>
        <p>&copy; @DateTime.Now.Year - TiendaDaw</p>
    </footer>

    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

### Usar Layout

```html
@page "/products"
@layout _ProductLayout
@{
    ViewData["Title"] = "Productos";
}
```

### Secciones

```html
<!-- En la página -->
@section Scripts {
    <script src="~/js/products.js"></script>
    <script>
        initProductsPage();
    </script>
}

<!-- En el Layout -->
@RenderSection("Scripts", required: false)
```

### ViewData y TempData en Layout

```html
<!-- Layout -->
@if (TempData.TryGetValue("SuccessMessage", out var success))
{
    <div class="alert alert-success">@success</div>
}

<main>
    @RenderBody()
</main>
```

---

## 5.8. Componentes Parciales

### Crear Componente Parcial

```html
<!-- Pages/Shared/_ProductCard.cshtml -->
@model ProductDto

<div class="card product-card">
    <img src="@Model.ImagenUrl" class="card-img-top" alt="@Model.Nombre" />
    <div class="card-body">
        <h5 class="card-title">@Model.Nombre</h5>
        <p class="card-text">@Model.Descripcion</p>
        <p class="price">@Model.Precio.ToString("C")</p>
        <a asp-page="/Products/Details" asp-route-id="@Model.Id" 
           class="btn btn-primary">Ver detalles</a>
    </div>
</div>
```

### Usar Componente Parcial

```html
<!-- Renderización con partial -->
<partial name="_ProductCard" model="product" />

<!-- Renderización asíncrona -->
@await Html.PartialAsync("_ProductCard", product)

<!-- Con opciones -->
<partial name="_ProductCard" model="product" view-data="ViewData" />
```

### View Components

```csharp
public class CartSummaryViewComponent : ViewComponent
{
    private readonly ICartService _cartService;

    public CartSummaryViewComponent(ICartService cartService)
    {
        _cartService = cartService;
    }

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var count = await _cartService.GetItemCountAsync();
        return View(new CartSummaryViewModel
        {
            ItemCount = count
        });
    }
}
```

---

## 5.9. Validación de Formularios

### Validación en PageModel

```csharp
public class CreateProductModel : PageModel
{
    [BindProperty]
    public CreateProductDto Product { get; set; } = new();

    [BindProperty]
    public IFormFile? Imagen { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Product.Precio <= 0)
        {
            yield return new ValidationResult(
                "El precio debe ser mayor que cero",
                nameof(Product.Precio));
        }
    }
}
```

### Mostrar Errores

```html
<form method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>

    <div class="form-group">
        <label asp-for="Product.Nombre"></label>
        <input asp-for="Product.Nombre" class="form-control" />
        <span asp-validation-for="Product.Nombre" class="text-danger"></span>
    </div>

    <div class="form-group">
        <label asp-for="Product.Precio"></label>
        <input asp-for="Product.Precio" class="form-control" />
        <span asp-validation-for="Product.Precio" class="text-danger"></span>
    </div>

    <button type="submit" class="btn btn-primary">Crear</button>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

---

## 5.10. Resumen

| Concepto                  | Descripción                                              |
| ------------------------- | ------------------------------------------------------- |
| **PageModel**             | Clase code-behind con lógica de la página               |
| **@page**                 | Directiva que define la ruta                             |
| **@model**                | Tipo del modelo de la página                            |
| **OnGet/OnPost**          | Handlers de petición                                    |
| **Tag Helpers**           | Atributos que generan HTML                              |
| **Layout**                | Plantilla base con `@RenderBody()`                      |
| **Secciones**             | `@section` y `@RenderSectionAsync()`                    |
| **Partial Views**         | Fragmentos reutilizables                               |
| **View Components**       | Componentes reutilizables con lógica                   |

---

**Anterior**: [04. MVC Controllers](../04-MVC-Controllers.md)  
**Próximo**: [06. Persistencia de Datos](../06-Persistence.md)
