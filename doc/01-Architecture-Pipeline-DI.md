# 01. La Forja de .NET: Arquitectura, Middlewares y DI

Bienvenido a la maestría de arquitectura. Para dominar .NET 10, debes entender que no estamos ante un "servidor de archivos", sino ante un **Pipeline de Ejecución Dinámico**.

## 1. El Viaje de una Petición (The Middleware Pipeline)
Cuando un usuario escribe `waladaw.com/Admin`, se dispara una cascada de eventos en el servidor. En `Program.cs`, el orden en el que llamas a los métodos `app.Use...` define el éxito o el fracaso de tu app.

### El Orden Sagrado:
1. **Exception Handler**: El primer middleware que entra y el último que sale. Captura errores de todos los que vienen después.
2. **Static Files**: Si la URL termina en `.css` o `.jpg`, este middleware entrega el archivo y corta la petición (no llega al controlador).
3. **Routing**: Analiza la URL y busca un "match" con tus controladores.
4. **Localization**: Establece la cultura del hilo (`CultureInfo.CurrentCulture`) basándose en la Cookie o la URL.
5. **Authentication**: ¿Quién es este usuario? (Lee la Cookie de Identity).
6. **Authorization**: ¿Tiene el rol "ADMIN"?
7. **Endpoints**: Ejecuta el código C# de tu controlador.

---

## 2. Inyección de Dependencias (DI): El Contenedor Maestro
Ya no instanciamos clases con `new`. El sistema de DI es el encargado de suministrar objetos.

### Tiempos de Vida (Lifetimes) - El examen de 2DAW:
- **`AddTransient`**: Úsalo para servicios sin estado. Se crea uno nuevo cada vez que se pide. Si el controlador pide el servicio y un componente dentro de la vista también, ¡tendrás dos objetos distintos en memoria!
- **`AddScoped`**: Es el estándar de oro. El objeto vive lo que dura la petición HTTP. El `DbContext` es Scoped para asegurar que si haces 3 cambios en distintas clases, todos ocurran en la misma transacción de base de datos.
- **`AddSingleton`**: Un solo objeto para toda la vida del servidor. Peligroso si guarda datos de usuario, perfecto para cachés de precios o tareas en segundo plano.

---

## 3. Configuración Avanzada: El Reto del ContentRoot
Al separar el proyecto en `TiendaDawWeb.Web`, Rider o VS pueden ejecutar la app desde la raíz de la solución.
- **El Fallo**: El servidor busca la carpeta `Views` en el sitio equivocado y da un error 500.
- **La Solución Pro**: 
```csharp
var builder = WebApplication.CreateBuilder(new WebApplicationOptions {
    ContentRootPath = isRoot ? Path.Combine(Directory.GetCurrentDirectory(), "TiendaDawWeb.Web") : Directory.GetCurrentDirectory()
});
```
Esta lógica detecta dinámicamente si estamos en el "Laboratorio" (Raíz) o en "Producción" (Subcarpeta) y ajusta las rutas de las vistas y los estáticos.

---

## 4. Constructores Primarios (C# 14)
Reducimos el código "boilerplate" (basura visual) usando la sintaxis:
`public class ProductService(ApplicationDbContext context, ILogger<ProductService> logger)`. 
El compilador entiende automáticamente que esas variables deben ser campos privados de la clase. Es limpio, moderno y eficiente.
