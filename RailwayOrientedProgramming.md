# 🚂 Railway Oriented Programming in TiendaDawWeb

## Overview

Railway Oriented Programming (ROP) is a functional programming pattern for handling errors in a clean, composable way. This document explains how ROP is implemented throughout the TiendaDawWeb application.

## The Problem: Traditional Error Handling

### ❌ Traditional Approach

```csharp
public async Task<CarritoItem?> AddToCarrito(long usuarioId, long productoId, int cantidad)
{
    if (cantidad <= 0)
        throw new ArgumentException("Cantidad inválida");
        
    var producto = await _context.Products.FindAsync(productoId);
    if (producto == null)
        throw new NotFoundException("Producto no encontrado");
        
    if (producto.Reservado)
        throw new BusinessException("Producto no disponible");
        
    var item = new CarritoItem { /* ... */ };
    await _context.SaveChangesAsync();
    return item;
}
```

**Problems:**
- Exceptions for control flow (expensive)
- Caller doesn't know what errors to expect
- No type safety for error handling
- Hard to compose operations
- Inconsistent error handling across codebase

## The Solution: Railway Oriented Programming

### ✅ ROP Approach

```csharp
public async Task<Result<CarritoItem, DomainError>> AddToCarritoAsync(
    long usuarioId, 
    long productoId, 
    int cantidad)
{
    if (cantidad <= 0)
        return Result.Failure<CarritoItem, DomainError>(
            CarritoError.InvalidQuantity(cantidad));
        
    var producto = await _context.Products.FindAsync(productoId);
    if (producto == null)
        return Result.Failure<CarritoItem, DomainError>(
            ProductError.NotFound(productoId));
        
    if (producto.Reservado)
        return Result.Failure<CarritoItem, DomainError>(
            CarritoError.ProductNotAvailable(productoId));
        
    var item = new CarritoItem { /* ... */ };
    await _context.SaveChangesAsync();
    
    return Result.Success<CarritoItem, DomainError>(item);
}
```

**Benefits:**
- Explicit error handling in method signature
- Type-safe error propagation
- Composable operations
- No exceptions for business logic
- Self-documenting code

## Core Components

### 1. Result<TSuccess, TFailure>

From `CSharpFunctionalExtensions` package:

```csharp
public class Result<TSuccess, TFailure>
{
    public bool IsSuccess { get; }
    public bool IsFailure { get; }
    public TSuccess Value { get; }      // Only accessible if IsSuccess
    public TFailure Error { get; }      // Only accessible if IsFailure
}
```

**Usage:**

```csharp
// Success case
var result = Result.Success<Product, DomainError>(product);

// Failure case
var result = Result.Failure<Product, DomainError>(ProductError.NotFound(id));

// Pattern matching
var message = result.IsSuccess 
    ? $"Product: {result.Value.Nombre}"
    : $"Error: {result.Error.Message}";
```

### 2. DomainError Hierarchy

```csharp
/// <summary>
/// Base class for all domain errors
/// </summary>
public abstract record DomainError(string Code, string Message);

/// <summary>
/// Product-specific errors
/// </summary>
public record ProductError : DomainError
{
    private ProductError(string code, string message) : base(code, message) { }

    public static ProductError NotFound(long id) =>
        new("PRODUCT_NOT_FOUND", $"Producto con ID {id} no encontrado");

    public static ProductError AlreadyReserved(long id) =>
        new("PRODUCT_RESERVED", $"Producto con ID {id} ya está reservado");

    public static ProductError InvalidPrice(decimal price) =>
        new("INVALID_PRICE", $"Precio {price} no es válido");
}

/// <summary>
/// Shopping cart errors
/// </summary>
public record CarritoError : DomainError
{
    private CarritoError(string code, string message) : base(code, message) { }

    public static CarritoError ItemNotFound(long id) =>
        new("CARRITO_ITEM_NOT_FOUND", $"Item del carrito con ID {id} no encontrado");

    public static CarritoError ProductNotAvailable(long productId) =>
        new("PRODUCT_NOT_AVAILABLE", $"El producto con ID {productId} no está disponible");

    public static CarritoError ConcurrencyConflict() =>
        new("CARRITO_CONCURRENCY_CONFLICT", 
            "El carrito fue modificado por otro proceso. Por favor, intenta de nuevo.");

    public static CarritoError InvalidQuantity(int quantity) =>
        new("INVALID_QUANTITY", $"La cantidad {quantity} no es válida");
}

/// <summary>
/// Generic errors for infrastructure issues
/// </summary>
public record GenericError : DomainError
{
    public GenericError(string code, string message) : base(code, message) { }

    public static GenericError DatabaseError(string message) =>
        new("DATABASE_ERROR", message);

    public static GenericError UnexpectedError(string message) =>
        new("UNEXPECTED_ERROR", message);
}
```

## 🎯 Implementation Patterns

### Pattern 1: Early Return on Validation

```csharp
public async Task<Result<CarritoItem, DomainError>> UpdateCantidadAsync(
    long itemId, 
    int nuevaCantidad)
{
    // Validation - early return on failure
    if (nuevaCantidad <= 0)
        return Result.Failure<CarritoItem, DomainError>(
            CarritoError.InvalidQuantity(nuevaCantidad));

    // Business logic
    var item = await _context.CarritoItems
        .Include(c => c.Producto)
        .FirstOrDefaultAsync(c => c.Id == itemId);

    if (item == null)
        return Result.Failure<CarritoItem, DomainError>(
            CarritoError.ItemNotFound(itemId));

    if (item.Producto.Reservado || item.Producto.CompraId != null)
        return Result.Failure<CarritoItem, DomainError>(
            CarritoError.ProductNotAvailable(item.ProductoId));

    // Success path
    item.Cantidad = nuevaCantidad;
    item.Subtotal = nuevaCantidad * item.Producto.Precio;
    item.UpdatedAt = DateTime.UtcNow;

    try
    {
        await _context.SaveChangesAsync();
        return Result.Success<CarritoItem, DomainError>(item);
    }
    catch (DbUpdateConcurrencyException)
    {
        return Result.Failure<CarritoItem, DomainError>(
            CarritoError.ConcurrencyConflict());
    }
}
```

### Pattern 2: Try-Catch for Infrastructure Errors

```csharp
public async Task<Result<IEnumerable<CarritoItem>, DomainError>> GetCarritoByUsuarioIdAsync(
    long usuarioId)
{
    try
    {
        var items = await _context.CarritoItems
            .Include(c => c.Producto)
            .Where(c => c.UsuarioId == usuarioId)
            .ToListAsync();

        return Result.Success<IEnumerable<CarritoItem>, DomainError>(items);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener carrito del usuario {UsuarioId}", usuarioId);
        return Result.Failure<IEnumerable<CarritoItem>, DomainError>(
            GenericError.DatabaseError("Error al obtener el carrito"));
    }
}
```

### Pattern 3: Composing Operations

```csharp
public async Task<Result<Purchase, DomainError>> FinalizarCompraAsync(long usuarioId)
{
    // Step 1: Get cart items
    var carritoResult = await _carritoService.GetCarritoByUsuarioIdAsync(usuarioId);
    if (carritoResult.IsFailure)
        return Result.Failure<Purchase, DomainError>(carritoResult.Error);

    var items = carritoResult.Value;
    
    if (!items.Any())
        return Result.Failure<Purchase, DomainError>(
            CarritoError.CarritoEmpty());

    // Step 2: Calculate total
    var totalResult = await _carritoService.GetTotalCarritoAsync(usuarioId);
    if (totalResult.IsFailure)
        return Result.Failure<Purchase, DomainError>(totalResult.Error);

    // Step 3: Create purchase
    var purchase = new Purchase
    {
        CompradorId = usuarioId,
        Total = totalResult.Value,
        FechaCompra = DateTime.UtcNow
    };

    _context.Purchases.Add(purchase);

    // Step 4: Mark products as sold
    foreach (var item in items)
    {
        item.Producto.CompraId = purchase.Id;
        item.Producto.Reservado = false;
    }

    // Step 5: Clear cart
    await _carritoService.ClearCarritoAsync(usuarioId);

    await _context.SaveChangesAsync();
    
    return Result.Success<Purchase, DomainError>(purchase);
}
```

## 🎨 Controller Integration

### MVC Controller Example

```csharp
[Route("Carrito")]
public class CarritoController : Controller
{
    private readonly ICarritoService _carritoService;

    public CarritoController(ICarritoService carritoService)
    {
        _carritoService = carritoService;
    }

    [HttpPost("Add")]
    public async Task<IActionResult> AddToCarrito(long productoId, int cantidad = 1)
    {
        var userId = long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        
        var result = await _carritoService.AddToCarritoAsync(userId, productoId, cantidad);

        return result.IsSuccess
            ? RedirectToAction("Index")
            : HandleError(result.Error);
    }

    [HttpPost("Update")]
    public async Task<IActionResult> UpdateCantidad(long itemId, int cantidad)
    {
        var result = await _carritoService.UpdateCantidadAsync(itemId, cantidad);

        return result.IsSuccess
            ? Ok(new { success = true, item = result.Value })
            : BadRequest(new { success = false, error = result.Error.Message });
    }

    private IActionResult HandleError(DomainError error)
    {
        return error switch
        {
            ProductError => NotFound(new { error = error.Message }),
            CarritoError { Code: "PRODUCT_NOT_AVAILABLE" } => 
                BadRequest(new { error = error.Message }),
            CarritoError { Code: "CARRITO_CONCURRENCY_CONFLICT" } => 
                Conflict(new { error = error.Message }),
            _ => StatusCode(500, new { error = "Error interno del servidor" })
        };
    }
}
```

### API Controller Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class CarritoApiController : ControllerBase
{
    private readonly ICarritoService _carritoService;

    [HttpPost]
    public async Task<ActionResult<CarritoItemDto>> AddToCarrito(
        [FromBody] AddToCarritoRequest request)
    {
        var userId = GetCurrentUserId();
        
        var result = await _carritoService.AddToCarritoAsync(
            userId, 
            request.ProductoId, 
            request.Cantidad);

        // Map Result to ActionResult
        return result.Match<ActionResult<CarritoItemDto>>(
            success => Ok(MapToDto(success)),
            error => error switch
            {
                ProductError => NotFound(new ErrorResponse(error)),
                CarritoError { Code: "PRODUCT_NOT_AVAILABLE" } => 
                    BadRequest(new ErrorResponse(error)),
                _ => StatusCode(500, new ErrorResponse(error))
            });
    }
}
```

## 🧪 Testing with ROP

### Unit Test Example

```csharp
[Test]
public async Task AddToCarritoAsync_ShouldReturnFailure_WhenProductNotFound()
{
    // Arrange
    var usuarioId = 1L;
    var productoId = 999L;

    // Act
    var result = await _carritoService.AddToCarritoAsync(usuarioId, productoId, 1);

    // Assert
    result.IsFailure.Should().BeTrue();
    result.Error.Should().BeOfType<ProductError>();
    result.Error.Code.Should().Be("PRODUCT_NOT_FOUND");
    result.Error.Message.Should().Contain("999");
}

[Test]
public async Task AddToCarritoAsync_ShouldReturnSuccess_WhenValidRequest()
{
    // Arrange
    var usuario = CreateTestUser(1, "test@test.com");
    var producto = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
    await _context.Users.AddAsync(usuario);
    await _context.Products.AddAsync(producto);
    await _context.SaveChangesAsync();

    // Act
    var result = await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id, 2);

    // Assert
    result.IsSuccess.Should().BeTrue();
    result.Value.Should().NotBeNull();
    result.Value.Cantidad.Should().Be(2);
    result.Value.Subtotal.Should().Be(20.00m);
}
```

## 📊 Benefits Summary

| Aspect | Traditional Exceptions | Railway Oriented |
|--------|----------------------|------------------|
| **Performance** | Slow (stack unwinding) | Fast (no exceptions) |
| **Type Safety** | Runtime errors | Compile-time safety |
| **Composability** | Hard to chain operations | Easy to compose |
| **Readability** | Try-catch nesting | Linear flow |
| **Testing** | Must test exceptions | Test return values |
| **Documentation** | Hidden error paths | Explicit in signature |

## 🎓 Best Practices

### ✅ DO

1. **Use specific error types**
```csharp
// Good
return Result.Failure<Product, DomainError>(ProductError.NotFound(id));

// Bad
return Result.Failure<Product, DomainError>(
    new GenericError("ERROR", "Not found"));
```

2. **Provide context in error messages**
```csharp
// Good
ProductError.NotFound(id) => 
    new("PRODUCT_NOT_FOUND", $"Producto con ID {id} no encontrado");

// Bad
new("NOT_FOUND", "Not found");
```

3. **Keep error codes consistent**
```csharp
// Use UPPER_SNAKE_CASE for codes
"PRODUCT_NOT_FOUND"
"CARRITO_CONCURRENCY_CONFLICT"
"INVALID_QUANTITY"
```

### ❌ DON'T

1. **Don't use Result for all exceptions**
```csharp
// Bad - ArgumentNullException should still be thrown
if (service == null)
    return Result.Failure<T, DomainError>(/* ... */);

// Good - Use exceptions for programming errors
if (service == null)
    throw new ArgumentNullException(nameof(service));
```

2. **Don't ignore the error**
```csharp
// Bad
var result = await _service.DoSomethingAsync();
var value = result.Value; // May throw if IsFailure!

// Good
var result = await _service.DoSomethingAsync();
if (result.IsFailure)
    return HandleError(result.Error);
var value = result.Value;
```

3. **Don't mix patterns**
```csharp
// Bad - inconsistent error handling
public async Task<Result<Product, DomainError>> GetAsync(long id)
{
    if (id <= 0)
        throw new ArgumentException("Invalid ID"); // Don't mix!
    
    var product = await _context.Products.FindAsync(id);
    return product != null
        ? Result.Success<Product, DomainError>(product)
        : Result.Failure<Product, DomainError>(ProductError.NotFound(id));
}
```

## 📚 Further Reading

- [Railway Oriented Programming](https://fsharpforfunandprofit.com/rop/) - Original article
- [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions) - Library we use
- [Functional C#](https://github.com/louthy/language-ext) - Alternative functional library
- [Error Handling in a Func](https://www.pluralsight.com/courses/error-handling-functional-way) - Pluralsight course

---

**Author**: TiendaDawWeb Team  
**Last Updated**: 2026-01-01  
**Version**: 1.0
