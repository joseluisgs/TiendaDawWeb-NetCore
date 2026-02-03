# TODO - Completar Conversión a Razor Pages

## ✅ Completado
- [x] Crear 24 PageModels (.cshtml.cs) basados en controladores MVC
- [x] Mantener toda la lógica de negocio idéntica
- [x] Preservar atributos de autorización
- [x] Usar inyección de dependencias por constructor
- [x] Implementar OnGet/OnPost correctamente

## 📝 Pendiente - Actualizar Vistas .cshtml

### 1. Agregar directivas @page y @model

**Para CADA archivo .cshtml (excepto partials `_*.cshtml`):**

```csharp
@page
@model TiendaDawWeb.Web.RazorPages.Pages.{Area}.{PageName}Model
```

**Ejemplo para `Public/Index.cshtml`:**
```csharp
@page
@model TiendaDawWeb.Web.RazorPages.Pages.Public.IndexModel
```

#### Archivos a actualizar:
- [ ] Public/Index.cshtml
- [ ] Auth/Login.cshtml
- [ ] Auth/Register.cshtml
- [ ] Auth/AccessDenied.cshtml
- [ ] Product/Index.cshtml
- [ ] Product/Details.cshtml
- [ ] Product/Create.cshtml
- [ ] Product/Edit.cshtml
- [ ] Product/MyProducts.cshtml
- [ ] Admin/Index.cshtml
- [ ] Admin/Usuarios.cshtml
- [ ] Admin/UsuarioDetails.cshtml
- [ ] Admin/Productos.cshtml
- [ ] Admin/Compras.cshtml
- [ ] Admin/Ventas.cshtml
- [ ] Admin/Estadisticas.cshtml
- [ ] Carrito/Index.cshtml
- [ ] Carrito/Resumen.cshtml
- [ ] Purchase/Index.cshtml
- [ ] Purchase/Details.cshtml
- [ ] Purchase/Confirmacion.cshtml
- [ ] Profile/Index.cshtml
- [ ] Profile/Edit.cshtml
- [ ] Profile/ChangePassword.cshtml
- [ ] Favorite/Index.cshtml

### 2. Reemplazar Tag Helpers de MVC por Razor Pages

#### Patrón a buscar y reemplazar:

**MVC:**
```html
<a asp-controller="Product" asp-action="Details" asp-route-id="@product.Id">Ver</a>
```

**Razor Pages:**
```html
<a asp-page="/Product/Details" asp-route-id="@product.Id">Ver</a>
```

#### Reemplazos comunes:

| MVC | Razor Pages |
|-----|-------------|
| `asp-controller="Public" asp-action="Index"` | `asp-page="/Public/Index"` |
| `asp-controller="Auth" asp-action="Login"` | `asp-page="/Auth/Login"` |
| `asp-controller="Auth" asp-action="Register"` | `asp-page="/Auth/Register"` |
| `asp-controller="Product" asp-action="Index"` | `asp-page="/Product/Index"` |
| `asp-controller="Product" asp-action="Details"` | `asp-page="/Product/Details"` |
| `asp-controller="Product" asp-action="Create"` | `asp-page="/Product/Create"` |
| `asp-controller="Product" asp-action="Edit"` | `asp-page="/Product/Edit"` |
| `asp-controller="Carrito" asp-action="Index"` | `asp-page="/Carrito/Index"` |
| `asp-controller="Purchase" asp-action="Index"` | `asp-page="/Purchase/Index"` |
| `asp-controller="Profile" asp-action="Index"` | `asp-page="/Profile/Index"` |
| `asp-controller="Favorite" asp-action="Index"` | `asp-page="/Favorite/Index"` |
| `asp-controller="Admin" asp-action="Index"` | `asp-page="/Admin/Index"` |

### 3. Actualizar Formularios

**MVC:**
```html
<form asp-controller="Auth" asp-action="Login" method="post">
```

**Razor Pages:**
```html
<form method="post">
```

**Nota**: En Razor Pages, los formularios POST por defecto se envían a la misma página, por lo que no necesitas especificar `asp-page` si es la misma página.

### 4. Actualizar ViewData/ViewBag en las vistas

**Antes (en algunos casos):**
```html
@ViewBag.CurrentPage
```

**Ahora (mejor práctica):**
```html
@ViewData["CurrentPage"]
```

**O crear propiedades en el PageModel:**
```csharp
public int CurrentPage { get; set; }
```

## 🔧 Configuración - Program.cs

Asegúrate de tener en `Program.cs`:

```csharp
// Agregar servicios
builder.Services.AddRazorPages();

// Configurar el pipeline
app.MapRazorPages();
```

## 🧪 Probar la Aplicación

1. **Compilar:**
   ```bash
   dotnet build
   ```

2. **Ejecutar:**
   ```bash
   dotnet run
   ```

3. **Verificar rutas principales:**
   - `/` o `/Public/Index` → Página principal
   - `/Auth/Login` → Login
   - `/Auth/Register` → Registro
   - `/Product/Index` → Productos (requiere login)
   - `/Product/Create` → Crear producto
   - `/Carrito/Index` → Ver carrito
   - `/Admin/Index` → Dashboard admin (requiere rol ADMIN)

## 🐛 Problemas Comunes

### Error: "No page found at '/ControllerName/Action'"
**Solución**: Verifica que el archivo .cshtml tenga `@page` en la primera línea.

### Error: "The model item passed is of type..."
**Solución**: Asegúrate de que la directiva `@model` coincida con el namespace del PageModel.

### Error: "No se puede acceder a Input"
**Solución**: Verifica que el PageModel tenga `[BindProperty] public XViewModel Input { get; set; }`

### Formulario POST no funciona
**Solución**: 
1. Verifica que el método sea `OnPostAsync`
2. Asegúrate de que el formulario tenga `method="post"`
3. Incluye `<input type="hidden" asp-for="Input.PropertyName" />` para propiedades ocultas

## 📚 Recursos

- [Documentación oficial de Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/razor-pages/)
- [Migrar de MVC a Razor Pages](https://learn.microsoft.com/en-us/aspnet/core/migration/mvc-to-razor-pages)

## 🎯 Próximos Pasos Opcionales

- [ ] Agregar validación del lado del cliente
- [ ] Implementar filtros de página (PageFilters)
- [ ] Agregar Unit Tests para PageModels
- [ ] Implementar caching en páginas públicas
- [ ] Optimizar consultas a BD con proyecciones

---

**Fecha**: 2026-02-03
