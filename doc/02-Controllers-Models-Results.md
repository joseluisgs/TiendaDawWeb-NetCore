# 02. El Cerebro de la App: Controladores y Lógica de Negocio

El Controlador es el director de orquesta. Recibe la batuta (la petición HTTP) y decide qué músico (Servicio) debe tocar.

## 1. Anatomía de una Acción
```csharp
[HttpPost] // Verbo HTTP
[ValidateAntiForgeryToken] // Protección contra ataques CSRF
public async Task<IActionResult> Create(ProductViewModel model) {
    if (!ModelState.IsValid) return View(model); // Validación automática
    // ...
}
```
**Tip Senior**: Nunca escribas lógica de SQL dentro de una acción. El controlador debe limitarse a validar la entrada y llamar al servicio.

## 2. El Patrón Result: Programación sin Excepciones
Lanzar una excepción (`throw`) para un error de negocio (ej. "No hay stock") es un pecado de rendimiento.
Usamos el **Patrón Result** de la librería `CSharpFunctionalExtensions`.
- **Éxito**: Devuelve los datos.
- **Fallo**: Devuelve un código de error tipado (`DomainError`).

Esto permite al controlador escribir código fluido:
```csharp
var result = await _service.PurchaseAsync(productId);
return result.Match(
    onSuccess: () => RedirectToAction("Success"),
    onFailure: (error) => View("Error", error)
);
```

## 3. ViewModels vs Entidades
- **Entidad (`Product`)**: Es el reflejo fiel de la base de datos.
- **ViewModel (`ProductViewModel`)**: Es lo que la web necesita.
**Por qué usarlos**: En la creación de un producto, el usuario NO envía el `Id` ni la `FechaCreacion`. Si usaras la Entidad directamente, el usuario podría "inyectar" valores malintencionados en campos ocultos (Overposting). El ViewModel es tu primer muro de seguridad.

## 4. Model Binding y Validación Automática
.NET lee el formulario HTML y busca coincidencias por nombre en el objeto C#. 
- **Atributos**: `[Required]`, `[StringLength(50)]`, `[EmailAddress]`.
- **ModelState**: Al entrar en el controlador, .NET ya ha validado todo. Si `ModelState.IsValid` es falso, no sigas; devuelve al usuario al formulario para que corrija sus errores.
