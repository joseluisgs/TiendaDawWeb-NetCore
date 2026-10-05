# 28. Blazor-ApexCharts: Integración y Solución de Problemas

## Índice

[28. Blazor-ApexCharts: Integración y Solución de Problemas](#28-blazor-apexcharts-integración-y-solución-de-problemas)
  - [28.1. Instalación](#281-instalación)
  - [28.2. Configuración en Program.cs](#282-configuración-en-programcs)
  - [28.3. Uso de Componentes](#283-uso-de-componentes)
  - [28.4. Problema: 404 en archivos JS](#284-problema-404-en-archivos-js)
  - [28.5. Resumen de Soluciones](#285-resumen-de-soluciones)

---

## 28.1. Instalación

Blazor-ApexCharts se instala como NuGet package en el proyecto Blazor (Razor Class Library):

```xml
<!-- TiendaDawWeb.Shared.Blazor.csproj -->
<PackageReference Include="Blazor-ApexCharts" />
```

La versión se controla centralmente en `Directory.Packages.props`:

```xml
<PackageVersion Include="Blazor-ApexCharts" Version="6.1.0" />
```

---

## 28.2. Configuración en Program.cs

### Requisitos en el proyecto host (MVC/RazorPages)

```csharp
// 1. Habilitar static web assets de RCLs (obligatorio)
builder.WebHost.UseStaticWebAssets();

// 2. En el pipeline, después de MapStaticAssets():
app.MapStaticAssets();        // Sirve assets de RCL incluidos _content/
app.ConfigureStaticFiles();   // Solo para uploads (PhysicalFileProvider)
```

### En el proyecto Blazor (Razor Class Library)

```csharp
// En _Imports.razor
@using ApexCharts
```

---

## 28.3. Uso de Componentes

```razor
@* En una vista Razor que renderiza componentes Blazor *@
<component type="typeof(TiendaDawWeb.Shared.Blazor.Admin.Charts.VentasMensualesChart)"
           render-mode="ServerPrerendered" />
```

```razor
@* Dentro de un componente Blazor *@
<ApexChart TItem="MonthlySalesDto"
           Title="Ventas Mensuales"
           Options="Options">
    <ApexSeries TItem="MonthlySalesDto"
                Name="Ventas"
                Items="Data"
                XValue="@(e => e.Month)"
                YValue="@(e => e.Total)" />
</ApexChart>

@code {
    private ApexChartOptions<MonthlySalesDto> Options { get; set; } = new();
}
```

---

## 28.4. Problema: 404 en archivos JS

### Síntoma

```
[ERR] Error detectado: 404 | Path: /_content/Blazor-ApexCharts/js/apex-charts.js
[ERR] Error detectado: 404 | Path: /_content/Blazor-ApexCharts/js/blazor-apex-charts.js
```

### Causa raíz

Los `<script>` tags manuales en el Layout referenciaban **nombres de archivo incorrectos** para la versión 6.x del paquete:

| Script en Layout (incorrecto) | Archivo real en v6.1.0 |
|------------------------------|----------------------|
| `apex-charts.js` | `apexcharts.esm.js` |
| `blazor-apex-charts.js` | `blazor-apexcharts.js` |

Los nombres de archivo cambiaron entre versiones del paquete. En versiones antiguas (1.x) era necesario incluir los scripts manualmente; desde v2.0+, **la librería inyecta sus scripts automáticamente**.

### Solución

**Eliminar los `<script>` manuales** de ambos Layouts:

```html
<!-- ❌ ELIMINAR - nombres incorrectos + innecesario en v6.x -->
<script src="_content/Blazor-ApexCharts/js/apex-charts.js"></script>
<script src="_content/Blazor-ApexCharts/js/blazor-apex-charts.js"></script>
```

La librería Blazor-ApexCharts v6.x se auto-inyecta al inicializar el componente Blazor. No se requieren tags manuales.

### Verificación

1. Eliminar los scripts de `_Layout.cshtml` (MVC y RazorPages)
2. Compilar y arrancar
3. Visitar el dashboard admin con gráficas
4. Comprobar que las gráficas se renderizan
5. Comprobar que ya no aparecen los 404 en logs

---

## 28.5. Resumen de Soluciones

| Problema | Causa | Solución |
|----------|-------|----------|
| 404 en `apex-charts.js` | Nombre incorrecto + innecesario en v6.x | Eliminar `<script>` manual |
| 404 en `blazor-apex-charts.js` | Nombre incorrecto + innecesario en v6.x | Eliminar `<script>` manual |
| Assets no se copian al build | Issue conocido #393 del repo | Rebuild completo (`dotnet clean` + `dotnet build`) |
| Gráficas no se renderizan | Falta `render-mode="ServerPrerendered"` | Añadir al `<component>` tag |

### Regla General

> **Desde Blazor-ApexCharts v2.0+, no incluyas `<script>` tags manuales.**
> La librería se auto-inyecta. Los nombres de archivo pueden cambiar entre versiones.

---

**Anterior**: [27. CI/CD](../27-CI-CD.md)
**Próximo**: Fin de documentación
