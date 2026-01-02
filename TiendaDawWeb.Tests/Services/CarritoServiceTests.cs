using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Implementations;

namespace TiendaDawWeb.Tests.Services;

/// <summary>
/// OBJETIVO: Validar la gestión del carrito de compras del usuario.
/// LO QUE BUSCA: Verificar que los productos se añaden, eliminan y suman correctamente,
/// respetando las reglas de disponibilidad y unicidad.
/// </summary>
[TestFixture]
public class CarritoServiceTests
{
    private ApplicationDbContext _context = null!;
    private CarritoService _carritoService = null!;
    private Mock<ILogger<CarritoService>> _loggerMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CarritoService>>();
        _carritoService = new CarritoService(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    /// <summary>
    /// PRUEBA: Añadir producto disponible al carrito.
    /// OBJETIVO: Confirmar que un producto existente y libre puede ser agregado.
    /// </summary>
    [Test]
    public async Task AddToCarritoAsync_ShouldAddNewItem_WhenProductIsAvailable()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddAsync(producto);
        await _context.SaveChangesAsync();

        // Act
        var result = await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Precio.Should().Be(10.00m);
        result.Value.ProductoId.Should().Be(producto.Id);
    }

    /// <summary>
    /// PRUEBA: Duplicidad en el carrito.
    /// OBJETIVO: Verificar que no se puede añadir el mismo producto dos veces al carrito del mismo usuario.
    /// </summary>
    [Test]
    public async Task AddToCarritoAsync_ShouldFail_WhenItemAlreadyExists()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddAsync(producto);
        await _context.SaveChangesAsync();

        await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id);

        // Act - Reintento de inserción
        var result = await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<CarritoError>();
    }

    /// <summary>
    /// PRUEBA: Producto inexistente.
    /// OBJETIVO: Validar el manejo de errores cuando se intenta añadir un ID que no existe en BD.
    /// </summary>
    [Test]
    public async Task AddToCarritoAsync_ShouldFail_WhenProductDoesNotExist()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        await _context.Users.AddAsync(usuario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _carritoService.AddToCarritoAsync(usuario.Id, 999);

        // Assert
        result.IsFailure.Should().BeTrue();
    }

    /// <summary>
    /// PRUEBA: Producto reservado.
    /// OBJETIVO: Asegurar que un producto que ya está reservado por otro proceso no puede entrar al carrito.
    /// </summary>
    [Test]
    public async Task AddToCarritoAsync_ShouldFail_WhenProductIsReserved()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        producto.Reservado = true;
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddAsync(producto);
        await _context.SaveChangesAsync();

        // Act
        var result = await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PRODUCT_NOT_AVAILABLE");
    }

    /// <summary>
    /// PRUEBA: Eliminación de ítem.
    /// OBJETIVO: Confirmar que un producto puede ser removido individualmente del carrito.
    /// </summary>
    [Test]
    public async Task RemoveFromCarritoAsync_ShouldRemoveItem_WhenItemExists()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddAsync(producto);
        await _context.SaveChangesAsync();

        var addResult = await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id);
        var itemId = addResult.Value.Id;

        // Act
        var result = await _carritoService.RemoveFromCarritoAsync(itemId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        (await _context.CarritoItems.CountAsync()).Should().Be(0);
    }

    /// <summary>
    /// PRUEBA: Vaciado del carrito.
    /// OBJETIVO: Verificar que todos los ítems de un usuario desaparecen tras ejecutar 'Clear'.
    /// </summary>
    [Test]
    public async Task ClearCarritoAsync_ShouldRemoveAllItems_WhenCarritoHasItems()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var p1 = CreateTestProduct(1, "P1", 10m, usuario.Id);
        var p2 = CreateTestProduct(2, "P2", 20m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddRangeAsync(p1, p2);
        await _context.SaveChangesAsync();

        await _carritoService.AddToCarritoAsync(usuario.Id, p1.Id);
        await _carritoService.AddToCarritoAsync(usuario.Id, p2.Id);

        // Act
        var result = await _carritoService.ClearCarritoAsync(usuario.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        (await _context.CarritoItems.Where(c => c.UsuarioId == usuario.Id).CountAsync()).Should().Be(0);
    }

    /// <summary>
    /// PRUEBA: Cálculo del total.
    /// OBJETIVO: Validar que la suma de precios de los productos en el carrito es matemática y financieramente exacta.
    /// </summary>
    [Test]
    public async Task GetTotalCarritoAsync_ShouldReturnCorrectTotal()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var p1 = CreateTestProduct(1, "P1", 10.50m, usuario.Id);
        var p2 = CreateTestProduct(2, "P2", 20.40m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddRangeAsync(p1, p2);
        await _context.SaveChangesAsync();

        await _carritoService.AddToCarritoAsync(usuario.Id, p1.Id);
        await _carritoService.AddToCarritoAsync(usuario.Id, p2.Id);

        // Act
        var result = await _carritoService.GetTotalCarritoAsync(usuario.Id);

        // Assert
        result.Value.Should().Be(30.90m); // 10.50 + 20.40
    }

    /// <summary>
    /// PRUEBA: Contador de ítems.
    /// OBJETIVO: Verificar que el método devuelve el número correcto de productos únicos en el carrito.
    /// </summary>
    [Test]
    public async Task GetCarritoCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var p1 = CreateTestProduct(1, "P1", 10m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddAsync(p1);
        await _context.SaveChangesAsync();

        await _carritoService.AddToCarritoAsync(usuario.Id, p1.Id);

        // Act
        var result = await _carritoService.GetCarritoCountAsync(usuario.Id);

        // Assert
        result.Value.Should().Be(1);
    }

    /// <summary>
    /// PRUEBA: Recuperación de lista.
    /// OBJETIVO: Validar que se recuperan todos los objetos CarritoItem vinculados a un usuario específico.
    /// </summary>
    [Test]
    public async Task GetCarritoByUsuarioIdAsync_ShouldReturnAllItems()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var p1 = CreateTestProduct(1, "P1", 10m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddAsync(p1);
        await _context.SaveChangesAsync();

        await _carritoService.AddToCarritoAsync(usuario.Id, p1.Id);

        // Act
        var result = await _carritoService.GetCarritoByUsuarioIdAsync(usuario.Id);

        // Assert
        result.Value.Should().HaveCount(1);
    }

    // Helper methods
    private static User CreateTestUser(long id, string email)
    {
        return new User
        {
            Id = id,
            UserName = email,
            Email = email,
            Nombre = "Test",
            Apellidos = "User",
            Rol = "USER",
            NormalizedUserName = email.ToUpper(),
            NormalizedEmail = email.ToUpper(),
            EmailConfirmed = true
        };
    }

    private static Product CreateTestProduct(long id, string nombre, decimal precio, long propietarioId)
    {
        return new Product
        {
            Id = id,
            Nombre = nombre,
            Descripcion = "Test product",
            Precio = precio,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = propietarioId,
            Imagen = "test.jpg",
            Reservado = false
        };
    }
}
