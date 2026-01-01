# 🔒 Concurrency Control in TiendaDawWeb

## Overview

This document explains the concurrency control mechanisms implemented in the TiendaDawWeb application, focusing on the critical shopping cart functionality.

## Why Concurrency Control?

In a web application, multiple requests can simultaneously try to modify the same data. Without proper concurrency control, this leads to:

- **Lost updates**: User A's changes are overwritten by User B
- **Inconsistent data**: Cart totals don't match individual items
- **Race conditions**: Two requests add the same product simultaneously
- **Data corruption**: Database integrity violations

## Optimistic vs Pessimistic Concurrency

### Pessimistic Concurrency (Locking)
- Lock the data when reading
- Block other users until the lock is released
- **Drawback**: Poor performance, potential deadlocks
- **Use case**: High contention scenarios

### Optimistic Concurrency (Version Checking)
- Don't lock the data
- Check if data changed before updating
- Retry or notify user if conflict detected
- **Advantage**: Better performance, no deadlocks
- **Use case**: Most web applications (including ours)

## ✅ Optimistic Concurrency Implementation

### 1. Database Level - Row Version

```csharp
public class CarritoItem
{
    public long Id { get; set; }
    public int Cantidad { get; set; }
    public decimal Subtotal { get; set; }
    
    /// <summary>
    /// Concurrency token - automatically updated by EF Core
    /// </summary>
    [Timestamp]
    public byte[]? RowVersion { get; set; }
    
    // ... other properties
}
```

**How it works:**
1. EF Core generates a `rowversion` column (SQL Server) or similar
2. Value changes automatically on every update
3. EF Core checks this value before updating
4. If changed, throws `DbUpdateConcurrencyException`

### 2. Entity Framework Configuration

```csharp
// In ApplicationDbContext.OnModelCreating
builder.Entity<CarritoItem>(entity =>
{
    // Índice único por usuario y producto
    entity.HasIndex(c => new { c.UsuarioId, c.ProductoId }).IsUnique();
});
```

**Benefits:**
- Prevents duplicate cart items for same user/product
- Database-level constraint ensures data integrity
- Complements row-level concurrency control

### 3. Service Layer Handling

```csharp
public async Task<Result<CarritoItem, DomainError>> AddToCarritoAsync(
    long usuarioId, 
    long productoId, 
    int cantidad = 1)
{
    try
    {
        // Check if item exists
        var existingItem = await _context.CarritoItems
            .FirstOrDefaultAsync(c => 
                c.UsuarioId == usuarioId && 
                c.ProductoId == productoId);

        if (existingItem != null)
        {
            // Update existing - concurrency controlled by RowVersion
            existingItem.Cantidad += cantidad;
            existingItem.Subtotal = existingItem.Cantidad * producto.Precio;
            existingItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(); // May throw DbUpdateConcurrencyException
        }
        else
        {
            // Create new item
            var nuevoItem = new CarritoItem { /* ... */ };
            _context.CarritoItems.Add(nuevoItem);
            await _context.SaveChangesAsync();
        }
        
        return Result.Success<CarritoItem, DomainError>(item);
    }
    catch (DbUpdateConcurrencyException)
    {
        _logger.LogWarning("Concurrency conflict detected");
        return Result.Failure<CarritoItem, DomainError>(
            CarritoError.ConcurrencyConflict());
    }
}
```

### 4. Error Handling Pattern

```csharp
public record CarritoError : DomainError
{
    public static CarritoError ConcurrencyConflict() =>
        new("CARRITO_CONCURRENCY_CONFLICT", 
            "El carrito fue modificado por otro proceso. Por favor, intenta de nuevo.");
}
```

## 🧪 Testing Concurrency

### Unit Test Example

```csharp
[Test]
public async Task ConcurrentAddToCarrito_ShouldHandleRaceCondition()
{
    // Arrange
    var usuario = CreateTestUser(1, "test@test.com");
    var producto = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
    await _context.Users.AddAsync(usuario);
    await _context.Products.AddAsync(producto);
    await _context.SaveChangesAsync();

    // Act - Simulate 10 concurrent additions
    var tasks = new List<Task>();
    for (int i = 0; i < 10; i++)
    {
        tasks.Add(Task.Run(async () =>
        {
            // Each task uses a separate context (simulating different requests)
            using var context = new ApplicationDbContext(options);
            var service = new CarritoService(context, _loggerMock.Object);
            await service.AddToCarritoAsync(usuario.Id, producto.Id, 1);
        }));
    }

    await Task.WhenAll(tasks);

    // Assert - Verify data consistency
    var items = await _context.CarritoItems
        .Where(c => c.UsuarioId == usuario.Id)
        .ToListAsync();
    
    // Should have only one item with accumulated quantity
    items.Should().HaveCount(1);
    items.First().Cantidad.Should().BeGreaterThan(0);
}
```

### Integration Test with Stress

```csharp
[Test]
[Explicit] // Only run explicitly due to high load
public async Task StressTest_ConcurrentCartOperations()
{
    const int numUsers = 100;
    const int numOperations = 1000;
    
    var tasks = new List<Task>();
    for (int i = 0; i < numOperations; i++)
    {
        tasks.Add(Task.Run(async () =>
        {
            var userId = Random.Shared.Next(1, numUsers);
            var productId = Random.Shared.Next(1, 50);
            
            await _carritoService.AddToCarritoAsync(userId, productId, 1);
        }));
    }
    
    await Task.WhenAll(tasks);
    
    // Verify no data corruption
    var allItems = await _context.CarritoItems.ToListAsync();
    allItems.Should().OnlyHaveUniqueItems(item => new { item.UsuarioId, item.ProductoId });
}
```

## 🎯 Best Practices

### 1. Always Handle Concurrency Exceptions

```csharp
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{
    // Log the conflict
    _logger.LogWarning(ex, "Concurrency conflict for entity {EntityType}", 
        ex.Entries.First().Entity.GetType().Name);
    
    // Return meaningful error to user
    return Result.Failure<T, DomainError>(
        CarritoError.ConcurrencyConflict());
}
```

### 2. Use Unique Indexes for Business Rules

```csharp
// Prevent duplicate favorites
entity.HasIndex(f => new { f.UsuarioId, f.ProductoId }).IsUnique();

// Prevent duplicate cart items
entity.HasIndex(c => new { c.UsuarioId, c.ProductoId }).IsUnique();
```

### 3. Update Timestamps on Every Change

```csharp
public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

// Always update on modification
item.UpdatedAt = DateTime.UtcNow;
```

### 4. Consider Retry Logic for Transient Failures

```csharp
public async Task<Result<T, DomainError>> WithRetryAsync<T>(
    Func<Task<Result<T, DomainError>>> operation,
    int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        var result = await operation();
        
        if (result.IsSuccess || i == maxRetries - 1)
            return result;
            
        if (result.Error is CarritoError { Code: "CARRITO_CONCURRENCY_CONFLICT" })
        {
            await Task.Delay(TimeSpan.FromMilliseconds(100 * (i + 1)));
            continue;
        }
        
        return result;
    }
    
    return Result.Failure<T, DomainError>(
        GenericError.UnexpectedError("Max retries exceeded"));
}
```

## 📊 Monitoring Concurrency Issues

### Logging Strategy

```csharp
_logger.LogWarning(
    "Concurrency conflict detected for User {UserId}, Product {ProductId}. " +
    "Attempt {Attempt} of {MaxAttempts}",
    usuarioId, productoId, currentAttempt, maxAttempts);
```

### Metrics to Track

- **Concurrency exceptions per minute** - High rate indicates contention
- **Retry success rate** - Shows if automatic retries help
- **Average response time** - Detect performance degradation
- **Unique index violations** - Indicates race conditions

## 🔍 Debugging Concurrency Issues

### SQL Server Profiler

Enable detailed logging to see actual SQL:

```csharp
// In Program.cs for development
if (builder.Environment.IsDevelopment())
{
    builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", 
        LogLevel.Information);
}
```

### Check Row Version Values

```sql
SELECT Id, Cantidad, Subtotal, RowVersion 
FROM CarritoItems 
WHERE UsuarioId = @UserId;
```

### Identify Hotspots

```csharp
// Add performance logging
var stopwatch = Stopwatch.StartNew();
await _context.SaveChangesAsync();
stopwatch.Stop();

if (stopwatch.ElapsedMilliseconds > 1000)
{
    _logger.LogWarning(
        "Slow save detected: {Elapsed}ms for {EntityType}",
        stopwatch.ElapsedMilliseconds, 
        typeof(CarritoItem).Name);
}
```

## 🚫 Common Pitfalls

### ❌ Pitfall 1: Not Handling Concurrency Exceptions

```csharp
// BAD - Exception bubbles up, crashes application
await _context.SaveChangesAsync();
```

```csharp
// GOOD - Proper exception handling
try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException)
{
    // Handle gracefully
}
```

### ❌ Pitfall 2: Using Detached Entities

```csharp
// BAD - Entity is detached, concurrency check won't work
var item = new CarritoItem { Id = existingId, Cantidad = 10 };
_context.CarritoItems.Update(item);
```

```csharp
// GOOD - Load entity, then modify
var item = await _context.CarritoItems.FindAsync(existingId);
if (item != null)
{
    item.Cantidad = 10;
    await _context.SaveChangesAsync();
}
```

### ❌ Pitfall 3: Ignoring Unique Constraints

```csharp
// BAD - May violate unique constraint
var item = new CarritoItem { UsuarioId = 1, ProductoId = 1 };
_context.CarritoItems.Add(item);
```

```csharp
// GOOD - Check for existing first
var existing = await _context.CarritoItems
    .FirstOrDefaultAsync(c => c.UsuarioId == 1 && c.ProductoId == 1);
    
if (existing == null)
{
    var item = new CarritoItem { UsuarioId = 1, ProductoId = 1 };
    _context.CarritoItems.Add(item);
}
else
{
    existing.Cantidad += 1;
}
```

## 📚 Further Reading

- [EF Core Concurrency Tokens](https://docs.microsoft.com/en-us/ef/core/saving/concurrency)
- [Optimistic Concurrency Control](https://en.wikipedia.org/wiki/Optimistic_concurrency_control)
- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/)
- [DbUpdateConcurrencyException](https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbupdateconcurrencyexception)

---

**Author**: TiendaDawWeb Team  
**Last Updated**: 2026-01-01  
**Version**: 1.0
