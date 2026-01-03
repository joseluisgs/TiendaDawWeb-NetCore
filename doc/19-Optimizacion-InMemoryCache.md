# 19 - Optimización de Rendimiento: InMemoryCache en .NET 10

En este volumen aprendemos a escalar nuestra aplicación reduciendo la carga en la base de datos mediante el uso de **Caché en Memoria (InMemoryCache)**.

---

## 1. El Concepto: ¿Por qué cachear en memoria?

Los servicios de nuestra aplicación (como `ProductService`) consultan frecuentemente la base de datos para obtener información que **cambia poco o nada**:
- El catálogo de productos
- Los detalles de productos individuales
- Las valoraciones de un producto

Cada consulta a EF Core implica:
1. Abrir conexión a la base de datos
2. Generar y ejecutar SQL
3. Deserializar los resultados
4. Cerrar conexión

Si 100 usuarios piden el mismo producto, estamos repitiendo este trabajo 100 veces. **InMemoryCache** nos permite hacer el trabajo **una sola vez** y mantener el resultado en memoria RAM para todas las siguientes consultas.

---

## 2. Implementación Técnica

### A. Registro del Servicio (`Program.cs`)

En .NET 10, la caché en memoria se registra como un servicio estándar:

```csharp
builder.Services.AddMemoryCache();
```

Esto registra `IMemoryCache` en el contenedor de DI con una duración de **Singleton** (una única instancia para toda la aplicación).

### B. Inyección y Uso en el Servicio (`ProductService.cs`)

```csharp
public class ProductService(
    ApplicationDbContext context,
    IMemoryCache cache,  // Inyectamos la caché
    ILogger<ProductService> logger
) : IProductService {
    private const string ProductsCacheKey = "all_products";
    
    public async Task<Result<IEnumerable<Product>, DomainError>> GetAllAsync() {
        // 1. Intentamos obtener de caché
        if (cache.TryGetValue<IEnumerable<Product>>(ProductsCacheKey, out var cachedProducts)) {
            logger.LogInformation("Productos servidos desde caché");
            return Result.Success<IEnumerable<Product>, DomainError>(cachedProducts!);
        }
        
        // 2. Si no está en caché, consultamos la DB
        var products = await context.Products
            .Include(p => p.Propietario)
            .Where(p => !p.Deleted)
            .Where(p => p.CompraId == null)  // Solo productos disponibles
            .ToListAsync();
        
        // 3. Guardamos en caché con expiración de 5 minutos
        cache.Set(ProductsCacheKey, products, TimeSpan.FromMinutes(5));
        
        logger.LogInformation("Productos cargados desde DB y cacheados");
        return Result.Success<IEnumerable<Product>, DomainError>(products);
    }
}
```

---

## 3. El Desafío de la Invalidación (Senior Tip)

Un error **crítico** del desarrollador junior es cachear sin plan de invalidación. Ejemplo del error:

**Escenario del desastre:**
1. El usuario A crea un producto "iPhone 15"
2. El usuario B consulta el catálogo → caché guarda la lista SIN el iPhone 15
3. El usuario A consulta el catálogo → ¡VE la lista con el iPhone 15! (caché incorrecta)

**La solución:** Debemos **invalidar (borrar) la caché** cuando se realizan modificaciones:

```csharp
// Al crear un producto
public async Task<Result<Product, DomainError>> CreateAsync(...) {
    context.Products.Add(product);
    await context.SaveChangesAsync();
    
    // INVALIDACIÓN: Borramos la caché para que la próxima consulta lea de DB
    cache.Remove(ProductsCacheKey);
}

// Al actualizar un producto
public async Task<Result<Product, DomainError>> UpdateAsync(...) {
    // IMPORTANTE: Invalidamos ANTES de guardar para evitar datos inconsistentes
    cache.Remove(ProductsCacheKey);
    cache.Remove(ProductDetailsCacheKey(id));
    
    await context.SaveChangesAsync();
}

// Al comprar (marcar producto como vendido)
// PurchaseService también debe invalidar
cache.Remove(ProductsCacheKey);
```

---

## 4. El Problema de EF Core con Caché (Expert Level)

Un problema sutil que se presenta con `InMemoryCache` + EF Core:

**El bug:**
```csharp
// GetUserAsync obtiene de caché (entidad trackeada por DbContext ANTERIOR)
var product = await GetByIdAsync(id);

// Intentamos modificar (¡FAIL! No está trackeada por este DbContext)
product.Nombre = "Nuevo nombre";
await context.SaveChangesAsync();  // No guarda nada
```

**La solución:** Para operaciones de UPDATE, **leer directamente de la base de datos** en lugar de la caché:

```csharp
// Para reads: Usamos caché
var products = await GetAllAsync();  // De caché si está disponible

// Para updates: Leemos de DB directamente
var product = await context.Products
    .Include(p => p.Propietario)
    .Include(p => p.Ratings)
    .FirstOrDefaultAsync(p => p.Id == id);  // Trackeado por este DbContext

product.Nombre = "Nuevo nombre";
await context.SaveChangesAsync();  // ✅ Guarda correctamente
```

---

## 5. Estrategias de Expiración

`IMemoryCache` ofrece múltiples formas de controlar cuándo expiran los datos:

```csharp
// A. Expiración por tiempo absoluto
cache.Set(key, data, TimeSpan.FromMinutes(5));  // Expira en 5 minutos

// B. Expiración por tiempo relativo (sliding)
cache.Set(key, data, new MemoryCacheEntryOptions {
    SlidingExpiration = TimeSpan.FromMinutes(5)  // Renueva si se accede
});

// C. Expiración por cambio (dependencias)
var cts = new CancellationTokenSource();
cache.Set(key, data, new MemoryCacheEntryOptions {
    ExpirationTokens = { cts.Token }
});
// Cuando ejecutemos cts.Cancel(), todas las entradas expirarán
```

**Para este proyecto:** Usamos expiración por tiempo absoluto (5 minutos) porque es más predecible y fácil de entender.

---

## 6. Comparación: InMemoryCache vs OutputCache vs ResponseCache

Es vital que el alumno distinga estos tres niveles de caché:

| Característica | ResponseCache | InMemoryCache | OutputCache |
| :--- | :--- | :--- | :--- |
| **Ubicación** | Navegador del cliente | Memoria RAM del servidor | Memoria RAM del servidor |
| **Almacena** | Respuestas HTTP completas | Objetos C# (productos, usuarios) | Respuestas HTML renderizadas |
| **Usa CPU/DB** | Sí (server procesa) | **No** (lee de RAM) | **No** (sirve HTML cacheado) |
| **Control** | Headers HTTP | Total desde C# | Atributos en Controllers |
| **Ideal para** | Páginas estáticas, assets | Objetos de negocio, consultas | Vistas completas, catálogos |
| **Duración** | Hasta que expire | Hasta que invalides/manual | Hasta que expire |

---

## 7. Conclusión para el Alumno

`InMemoryCache` es la herramienta de optimización más poderosa para **escalar lecturas** de base de datos. Nos permite:

✅ Reducir la carga en la base de datos hasta un 90%
✅ Mejorar el tiempo de respuesta de milisegundos a microsegundos
✅ Servir a más usuarios con el mismo hardware

**Pero recuerda:**
- ⚠️ La caché es **volátil** (se pierde al reiniciar el servidor)
- ⚠️ Requiere **invalidación manual** (borrar caché en updates)
- ⚠️ Puede tener **problemas de tracking** con EF Core en updates

Un arquitecto .NET moderno combina los tres tipos de caché para máxima eficiencia.
