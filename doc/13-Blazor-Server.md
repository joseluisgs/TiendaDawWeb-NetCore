# 13. Blazor Server Basics: La Magia de C# en el Navegador

## Índice

[13. Blazor Server Basics: La Magia de C# en el Navegador](#13-blazor-server-basics-la-magia-de-c-en-el-navegador)
  - [13.1. El Túnel de SignalR](#131-el-túnel-de-signalr)
  - [13.2. Ciclo de Vida de un Componente](#132-ciclo-de-vida-de-un-componente)
  - [13.3. Estado y Binding](#133-estado-y-binding)
  - [13.4. Integración con Razor](#134-integración-con-razor)
  - [13.5. Gráficos con Blazor-ApexCharts](#135-gráficos-con-blazor-apexcharts)
  - [13.6. AdminStatsWidget: Métricas en Tiempo Real](#136-adminstatswidget-métricas-en-tiempo-real)

---

## 13.1. El Túnel de SignalR

Blazor Server permite ejecutar C# en el servidor que controla el HTML del navegador.

### Flujo de Comunicación

```mermaid
sequenceDiagram
    participant B as Navegador
    participant S as Servidor
    participant C as Componente Blazor
    participant DB as Base Datos
    
    B->>S: Conexión SignalR
    S->>B: Circuit establecido
    
    B->>S: Clic en botón
    S->>C: Ejecutar @onclick
    C->>DB: Consultar datos
    DB-->>C: Datos
    C->>C: Calcular cambios
    C->>S: Enviar diff binario
    S->>B: Actualizar DOM
```

### Características del Túnel

| Aspecto       | Descripción                 |
| ------------- | --------------------------- |
| **Protocolo** | SignalR (WebSockets)        |
| **Mensajes**  | Diff binario (solo cambios)  |
| **Latencia**  | Muy baja (tiempo real)       |
| **Estado**    | Mantenido en el servidor    |

---

## 13.2. Ciclo de Vida de un Componente

### Métodos del Ciclo de Vida

```csharp
protected override void OnInitialized()
{
    // Se ejecuta cuando el componente recibe parámetros
}

protected override void OnParametersSet()
{
    // Se ejecuta después de recibir parámetros
    // Ideal para cargar datos iniciales
}

protected override void OnAfterRender(bool firstRender)
{
    // Se ejecuta después del renderizado
    // Ideal para JS interop
}

public void Dispose()
{
    // Limpieza de recursos
}
```

### Métodos Asíncronos

```csharp
protected override async Task OnInitializedAsync()
{
    // Se ejecuta una vez al inicializar
    // Ideal para carga de datos
    products = await LoadProductsAsync();
}

protected override async Task OnParametersSetAsync()
{
    // Se ejecuta cada vez que cambian los parámetros
    if (ProductId != lastProductId)
    {
        lastProductId = ProductId;
        await LoadProductDetailsAsync();
    }
}

protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender)
    {
        // Solo la primera vez
        await InitializeJsInterop();
    }
}
```

---

## 13.3. Estado y Binding

### One-Way Binding

```html
<h1>@Title</h1>
<p>El precio es: @Product.Price</p>
<div class="category">@Category?.Name</div>
```

### Two-Way Binding

```html
<input @bind="searchTerm" />
<input @bind="searchTerm" @bind:format="dd/MM/yyyy" />
<textarea @bind="description" />

@code {
    private string searchTerm = "";
    private string description = "";
}
```

### Event Handling

```html
<button @onclick="HandleClick">Clic aquí</button>
<button @onclick="() => HandleAction(id)">Eliminar</button>
<input @oninput="OnInputChanged" />
<select @onchange="OnSelectionChanged">
    <option value="1">Opción 1</option>
</select>

@code {
    private void HandleClick()
    {
        // Manejar clic
    }
    
    private void HandleAction(long id)
    {
        // Manejar acción con parámetro
    }
    
    private void OnInputChanged(ChangeEventArgs e)
    {
        searchTerm = e.Value?.ToString();
    }
}
```

---

## 13.4. Integración con Razor

### Componente Embebido en Razor

```html
@page "/products/{id:long}"

<h1>Detalles del Producto</h1>

<RatingSummary ProductId="id" />
<RatingSection ProductId="id" />
```

### Componente Razor en Blazor

```csharp
// En un componente Blazor
@inject HttpClient Http

<div>
    @if (products == null)
    {
        <p>Cargando...</p>
    }
    else
    {
        <ul>
            @foreach (var product in products)
            {
                <li>@product.Nombre</li>
            }
        </ul>
    }
</div>

@code {
    private List<ProductDto>? products;
    
    protected override async Task OnInitializedAsync()
    {
        products = await Http.GetFromJsonAsync<List<ProductDto>>("/api/products");
    }
}
```

---

## 13.5. Gráficos con Blazor-ApexCharts

Blazor-ApexCharts es un wrapper de ApexCharts.js para Blazor que permite crear gráficos interactivos y visualizaciones de datos en tiempo real.

### ¿Qué aporta Blazor-ApexCharts?

| Característica | Descripción |
| --------------- | ------------ |
| **Gráficos interactivos** | Líneas, barras, áreas, tartas, donuts, heatmaps |
| **Tiempo real** | Actualización automática cada X segundos |
| **Toolbars** | Zoom, pan, download, reset integrados |
| **Personalización** | Colores, animaciones, themes via código C# |
| **Sin JS manual** | Todo desde componentes Blazor |

### Componentes del Proyecto

El proyecto usa Blazor-ApexCharts en el dashboard de administración:

| Componente | Tipo | Propósito |
|------------|------|-----------|
| **VentasMensualesChart** | Líneas + Barras | Evolución de ventas y pedidos mensuales |
| **VentasCategoriasChart** | Donut | Distribución de ventas por categoría |
| **TopParticipantesChart** | Barras horizontales | Top 10 compradores/vendedores conmutable |

### Actualización en Tiempo Real

Los gráficos se actualizan automáticamente cada 30 segundos usando un Timer en el servidor, manteniendo la conexión SignalR activa para mostrar datos vivos en el dashboard de admin.

### Comparativa: Gráficos Estáticos vs Tiempo Real

| Aspecto        | Estático (Razor)       | Tiempo Real (Blazor)              |
| --------------- | ---------------------- | -------------------------------- |
| **Actualización** | Manual (F5)           | Automática (Timer)               |
| **Interacción**  | Limitada              | Zoom, pan, tooltips              |
| **Casos uso**    | Reports, exports      | Dashboards, monitoring           |

---

## 13.6. AdminStatsWidget: Métricas en Tiempo Real

El AdminStatsWidget es un componente Blazor que muestra las métricas clave del negocio en tiempo real.

### ¿Qué aporta?

| Característica | Descripción |
| --------------- | ------------ |
| **KPI críticos** | Usuarios, productos y ventas en una sola vista |
| **Tiempo real** | Actualización automática cada 30 segundos |
| **Interactivo** | Botón de refresh manual |
| **Sin gráficos** | Solo números (más ligero que ApexCharts) |

### Funcionalidad del Componente

El widget muestra 3 métricas fundamentales:

| Métrica | Descripción |
|---------|------------|
| **Usuarios Registrados** | Total de usuarios en el sistema |
| **Productos Activos** | Productos no eliminados disponibles |
| **Ventas Totales** | Total de compras/procesadas |

### Actualización

Al igual que los gráficos, usa un Timer para auto-refresh cada 30 segundos, consultando:
- `UserManager.Users.CountAsync()` → Usuarios
- `ProductService.GetAllAsync()` → Productos
- `PurchaseService.GetAllAsync()` → Ventas

La hora de última actualización se muestra para saber cuándo se actualizaron los datos.

---

## Resumen

### ¿Qué aporta Blazor Server en una página Razor?

Blazor Server permite enriquecer páginas Razor tradicionales con componentes interactivos C# que se ejecutan en el servidor pero se renderizan en el navegador, todo sin escribir JavaScript.

| Ventaja | Descripción |
|---------|-------------|
| **Single Language Stack** | Todo en C# (backend y frontend) |
| **Tiempo Real** | Actualización automática sin recargar (SignalR) |
| **Componentes Reutilizables** | RatingSummary, RatingSection, Gráficos, Widgets |
| **Integración Progressive** | Se puede usar solo donde se necesita |
| **Menor JS** | Lógica compleja en C# en lugar de JavaScript |

### Casos de Uso en el Proyecto

| Componente | Tipo | Cuándo usar |
|------------|------|-------------|
| **RatingSummary** | Solo lectura | Mostrar valoraciones en página producto |
| **RatingSection** | Interactivo | Formulario para valorar compra |
| **AdminStatsWidget** | KPI | Dashboard admin |
| **Gráficos (ApexCharts)** | Visualización | Dashboard con datos evolutivos |

### Conceptos Clave

| Concepto           | Descripción                                              |
| ------------------ | -------------------------------------------------------- |
| **SignalR**        | Túnel de comunicación entre cliente y servidor          |
| **Ciclo de Vida** | OnInitialized → OnParametersSet → OnAfterRender         |
| **One-Way**       | `@variable`                                             |
| **Two-Way**       | `@bind="variable"`                                       |
| **Eventos**       | `@onclick="handler"`                                     |
| **Timer**         | Actualización periódica (gráficos, widgets)             |

---

**Anterior**: [12. Razor vs AJAX vs Blazor](../12-BlazorVsRazor.md)  
**Próximo**: [14. State Container](../14-Blazor-Comm.md)
