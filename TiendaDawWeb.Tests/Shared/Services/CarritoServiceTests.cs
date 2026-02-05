using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Carrito;

namespace TiendaDawWeb.Tests.Shared.Services;

public class CarritoServiceTests
{
    private ApplicationDbContext _context = null!;
    private CarritoService _service = null!;
    private Mock<ILogger<CarritoService>> _loggerMock = null!;
    private MemoryCache _memoryCache = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<CarritoService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new CarritoService(_context, _memoryCache, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _memoryCache.Dispose();
    }

    #region GetCarritoByUsuarioIdAsync Tests

    [Test]
    public async Task GetCarritoByUsuarioIdAsync_ReturnsEmptyList_WhenEmpty()
    {
        var result = await _service.GetCarritoByUsuarioIdAsync(1);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();
    }

    [Test, Explicit("Skip: EF Core in-memory database limitation with Include on navigation properties")]
    public async Task GetCarritoByUsuarioIdAsync_ReturnsItems_WhenExist()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var item = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100, CreatedAt = DateTime.UtcNow };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.CarritoItems.Add(item);
        await _context.SaveChangesAsync();

        var result = await _service.GetCarritoByUsuarioIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    #endregion

    #region AddToCarritoAsync Tests

    [Test]
    public async Task AddToCarritoAsync_AddsItem()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false, CompraId = null };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddToCarritoAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        _context.CarritoItems.Should().HaveCount(1);
    }

    [Test]
    public async Task AddToCarritoAsync_ReturnsFailure_ProductNotFound()
    {
        var result = await _service.AddToCarritoAsync(1, 999);

        result.IsSuccess.Should().BeFalse();
        _context.CarritoItems.Should().BeEmpty();
    }

    [Test]
    public async Task AddToCarritoAsync_ReturnsFailure_ProductDeleted()
    {
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, Deleted = true };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddToCarritoAsync(1, 1);

        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task AddToCarritoAsync_ReturnsFailure_ProductAlreadySold()
    {
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, Deleted = false, CompraId = 1 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddToCarritoAsync(1, 1);

        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task AddToCarritoAsync_ReturnsFailure_ProductAlreadyInCart()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false, CompraId = null };
        var existingItem = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100 };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.CarritoItems.Add(existingItem);
        await _context.SaveChangesAsync();

        var result = await _service.AddToCarritoAsync(1, 1);

        result.IsSuccess.Should().BeFalse();
        _context.CarritoItems.Should().HaveCount(1);
    }

    [Test]
    public async Task AddToCarritoAsync_ReturnsFailure_ProductReservedByOther()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product
        {
            Id = 1,
            Nombre = "Test",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.SMARTPHONES,
            Deleted = false,
            Reservado = true,
            ReservadoPor = 2,
            ReservadoHasta = DateTime.UtcNow.AddMinutes(5)
        };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddToCarritoAsync(1, 1);

        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task AddToCarritoAsync_AllowsWhenReservationExpired()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product
        {
            Id = 1,
            Nombre = "Test",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.SMARTPHONES,
            Deleted = false,
            Reservado = true,
            ReservadoPor = 2,
            ReservadoHasta = DateTime.UtcNow.AddMinutes(-5)
        };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddToCarritoAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task AddToCarritoAsync_SetsReservation()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddToCarritoAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        var updatedProduct = await _context.Products.FindAsync(1L);
        updatedProduct!.Reservado.Should().BeTrue();
        updatedProduct.ReservadoPor.Should().Be(1);
    }

    #endregion

    #region RemoveFromCarritoAsync Tests

    [Test]
    public async Task RemoveFromCarritoAsync_RemovesItem()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var item = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100, CreatedAt = DateTime.UtcNow };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.CarritoItems.Add(item);
        await _context.SaveChangesAsync();

        var result = await _service.RemoveFromCarritoAsync(1);

        result.IsSuccess.Should().BeTrue();
        _context.CarritoItems.Should().BeEmpty();
    }

    [Test]
    public async Task RemoveFromCarritoAsync_ReleasesProductReservation()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product
        {
            Id = 1,
            Nombre = "Test",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.SMARTPHONES,
            Deleted = false,
            Reservado = true,
            ReservadoPor = 1,
            ReservadoHasta = DateTime.UtcNow.AddMinutes(5)
        };
        var item = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100 };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.CarritoItems.Add(item);
        await _context.SaveChangesAsync();

        var result = await _service.RemoveFromCarritoAsync(1);

        result.IsSuccess.Should().BeTrue();
        var updatedProduct = await _context.Products.FindAsync(1L);
        updatedProduct!.Reservado.Should().BeFalse();
    }

    [Test]
    public async Task RemoveFromCarritoAsync_ReturnsFailure_ItemNotFound()
    {
        var result = await _service.RemoveFromCarritoAsync(999);

        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region ClearCarritoAsync Tests

    [Test]
    public async Task ClearCarritoAsync_ClearsAllItems()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product1 = new Product { Id = 1, Nombre = "P1", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, Deleted = false };
        var product2 = new Product { Id = 2, Nombre = "P2", Descripcion = "Desc", Precio = 200, Categoria = ProductCategory.ACCESSORIES, Deleted = false };
        var item1 = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100 };
        var item2 = new CarritoItem { Id = 2, UsuarioId = 1, ProductoId = 2, Precio = 200 };
        _context.Users.Add(user);
        _context.Products.AddRange(product1, product2);
        _context.CarritoItems.AddRange(item1, item2);
        await _context.SaveChangesAsync();

        var result = await _service.ClearCarritoAsync(1);

        result.IsSuccess.Should().BeTrue();
        _context.CarritoItems.Should().BeEmpty();
    }

    [Test]
    public async Task ClearCarritoAsync_ReleasesAllReservations()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product1 = new Product
        {
            Id = 1, Nombre = "P1", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, Deleted = false,
            Reservado = true, ReservadoPor = 1
        };
        var product2 = new Product
        {
            Id = 2, Nombre = "P2", Descripcion = "Desc", Precio = 200, Categoria = ProductCategory.ACCESSORIES, Deleted = false,
            Reservado = true, ReservadoPor = 1
        };
        var item1 = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100 };
        var item2 = new CarritoItem { Id = 2, UsuarioId = 1, ProductoId = 2, Precio = 200 };
        _context.Users.Add(user);
        _context.Products.AddRange(product1, product2);
        _context.CarritoItems.AddRange(item1, item2);
        await _context.SaveChangesAsync();

        var result = await _service.ClearCarritoAsync(1);

        result.IsSuccess.Should().BeTrue();
        var products = await _context.Products.ToListAsync();
        products.All(p => !p.Reservado).Should().BeTrue();
    }

    [Test]
    public async Task ClearCarritoAsync_ReturnsSuccess_WhenEmpty()
    {
        var result = await _service.ClearCarritoAsync(999);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region GetTotalCarritoAsync Tests

    [Test]
    public async Task GetTotalCarritoAsync_ReturnsTotal()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product1 = new Product { Id = 1, Nombre = "P1", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var product2 = new Product { Id = 2, Nombre = "P2", Descripcion = "Desc", Precio = 200, Categoria = ProductCategory.LAPTOPS, PropietarioId = 3, Deleted = false };
        var item1 = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100, CreatedAt = DateTime.UtcNow };
        var item2 = new CarritoItem { Id = 2, UsuarioId = 1, ProductoId = 2, Precio = 200, CreatedAt = DateTime.UtcNow };
        _context.Users.Add(user);
        _context.Products.AddRange(product1, product2);
        _context.CarritoItems.AddRange(item1, item2);
        await _context.SaveChangesAsync();

        var result = await _service.GetTotalCarritoAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(300);
    }

    [Test]
    public async Task GetTotalCarritoAsync_ReturnsZero_WhenEmpty()
    {
        var result = await _service.GetTotalCarritoAsync(999);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    #endregion

    #region GetCarritoCountAsync Tests

    [Test]
    public async Task GetCarritoCountAsync_ReturnsCount()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product1 = new Product { Id = 1, Nombre = "P1", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var product2 = new Product { Id = 2, Nombre = "P2", Descripcion = "Desc", Precio = 200, Categoria = ProductCategory.LAPTOPS, PropietarioId = 3, Deleted = false };
        var item1 = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100, CreatedAt = DateTime.UtcNow };
        var item2 = new CarritoItem { Id = 2, UsuarioId = 1, ProductoId = 2, Precio = 200, CreatedAt = DateTime.UtcNow };
        _context.Users.Add(user);
        _context.Products.AddRange(product1, product2);
        _context.CarritoItems.AddRange(item1, item2);
        await _context.SaveChangesAsync();

        var result = await _service.GetCarritoCountAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(2);
    }

    [Test]
    public async Task GetCarritoCountAsync_ReturnsZero_WhenEmpty()
    {
        var result = await _service.GetCarritoCountAsync(999);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    #endregion
}
