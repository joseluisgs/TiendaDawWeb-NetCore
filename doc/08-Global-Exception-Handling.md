# 15 - Gestión Global de Errores: El Middleware de Seguridad

En este volumen aprendemos a construir una "red de seguridad" que captura cualquier fallo inesperado en nuestra aplicación, evitando que el usuario vea pantallas técnicas y asegurando que nosotros (los desarrolladores) tengamos un rastro claro del error.

---

## 1. ¿Qué es un Middleware de Excepciones?

Es una pieza de código que se sitúa al principio del pipeline de .NET. Todas las peticiones pasan por él.
- Si la petición tiene éxito, no hace nada.
- Si cualquier componente posterior (Controlador, Servicio, Base de Datos) lanza un error, el Middleware lo captura en su bloque `catch`.

---

## 2. Por qué es mejor que el predeterminado

El `app.UseExceptionHandler` de .NET está orientado principalmente a páginas web (HTML). Sin embargo, WalaDaw es una aplicación **híbrida**:
- Tiene Vistas Razor.
- Tiene APIs JSON para AJAX y Favoritos.

Nuestro Middleware personalizado detecta el origen de la petición:
1. **Si es una API**: Devuelve un JSON estructurado con `success: false`. Esto evita que el JavaScript del navegador intente procesar un HTML de error y falle silenciosamente.
2. **Si es una Web**: Redirige a la vista de error amigable `/Error`.

---

## 3. ¿Cuándo NO actúa el Middleware? (La Pregunta del Millón)

El **GlobalExceptionMiddleware** solo captura **excepciones inesperadas** (bugs/crashes). No captura:

| Tipo de error | Capturado por | Ver documento |
|---------------|---------------|---------------|
| Validaciones de formulario (ej. email inválido) | ModelState | [Vol. 07: Controladores y Models](07-Controllers-Models-Results.md) |
| Errores de dominio (ej. usuario no existe) | Result<T,E> | [Vol. 07: Controladores y Models](07-Controllers-Models-Results.md) |

### El Flujo Completo de Errores en WalaDaw

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        PIPELINE DE PETICIÓN HTTP                            │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  1. GLOBAL EXCEPTION MIDDLEWARE (Capa de Seguridad)                         │
│  ─────────────────────────────────────────────────────────────────────────  │
│  │                                                                           │
│  │   ¿Excepción inesperada (NullReference, DB timeout, bug)?               │
│  │                                                                          │
│  │   ✅ SÍ → Captura, loguea en Serilog, devuelve 500                       │
│  │   ❌ NO → Pasa al siguiente middleware                                   │
│  │                                                                           │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  2. MODEL BINDING + DATA ANNOTATIONS (Validación de Entrada)                │
│  ─────────────────────────────────────────────────────────────────────────  │
│  │                                                                           │
│  │   ¿El formulario tiene datos inválidos?                                  │
│  │   (ej. email sin @, campo requerido vacío)                               │
│  │                                                                          │
│  │   ✅ SÍ → ModelState.IsValid = false                                     │
│  │         → El Controlador muestra errores en la vista                    │
│  │   ❌ NO → Continúa                                                       │
│  │                                                                           │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  3. CONTROLADOR (Orquestación)                                              │
│  ─────────────────────────────────────────────────────────────────────────  │
│  │                                                                           │
│  │   Llama al Servicio                                                      │
│  │                                                                           │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  4. SERVICIO (Lógica de Negocio)                                            │
│  ─────────────────────────────────────────────────────────────────────────  │
│  │                                                                           │
│  │   ¿Error de dominio esperado?                                            │
│  │   (ej. producto no existe, usuario ya registrado)                        │
│  │                                                                          │
│  │   ✅ SÍ → return Result.Failure<Error>                                   │
│  │   ❌ NO → Continúa normalmente                                           │
│  │                                                                           │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│  5. CONTROLADOR (Gestión del Resultado)                                     │
│  ─────────────────────────────────────────────────────────────────────────  │
│  │                                                                           │
│  │   result.Match(                                                          │
│  │       success: () => View/Json/Redirect,                                 │
│  │       failure: error => Manejar error específico                         │
│  │   )                                                                      │
│  │                                                                           │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Resumen: ¿Quién Captura Qué?

| Escenario | Ejemplo | Capturado por |
|-----------|---------|---------------|
| Bug de programación | `product.Name.Length` siendo `null` | GlobalExceptionMiddleware |
| Timeout de base de datos | Conexión perdida | GlobalExceptionMiddleware |
| Email sin formato | `"hola"` en campo `[EmailAddress]` | ModelState (DataAnnotations) |
| Campo requerido vacío | `""` en `[Required]` | ModelState (DataAnnotations) |
| Producto no existe | ID 99999 inexistente | Result<T,E> (servicio) |
| Email ya registrado | Usuario duplicado | Result<T,E> (servicio) |
| Password débil | Menos de 8 caracteres | Result<T,E> (servicio) |

---

## 4. Implementación en WalaDaw

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
