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
        var result = await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id, 2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Cantidad.Should().Be(2);
        result.Value.Subtotal.Should().Be(20.00m);
        result.Value.ProductoId.Should().Be(producto.Id);
        result.Value.UsuarioId.Should().Be(usuario.Id);
    }

    [Test]
    public async Task AddToCarritoAsync_ShouldUpdateQuantity_WhenItemAlreadyExists()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddAsync(producto);
        await _context.SaveChangesAsync();

        // Add item first time
        await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id, 1);

        // Act - add again
        var result = await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id, 2);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Cantidad.Should().Be(3); // 1 + 2
        result.Value.Subtotal.Should().Be(30.00m);

        // Verify only one item exists
        var items = await _context.CarritoItems.ToListAsync();
        items.Should().HaveCount(1);
    }

    [Test]
    public async Task AddToCarritoAsync_ShouldFail_WhenProductDoesNotExist()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        await _context.Users.AddAsync(usuario);
        await _context.SaveChangesAsync();

        // Act
        var result = await _carritoService.AddToCarritoAsync(usuario.Id, 999, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeAssignableTo<DomainError>();
    }

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
        var result = await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id, 1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<CarritoError>();
        result.Error.Code.Should().Be("PRODUCT_NOT_AVAILABLE");
    }

    [Test]
    public async Task AddToCarritoAsync_ShouldFail_WhenQuantityIsInvalid()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddAsync(producto);
        await _context.SaveChangesAsync();

        // Act
        var result = await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id, -1);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<CarritoError>();
        result.Error.Code.Should().Be("INVALID_QUANTITY");
    }

    [Test]
    public async Task UpdateCantidadAsync_ShouldUpdateQuantity_WhenItemExists()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddAsync(producto);
        await _context.SaveChangesAsync();

        var addResult = await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id, 2);
        var itemId = addResult.Value.Id;

        // Act
        var result = await _carritoService.UpdateCantidadAsync(itemId, 5);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Cantidad.Should().Be(5);
        result.Value.Subtotal.Should().Be(50.00m);
    }

    [Test]
    public async Task UpdateCantidadAsync_ShouldFail_WhenItemDoesNotExist()
    {
        // Act
        var result = await _carritoService.UpdateCantidadAsync(999, 5);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeOfType<CarritoError>();
        result.Error.Code.Should().Be("CARRITO_ITEM_NOT_FOUND");
    }

    [Test]
    public async Task RemoveFromCarritoAsync_ShouldRemoveItem_WhenItemExists()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddAsync(producto);
        await _context.SaveChangesAsync();

        var addResult = await _carritoService.AddToCarritoAsync(usuario.Id, producto.Id, 2);
        var itemId = addResult.Value.Id;

        // Act
        var result = await _carritoService.RemoveFromCarritoAsync(itemId);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        var items = await _context.CarritoItems.ToListAsync();
        items.Should().BeEmpty();
    }

    [Test]
    public async Task ClearCarritoAsync_ShouldRemoveAllItems_WhenCarritoHasItems()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto1 = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        var producto2 = CreateTestProduct(2, "Product 2", 20.00m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddRangeAsync(producto1, producto2);
        await _context.SaveChangesAsync();

        await _carritoService.AddToCarritoAsync(usuario.Id, producto1.Id, 1);
        await _carritoService.AddToCarritoAsync(usuario.Id, producto2.Id, 2);

        // Act
        var result = await _carritoService.ClearCarritoAsync(usuario.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();

        var items = await _context.CarritoItems.Where(c => c.UsuarioId == usuario.Id).ToListAsync();
        items.Should().BeEmpty();
    }

    [Test]
    public async Task GetTotalCarritoAsync_ShouldReturnCorrectTotal()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto1 = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        var producto2 = CreateTestProduct(2, "Product 2", 20.00m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddRangeAsync(producto1, producto2);
        await _context.SaveChangesAsync();

        await _carritoService.AddToCarritoAsync(usuario.Id, producto1.Id, 2); // 20
        await _carritoService.AddToCarritoAsync(usuario.Id, producto2.Id, 3); // 60

        // Act
        var result = await _carritoService.GetTotalCarritoAsync(usuario.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(80.00m);
    }

    [Test]
    public async Task GetCarritoCountAsync_ShouldReturnCorrectCount()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto1 = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        var producto2 = CreateTestProduct(2, "Product 2", 20.00m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddRangeAsync(producto1, producto2);
        await _context.SaveChangesAsync();

        await _carritoService.AddToCarritoAsync(usuario.Id, producto1.Id, 2);
        await _carritoService.AddToCarritoAsync(usuario.Id, producto2.Id, 3);

        // Act
        var result = await _carritoService.GetCarritoCountAsync(usuario.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5); // 2 + 3
    }

    [Test]
    public async Task GetCarritoByUsuarioIdAsync_ShouldReturnAllItems()
    {
        // Arrange
        var usuario = CreateTestUser(1, "test@test.com");
        var producto1 = CreateTestProduct(1, "Product 1", 10.00m, usuario.Id);
        var producto2 = CreateTestProduct(2, "Product 2", 20.00m, usuario.Id);
        await _context.Users.AddAsync(usuario);
        await _context.Products.AddRangeAsync(producto1, producto2);
        await _context.SaveChangesAsync();

        await _carritoService.AddToCarritoAsync(usuario.Id, producto1.Id, 1);
        await _carritoService.AddToCarritoAsync(usuario.Id, producto2.Id, 2);

        // Act
        var result = await _carritoService.GetCarritoByUsuarioIdAsync(usuario.Id);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
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
