# 14 - Optimización de Rendimiento: Output Cache en .NET 10

En este volumen aprendemos a escalar nuestra aplicación reduciendo el trabajo innecesario del servidor mediante el uso de la **Caché de Salida (Output Cache)**.

---

## 1. El Concepto: ¿Por qué cachear el catálogo?

En el escaparate público (`Public/Index`), los productos no cambian cada segundo. Sin embargo, cada vez que un usuario entra o refresca, el servidor:
1. Procesa la petición.
2. Consulta la base de datos (EF Core).
3. Renderiza el HTML (Razor).
4. Envía la respuesta.

Si tenemos 1.000 usuarios concurrentes, estamos haciendo 1.000 consultas idénticas. **Output Cache** permite que el servidor haga este trabajo una sola vez y guarde el resultado en memoria para los siguientes usuarios.

---

## 2. Implementación Técnica

### A. Registro del Servicio (`Program.cs`)
```csharp
builder.Services.AddOutputCache();
// ...
app.UseOutputCache();
```

### B. Aplicación en el Controlador (`PublicController.cs`)
Hemos aplicado el atributo `[OutputCache]` con una duración de 60 segundos.

```csharp
[OutputCache(Duration = 60, VaryByQueryKeys = new[] { "q", "categoria", "minPrecio", "maxPrecio", "page", "size" })]
public async Task<IActionResult> Index(...) { ... }
```

---

## 3. El desafío de la Variación (`VaryByQueryKeys`)

Un error común del programador junior es cachear la página sin tener en cuenta los filtros. Si un usuario busca "iPhone" y el servidor cachea esa respuesta para todos, el siguiente usuario que busque "Samsung" vería los iPhones.

Para evitarlo, usamos `VaryByQueryKeys`. Esto le indica a .NET que genere **entradas de caché distintas** para cada combinación de:
- Texto de búsqueda (`q`).
- Categoría seleccionada.
- Rango de precios.
- Número de página.

---

## 4. Diferencia con ResponseCache (Senior Tip)

Es vital que el alumno distinga estos dos conceptos:

| Característica | ResponseCache | OutputCache (.NET 10) |
| :--- | :--- | :--- |
| **Ubicación** | Navegador del cliente | Memoria del servidor |
| **Ahorro de Red** | Sí (el cliente no pide) | No (el cliente pide, el server responde rápido) |
| **Ahorro de CPU/DB** | No (si otro cliente pide, el server trabaja) | **SÍ (el server no vuelve a consultar DB)** |
| **Control** | Basado en cabeceras HTTP | Control total desde C# |

---

## 5. Conclusión para el Alumno

La caché es una de las herramientas más potentes pero peligrosas. Un buen desarrollador debe saber **qué cachear** (datos que cambian poco y se consultan mucho) y **cómo invalidar** esa caché si fuera necesario. En este proyecto, logramos que el escaparate sea ultrarrápido sin comprometer la precisión de los filtros.
