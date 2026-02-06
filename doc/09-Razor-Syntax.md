# 9. Sintaxis Razor

## Índice

[9. Sintaxis Razor](#9-sintaxis-razor)
  - [9.1. Introducción](#91-introducción)
  - [9.2. Directivas](#92-directivas)
  - [9.3. Expresiones y Bloques](#93-expresiones-y-bloques)
  - [9.4. Condicionales](#94-condicionales)
  - [9.5. Iteraciones](#95-iteraciones)
  - [9.6. Inyección de Servicios](#96-inyección-de-servicios)
  - [9.7. Helpers y Componentes](#97-helpers-y-componentes)

---

## 9.1. Introducción

Razor es un motor de vistas que permite combinar HTML con código C#.

```html
@{
    var message = "Hola Mundo";
}

<h1>@message</h1>
<p>La fecha es: @DateTime.Now.ToString("dd/MM/yyyy")</p>
```

---

## 9.2. Directivas

| Directiva     | Propósito                                    |
| ------------ | ------------------------------------------ |
| `@page`      | Define la ruta de la página                 |
| `@model`     | Especifica el tipo del modelo               |
| `@using`    | Añade using al código                       |
| `@inject`    | Inyecta un servicio                         |
| `@section`  | Define una sección para el layout           |
| `@layout`   | Especifica el layout a usar                 |
| `@attribute`| Añade atributos a la clase                 |
| `@namespace`| Define el namespace                        |
| `@addTagHelper`| Añade Tag Helpers                        |
| `@removeTagHelper`| Elimina Tag Helpers                  |
| `@tagHelperPrefix`| Define prefijo para Tag Helpers        |

### Ejemplos de Directivas

```html
@page "/products"
@model ProductDetailModel
@using TiendaDawWeb.Shared.Models
@inject IProductService ProductService
@addTagHelper *, TiendaDawWeb.Mvc

@section Scripts {
    <script src="~/js/products.js"></script>
}
```

---

## 9.3. Expresiones y Bloques

### Expresiones

```html
<!-- Expresión simple -->
<h1>@Model.Product.Nombre</h1>

<!-- Expresión con método -->
<p>@DateTime.Now.ToString("dd/MM/yyyy")</p>

<!-- Operador ternario -->
<p>Estado: @(product.Stock > 0 ? "En stock" : "Agotado")</p>

<!-- Conversión explícita -->
@((Html.Raw("<strong>Texto en negrita</strong>")))
```

### Bloques de Código

```html
@{
    // Declaraciones de variables
    var title = "Mi Página";
    var today = DateTime.Now;
    
    // Funciones locales
    string FormatPrice(decimal price) => price.ToString("C");
    
    // Cálculos
    var discount = 0.1m;
    var finalPrice = product.Precio * (1 - discount);
}

<h1>@title</h1>
<p>Hoy es @today.ToString("dddd")</p>
<p>Precio final: @FormatPrice(finalPrice)</p>
```

---

## 9.4. Condicionales

### If/Else

```html
@if (Model.Product.Stock > 0)
{
    <span class="badge bg-success">En stock</span>
}
else if (Model.Product.Stock > 0 && Model.Product.Stock < 10)
{
    <span class="badge bg-warning">¡Últimas unidades!</span>
}
else
{
    <span class="badge bg-danger">Agotado</span>
}
```

### Switch

```html
@switch (order.Status)
{
    case OrderStatus.Pending:
        <span class="badge bg-warning">Pendiente</span>
        break;
    case OrderStatus.Processing:
        <span class="badge bg-info">Procesando</span>
        break;
    case OrderStatus.Shipped:
        <span class="badge bg-primary">Enviado</span>
        break;
    case OrderStatus.Delivered:
        <span class="badge bg-success">Entregado</span>
        break;
    default:
        <span class="badge bg-secondary">Desconocido</span>
        break;
}
```

---

## 9.5. Iteraciones

### Foreach

```html
@foreach (var product in Model.Products)
{
    <div class="product-card">
        <h3>@product.Nombre</h3>
        <p>@product.Precio.ToString("C")</p>
    </div>
}
```

### For

```html
@for (int i = 0; i < Model.Pages; i++)
{
    <a href="/page/@i" class="page-link">@(i + 1)</a>
}
```

### While

```html
@{ var index = 0; }
@while (index < Model.Products.Count)
{
    <div class="product">@Model.Products[index].Nombre</div>
    index++;
}
```

---

## 9.6. Inyección de Servicios

### @inject

```html
@page "/products"
@model ProductsModel
@inject IProductService ProductService
@inject IHtmlLocalizer<ProductsModel> Localizer

@{
    var products = await ProductService.GetAllAsync();
}

<h1>@Localizer["ProductsTitle"]</h1>

@foreach (var product in products)
{
    <div>@product.Nombre</div>
}
```

### @inject con Tipado Fuerte

```csharp
// En el PageModel
[Inject]
public IProductService ProductService { get; set; } = null!;

// En la vista
@inject TiendaDawWeb.Shared.Services.IProductService ProductService
```

---

## 9.7. Helpers y Componentes

### Helpers de Texto

```html
<!-- Truncar texto -->
@product.Descripcion.Substring(0, 100)...

<!-- Formatear fecha -->
@Model.Order.Date.ToString("dd/MM/yyyy HH:mm")

<!-- Formatear número -->
@Model.Product.Precio.ToString("N2") unidades
```

### Componentes Inline

```html
@{
    RenderFragment button = @<button class="btn btn-primary">Click me</button>;
    RenderFragment<string> badge = @<span class="badge">@context</span>;
}

@button
@badge("Mi Badge")
```

---

## Resumen

| Concepto           | Descripción                                              |
| ------------------ | -------------------------------------------------------- |
| `@`                | Carácter de escape para código C#                        |
| `@{}`              | Bloque de código C#                                      |
| `@Model`           | Referencia al modelo de la vista                         |
| `@inject`           | Inyección de dependencias en la vista                    |
| `@section`         | Sección para el layout                                   |

---

**Anterior**: [08. Object Mapping](../08-Object-Mapping.md)  
**Próximo**: [10. Internacionalización (I18n)](../10-I18n.md)
