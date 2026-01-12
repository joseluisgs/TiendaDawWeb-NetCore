- [6. SQLite In-Memory vs InMemoryDatabase](#6-sqlite-in-memory-vs-inmemorydatabase)
  - [1. ¿Por qué el cambio?](#1-por-qué-el-cambio)
  - [2. El Desafío del Ciclo de Vida](#2-el-desafío-del-ciclo-de-vida)
  - [3. La Solución Keep-Alive](#3-la-solución-keep-alive)
  - [4. Integración con SeedData](#4-integración-con-seeddata)


# 6. SQLite In-Memory vs InMemoryDatabase
En esta sección, exploramos por qué migramos de `InMemoryDatabase` a SQLite In-Memory para pruebas y desarrollo.

## 1. ¿Por qué el cambio?

Aunque ambos proveedores viven en memoria RAM, existen diferencias críticas:

```mermaid
flowchart TD
    subgraph "COMPARACIÓN"
        A[InMemoryDatabase] -->|Diccionarios C#| B[No enforce FK]
        C[SQLite In-Memory] -->|Motor SQL Real| D[✅ Enforce FK]
        C -->|Transacciones| E[✅ Rollback real]
        B -->|Transacciones| F[❌ Ignoradas]
    end
    
    style C fill:#00b894
    style D fill:#00b894
    style E fill:#00b894
    style A fill:#fdcb6e
    style B fill:#fdcb6e
    style F fill:#fdcb6e
```

| Característica        | InMemoryDatabase  | SQLite In-Memory     |
| --------------------- | ----------------- | -------------------- |
| **Tipo de Motor**     | Diccionarios C#   | Motor SQL Relacional |
| **Transacciones**     | Ignoradas (No-op) | ✅ Soportadas         |
| **Claves Foráneas**   | Ignoradas         | ✅ Enforzadas         |
| **Generación de IDs** | Básica            | Secuencial SQL       |

Para transacciones serializables, `InMemoryDatabase` es insuficiente.

---

## 2. El Desafío del Ciclo de Vida

Por defecto, SQLite In-Memory desaparece cuando se cierra la conexión:

```mermaid
sequenceDiagram
    participant App as Aplicación
    participant DB as SQLite In-Memory
    participant Req as Petición 1
    participant Req2 as Petición 2

    App->>DB: Abrir conexión
    DB-->>App: ✅ BD viva
    Req->>DB: Query productos
    DB-->>Req: Productos
    Req2->>DB: Query usuarios
    DB-->>Req2: Usuarios
    App->>DB: Cerrar conexión
    DB-->>App: ❌ BD destruida
```

**Problema**: El `DbContext` se abre/cierra en cada petición.

---

## 3. La Solución Keep-Alive

En `Program.cs`, implementamos conexión persistente:

```csharp
var connectionString = "DataSource=:memory:";
var connection = new SqliteConnection(connectionString);
connection.Open();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connection));

var app = builder.Build();
SeedData.Initialize(app.Services);

app.Run();

// La conexión vive hasta que el proceso se detiene
```

Mientras `connection` viva, la base de datos permanece intacta.

---

## 4. Integración con SeedData

```csharp
public static async Task InitializeAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    if (context.Products.Any()) return;  // Ya sembrado
    
    // Crear datos de prueba
    var products = new List<Product>
    {
        new Product { Nombre = "iPhone 17 Pro Max", Precio = 1199.99m },
        new Product { Nombre = "MacBook Pro M3", Precio = 2499.99m }
    };
    
    context.Products.AddRange(products);
    await context.SaveChangesAsync();
}
```

| Ventaja                     | Inconveniente                          |
| --------------------------- | -------------------------------------- |
| Entorno limpio y predecible | Datos se pierden al reiniciar servidor |

---

**Anterior Volumen**: [05. EF Core y Persistencia](../05-EFCore-Persistence-Seed.md)  
**Próximo Volumen**: [07. Auditoría Automática](../07-Entity-Auditing-EFCore.md)
