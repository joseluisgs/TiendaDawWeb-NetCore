using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Carrito;

namespace TiendaDawWeb.Tests.Shared.Services.Carrito;

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

    #endregion

    #region AddToCarritoAsync Tests

    [Test]
    public async Task AddToCarritoAsync_AddsItem()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Product", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, Deleted = false };
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
    }

    [Test]
    public async Task AddToCarritoAsync_ReturnsFailure_ProductDeleted()
    {
        var product = new Product { Id = 1, Nombre = "Product", Deleted = true };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddToCarritoAsync(1, 1);
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task AddToCarritoAsync_ReturnsFailure_ProductAlreadySold()
    {
        var product = new Product { Id = 1, Nombre = "Product", Deleted = false, CompraId = 1 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddToCarritoAsync(1, 1);
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task AddToCarritoAsync_ReturnsFailure_ProductAlreadyInCart()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Product", Deleted = false };
        var existingItem = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100 };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.CarritoItems.Add(existingItem);
        await _context.SaveChangesAsync();

        var result = await _service.AddToCarritoAsync(1, 1);
        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task AddToCarritoAsync_SetsReservation()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Product", Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddToCarritoAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        var updatedProduct = await _context.Products.FindAsync(1L);
        updatedProduct!.Reservado.Should().BeTrue();
    }

    #endregion

    #region RemoveFromCarritoAsync Tests

    [Test]
    public async Task RemoveFromCarritoAsync_RemovesItem()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Product", Deleted = false };
        var item = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100 };
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
        var product = new Product
        {
            Id = 1,
            Nombre = "Product",
            Deleted = false,
            Reservado = true,
            ReservadoPor = 1,
            ReservadoHasta = DateTime.UtcNow.AddMinutes(5)
        };
        var item = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100 };
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
        var item1 = new CarritoItem { Id = 1, UsuarioId = 1, ProductoId = 1, Precio = 100 };
        var item2 = new CarritoItem { Id = 2, UsuarioId = 1, ProductoId = 2, Precio = 200 };
        _context.Users.Add(user);
        _context.CarritoItems.AddRange(item1, item2);
        await _context.SaveChangesAsync();

        var result = await _service.ClearCarritoAsync(1);

        result.IsSuccess.Should().BeTrue();
        _context.CarritoItems.Should().BeEmpty();
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
    public async Task GetTotalCarritoAsync_ReturnsZero_WhenEmpty()
    {
        var result = await _service.GetTotalCarritoAsync(999);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    #endregion

    #region GetCarritoCountAsync Tests

    [Test]
    public async Task GetCarritoCountAsync_ReturnsZero_WhenEmpty()
    {
        var result = await _service.GetCarritoCountAsync(999);
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    #endregion
}
