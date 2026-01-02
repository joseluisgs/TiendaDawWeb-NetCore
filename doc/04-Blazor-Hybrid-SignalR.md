# 04. Blazor Server: Interactividad Real-Time en Islas

Blazor Server permite ejecutar C# en el cliente mediante un túnel de WebSockets (SignalR). En WalaDaw, lo usamos para el Widget de Estadísticas del Administrador.

## 1. El Triángulo de Configuración
Para que Blazor viva dentro de MVC, necesitas:
1. **Backend**: `builder.Services.AddServerSideBlazor()` y `app.MapBlazorHub()`.
2. **Layout**: La etiqueta `<base href="~/" />` para orientar al JS de Blazor.
3. **Frontend**: El script `<script src="_framework/blazor.server.js"></script>`.

## 2. El Apocalipsis del Error 404 en el Framework
**Problema**: Al mover el proyecto a una subcarpeta, el navegador no encontraba el JS de Blazor.
**Causa**: .NET sirve archivos virtuales del framework desde las DLLs. Si el ContentRoot no es exacto, los ignora.
**Solución Senior**: 
```csharp
builder.WebHost.UseStaticWebAssets();
```
Esto habilita la lectura de recursos estáticos embebidos en paquetes NuGet y librerías del sistema.

## 3. Ciclo de Vida y Estados
- **`OnInitializedAsync`**: Aquí cargamos los datos. Se ejecuta una vez al conectar.
- **`StateHasChanged()`**: Notifica a Blazor que debe repintar el HTML. Útil cuando recibes datos de un hilo secundario (Timer).

## 4. Multi-hilo y Timers (InvokeAsync)
El widget de estadísticas se actualiza solo cada 15s usando un Timer. 
- **Peligro**: Los Timers corren en hilos de background. No pueden tocar el HTML directamente.
- **Solución**: `await InvokeAsync(StateHasChanged)`. Esto envía el refresco al hilo principal de la UI.

---

## 5. El "Dispose" (Manejo de Memoria)
Blazor Server mantiene el estado en la RAM del servidor. Si no detienes los Timers al cerrar la pestaña, el servidor acabará por saturarse.
```csharp
public void Dispose() {
    _timer?.Dispose();
}
```
Esto asegura que la aplicación sea sostenible y profesional.
