# 05. El Almacén del Saber: EF Core y Persistencia Avanzada

Entity Framework Core (EF Core) es el "traductor" entre tus objetos C# y las tablas de tu base de datos. Nos permite trabajar con objetos como `Product` o `User` y EF Core se encarga de generar el SQL por nosotros.

## 1. `ApplicationDbContext`: El Mapa de tu Base de Datos

La clase `ApplicationDbContext` hereda de `DbContext` y es el corazón de EF Core. Define las colecciones (`DbSet`) que representan tus tablas:

```csharp
// TiendaDawWeb.Web/Data/ApplicationDbContext.cs
public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<long>, long>
{
    public DbSet<Product> Products { get; set; } // Representa la tabla 'Products'
    public DbSet<Purchase> Purchases { get; set; } // Representa la tabla 'Purchases'
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<CarritoItem> CarritoItems { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Aquí se pueden configurar relaciones, índices, y otros detalles
        // Por ejemplo, para evitar borrados en cascada no deseados:
        foreach (var relationship in builder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.Restrict;
        }
        // Configuración específica para el modelo 'Product'
        builder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Precio).HasColumnType("decimal(18, 2)");
            entity.Property(p => p.CreatedAt).HasDefaultValueSql("GETDATE()"); // Valor por defecto en la BD
            entity.Property(p => p.UpdatedAt).ValueGeneratedOnAddOrUpdate(); // Actualiza en cada modificación
            // Relación con el usuario propietario
            entity.HasOne(p => p.Propietario)
                  .WithMany(u => u.Products)
                  .HasForeignKey(p => p.PropietarioId)
                  .OnDelete(DeleteBehavior.Restrict); // Evita borrado en cascada del usuario si tiene productos
        });
        // ... otras configuraciones de modelos
    }
}
```

### 1.1. Ciclo de Vida y Concurrencia (Contexto Scoped)
El `ApplicationDbContext` se registra como **Scoped** en `Program.cs`. Esto significa:
-   Una nueva instancia del `DbContext` se crea para cada petición HTTP.
-   Todos los servicios que se inyectan en la misma petición compartirán la misma instancia del `DbContext`. Esto es crucial para la **coherencia transaccional**: si haces varias operaciones a la BD en la misma petición (ej. crear un producto y un carrito), todas se realizan dentro del mismo "contexto de trabajo".

---

## 2. In-Memory Database: Tu Laboratorio Personal

Para este proyecto didáctico, usamos una base de datos en memoria:
```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("WalaDawDb"));
```

### 2.1. Ventajas Educativas y de Desarrollo:
-   **Configuración Cero**: No necesitas instalar SQL Server, PostgreSQL, etc.
-   **Estado Limpio**: Cada vez que reinicias la aplicación, la base de datos se borra y se vuelve a crear con los datos de prueba (`SeedData`). Ideal para experimentar.
-   **Pruebas Rápidas**: En las pruebas unitarias, usar una base de datos en memoria es extremadamente rápido, ya que evita las lentas operaciones de disco.

### 2.2. Migración a Bases de Datos Reales: Un Cambio Mínimo
La belleza de EF Core es su abstracción. Para usar SQL Server, por ejemplo, solo cambiarías una línea en `Program.cs`:
```csharp
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
```
El resto de tu código de acceso a datos (`DbSet`, `Linq`) sería **idéntico**.

---

## 3. Background Services y el Dilema del `DbContext`

Los Background Services (como `CarritoCleanupService`) son tareas que se ejecutan en segundo plano, independientemente de las peticiones de los usuarios. Se registran como **Singletons**.

### 3.1. El Problema del Singleton vs. Scoped (`DbContext`)
-   **`BackgroundService`**: Es un **Singleton** (vive toda la vida de la aplicación).
-   **`ApplicationDbContext`**: Es **Scoped** (vive solo una petición HTTP).
-   **El Error**: No puedes inyectar directamente un `DbContext` en un `BackgroundService`. Esto generaría un error de "Lifetime Mismatch" porque estarías intentando meter un objeto de vida corta dentro de uno de vida larga.

### 3.2. La Solución (Patrón Factory): Creando un "Scope" Manual
Para que un Background Service pueda acceder a la base de datos de forma segura:
1.  Inyectamos `IServiceScopeFactory` (que es un Singleton).
2.  Creamos un nuevo "ámbito" (`scope`) de vida corta explícitamente.
3.  Dentro de ese ámbito, pedimos una instancia de `ApplicationDbContext`.
4.  Una vez usada, el ámbito y el `DbContext` se destruyen automáticamente.

```csharp
// TiendaDawWeb.Web/Services/Implementations/BackgroundServices/CarritoCleanupService.cs
public class CarritoCleanupService(IServiceScopeFactory scopeFactory, ILogger<CarritoCleanupService> logger) 
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Creamos un nuevo 'scope' para cada ejecución del Background Service
            using (var scope = scopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Aquí ya podemos usar 'context' de forma segura
                // ... lógica de limpieza del carrito ...
            }
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Esperar 1 hora
        }
    }
}
```
**Lección de Supervivencia**: Cuando necesites recursos `Scoped` dentro de un `Singleton`, usa `IServiceScopeFactory` para gestionar su ciclo de vida manualmente.

---

## 4. Estrategias de Carga de Datos Relacionados (Navegación)

En una base de datos, `Product` tiene un `PropietarioId`. Pero el objeto `Product` en C# tiene una propiedad `Propietario` (el objeto `User` completo).

### 4.1. Lazy Loading (Carga Perezosa) - Cuidado!
-   EF Core solo carga los datos relacionados (ej. `Propietario`) cuando accedes a ellos por primera vez.
-   **Peligro**: Si en tu vista accedes a `product.Propietario.Nombre` dentro de un bucle `foreach` sin haber cargado `Propietario` previamente, EF Core hará una consulta a la base de datos por CADA producto. Esto se conoce como el problema de "N+1 consultas" y puede matar el rendimiento.

### 4.2. Eager Loading (Carga Anticipada) - La Estrategia Usada
Cargamos los datos relacionados explícitamente con `Include()`:
```csharp
// TiendaDawWeb.Web/Services/Implementations/ProductService.cs
var product = await _context.Products
    .Include(p => p.Propietario) // Carga el objeto User del propietario
    .FirstOrDefaultAsync(p => p.Id == id);
```
**Ventaja**: Realiza una única consulta optimizada a la base de datos que trae el producto y su propietario a la vez.

---

## 5. Seed Data: Poblando tu Mundo de Prueba

El archivo `TiendaDawWeb.Web/Data/SeedData.cs` se encarga de que tu aplicación tenga datos de prueba al arrancar.

### 5.1. Pasos de un Buen Seed Data:
1.  **Verificación Previa**: `if (context.Products.Any()) return;`. Evita duplicar datos si ya existen.
2.  **Creación de Roles**: `roleManager.CreateAsync(new IdentityRole<long>("ADMIN"))`. Crea los roles necesarios.
3.  **Creación de Usuarios**: Usa `userManager.CreateAsync()` para crear usuarios y `userManager.AddToRoleAsync()` para asignar roles. Es crucial usar `UserManager` para que las contraseñas se hasheen correctamente.
4.  **Creación de Entidades**: Productos, Carritos, etc.
5.  **Relaciones**: Asegúrate de que las entidades se relacionen correctamente (ej. `new Product { Propietario = userAdmin }`).

**Lección de Supervivencia**: Un Seed Data bien hecho es tu mejor aliado para el desarrollo y las pruebas. Te permite tener un entorno reproducible con un solo `dotnet run`.

---

Este volumen te ha guiado por los profundos secretos de la persistencia de datos en .NET Core. Con EF Core, el DbContext y las estrategias de carga, eres un maestro de los datos.