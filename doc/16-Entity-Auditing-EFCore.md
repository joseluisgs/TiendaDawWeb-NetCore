# 16 - Auditoría Automática de Entidades con EF Core

En el desarrollo de aplicaciones empresariales, es vital saber cuándo se creó un registro y cuándo se modificó por última vez. En este volumen aprendemos a automatizar este proceso usando la potencia de Entity Framework Core.

---

## 1. El Enfoque "Manual" (Junior)

Tradicionalmente, un desarrollador tendría que escribir este código en cada acción de sus controladores:
```csharp
producto.Nombre = "Nuevo Nombre";
producto.UpdatedAt = DateTime.UtcNow; // <--- Repetitivo y propenso a olvidos
_context.SaveChanges();
```

---

## 2. El Enfoque "Automático" (Senior)

Hemos implementado un sistema donde las entidades se "auditán" solas.

### A. La Clase Base (`AuditableEntity.cs`)
Todas nuestras clases de negocio (`Product`, `Purchase`, `Rating`, etc.) heredan de esta base que contiene los campos:
- `CreatedAt`
- `UpdatedAt`
- `CreatedBy` (Opcional)
- `UpdatedBy` (Opcional)

### B. El Interceptor (`ApplicationDbContext.cs`)
Hemos sobrescrito el método `SaveChangesAsync`. Antes de que los datos viajen a la base de datos, EF Core:
1.  Revisa qué entidades han cambiado (`ChangeTracker`).
2.  Si la entidad es de tipo `AuditableEntity`, rellena la fecha automáticamente.

```csharp
public override Task<int> SaveChangesAsync(...) {
    foreach (var entry in ChangeTracker.Entries<AuditableEntity>()) {
        if (entry.State == EntityState.Added)
            entry.Entity.CreatedAt = DateTime.UtcNow;
        else if (entry.State == EntityState.Modified)
            entry.Entity.UpdatedAt = DateTime.UtcNow;
    }
    return base.SaveChangesAsync();
}
```

---

## 3. Beneficios para el Proyecto

1.  **Código Limpio (DRY)**: Los controladores y servicios se centran solo en la lógica de negocio. No hay código de "fontanería" para gestionar fechas.
2.  **Consistencia**: Garantizamos que TODOS los registros tengan su rastro de auditoría sin excepción.
3.  **Trazabilidad**: Si un producto cambia de precio, sabremos exactamente en qué segundo ocurrió.

---

## 4. Conclusión para el Alumno

La auditoría automática demuestra por qué Entity Framework Core es mucho más que un simple mapeador de tablas. Es un motor con inteligencia capaz de inyectar comportamiento transversal en toda nuestra aplicación, mejorando la calidad del dato y la productividad del equipo.
