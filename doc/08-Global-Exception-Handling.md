# 15 - Gestión Global de Errores: El Middleware de Seguridad

En este volumen aprendemos a construir una "red de seguridad" que captura cualquier fallo inesperado en nuestra aplicación, evitando que el usuario vea pantallas técnicas y asegurando que nosotros (los desarrolladores) tengamos un rastro claro del error.

---

## 1. ¿Qué es un Middleware de Excepciones?

Es una pieza de código que se sitúa al principio del pipeline de .NET. Todas las peticiones pasan por él.
-   Si la petición tiene éxito, no hace nada.
-   Si cualquier componente posterior (Controlador, Servicio, Base de Datos) lanza un error, el Middleware lo captura en su bloque `catch`.

---

## 2. Por qué es mejor que el predeterminado

El `app.UseExceptionHandler` de .NET está orientado principalmente a páginas web (HTML). Sin embargo, WalaDaw es una aplicación **híbrida**:
-   Tiene Vistas Razor.
-   Tiene APIs JSON para AJAX y Favoritos.

Nuestro Middleware personalizado detecta el origen de la petición:
1.  **Si es una API**: Devuelve un JSON estructurado con `success: false`. Esto evita que el JavaScript del navegador intente procesar un HTML de error y falle silenciosamente.
2.  **Si es una Web**: Redirige a la vista de error amigable `/Error`.

---

## 3. Implementación en WalaDaw

### La Clase Middleware (`TiendaDawWeb.Web/Middlewares/GlobalExceptionMiddleware.cs`)
Utiliza un `try-catch` que envuelve al `RequestDelegate next`.

### Registro en el Pipeline (`Program.cs`)
```csharp
app.UseGlobalExceptionHandler(); // Debe ir al principio
```

---

## 4. Beneficios para el Alumno

-   **Limpieza de Código**: Ya no necesitas llenar tus controladores de bloques `try-catch` repetitivos. Si algo falla, el middleware se encarga.
-   **Observabilidad profesional**: Cada error no controlado se guarda automáticamente en **Serilog** con el mensaje, el stack trace y la ruta que falló.
-   **Robustez**: Garantiza que la aplicación nunca devuelva una "pantalla blanca" o un error 500 sin formato.

---

## 5. Conclusión

El manejo global de excepciones es una de las marcas de un desarrollador senior. Separa la lógica de negocio del manejo de desastres, resultando en un sistema más mantenible y fácil de depurar.
