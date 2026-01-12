- [16. Gestión Global de Errores: El Middleware de Seguridad](#16-gestión-global-de-errores-el-middleware-de-seguridad)
  - [1. ¿Qué es un Middleware de Excepciones?](#1-qué-es-un-middleware-de-excepciones)
  - [2. Por qué es mejor que el predeterminado](#2-por-qué-es-mejor-que-el-predeterminado)
  - [3. ¿Cuándo NO actúa el Middleware? (La Pregunta del Millón)](#3-cuándo-no-actúa-el-middleware-la-pregunta-del-millón)
    - [El Flujo Completo de Errores en WalaDaw](#el-flujo-completo-de-errores-en-waladaw)
    - [Diagrama de Secuencia: Una Petición con Error](#diagrama-de-secuencia-una-petición-con-error)
    - [Resumen: ¿Quién Captura Qué?](#resumen-quién-captura-qué)
  - [4. Implementación en WalaDaw](#4-implementación-en-waladaw)
    - [La Clase Middleware (`TiendaDawWeb.Web/Middlewares/GlobalExceptionMiddleware.cs`)](#la-clase-middleware-tiendadawwebwebmiddlewaresglobalexceptionmiddlewarecs)
    - [Registro en el Pipeline (`Program.cs`)](#registro-en-el-pipeline-programcs)
  - [4. Beneficios para el Alumno](#4-beneficios-para-el-alumno)
  - [5. Conclusión](#5-conclusión)


# 16. Gestión Global de Errores: El Middleware de Seguridad

En esta sección aprendemos a construir una "red de seguridad" que captura cualquier fallo inesperado en nuestra aplicación, evitando que el usuario vea pantallas técnicas y asegurando que nosotros (los desarrolladores) tengamos un rastro claro del error.

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

| Tipo de error                                   | Capturado por | Ver documento                                                       |
| ----------------------------------------------- | ------------- | ------------------------------------------------------------------- |
| Validaciones de formulario (ej. email inválido) | ModelState    | [Vol. 07: Controladores y Models](07-Controllers-Models-Results.md) |
| Errores de dominio (ej. usuario no existe)      | Result<T,E>   | [Vol. 07: Controladores y Models](07-Controllers-Models-Results.md) |

### El Flujo Completo de Errores en WalaDaw

```mermaid
flowchart TD
    A["📥 Petición HTTP"] --> B[1. GLOBAL EXCEPTION MIDDLEWARE]
    
    B --> C{¿Excepción inesperada?<br/>Bug, NullReference,<br/>DB timeout}
    C -->|SÍ| D[📝 Log en Serilog]
    D --> E[❌ Devuelve 500]
    E --> F["📄 Página de error<br/>o JSON 500"]
    
    C -->|NO| G[2. MODEL BINDING + DATA ANNOTATIONS]
    
    G --> H{¿Datos inválidos?<br/>Email sin @,<br/>Campo vacío}
    H -->|SÍ| I[❌ ModelState.IsValid = false]
    I --> J["📝 Mostrar errores<br/>en formulario"]
    
    H -->|NO| K[3. CONTROLADOR]
    K --> L[4. SERVICIO<br/>Lógica de Negocio]
    
    L --> M{¿Error esperado?<br/>Producto no existe,<br/>Email duplicado}
    M -->|SÍ| N[✅ return Result.Failure]
    N --> O[5. CONTROLADOR<br/>Match]
    
    M -->|NO| P[✅ Continúa normal]
    P --> O
    
    O --> Q{¿Éxito o Error?}
    Q -->|Éxito| R["📄 View / Redirect / JSON"]
    Q -->|Error| S["📝 Manejo específico<br/>del error de dominio"]
    
    style A fill:#e1f5fe
    style B fill:#fff3e0
    style G fill:#e8f5e9
    style L fill:#fce4ec
    style O fill:#f3e5f5
    style D fill:#ffebee
    style I fill:#ffebee
    style N fill:#e8f5e9
    style S fill:#fff3e0
```

### Diagrama de Secuencia: Una Petición con Error

```mermaid
sequenceDiagram
    participant U as Usuario
    participant M as Middleware
    participant C as Controlador
    participant S as Servicio
    participant DB as Base Datos

    U->>M: POST /productos/crear
    M->>M: ¿Excepción?
    Note over M: No, continúa
    
    M->>C: Invoke action
    C->>C: ModelState.IsValid?
    Note over C: Sí, datos válidos
    
    C->>S: CreateAsync(datos)
    S->>DB: SaveChanges()
    
    DB-->>S: ❌ TimeoutException!
    S-->>C: throw DB timeout
    C-->>M: Excepción propagada
    
    M->>M: Captura excepción
    M->>M: Log en Serilog
    M-->>U: ❌ 500 Internal Error
    
    Note over M,U: GlobalExceptionMiddleware<br/>captura el bug inesperado
```

### Resumen: ¿Quién Captura Qué?

| Escenario                | Ejemplo                             | Capturado por                |
| ------------------------ | ----------------------------------- | ---------------------------- |
| Bug de programación      | `product.Name.Length` siendo `null` | GlobalExceptionMiddleware    |
| Timeout de base de datos | Conexión perdida                    | GlobalExceptionMiddleware    |
| Email sin formato        | `"hola"` en campo `[EmailAddress]`  | ModelState (DataAnnotations) |
| Campo requerido vacío    | `""` en `[Required]`                | ModelState (DataAnnotations) |
| Producto no existe       | ID 99999 inexistente                | Result<T,E> (servicio)       |
| Email ya registrado      | Usuario duplicado                   | Result<T,E> (servicio)       |
| Password débil           | Menos de 8 caracteres               | Result<T,E> (servicio)       |

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
