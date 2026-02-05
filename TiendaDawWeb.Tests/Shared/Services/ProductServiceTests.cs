using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Product;

namespace TiendaDawWeb.Tests.Shared.Services;

public class ProductServiceTests
{
    private ApplicationDbContext _context = null!;
    private ProductService _service = null!;
    private Mock<ILogger<ProductService>> _loggerMock = null!;
    private MemoryCache _memoryCache = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<ProductService>>();
        _memoryCache = new MemoryCache(new MemoryCacheOptions());
        _service = new ProductService(_context, _memoryCache, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _memoryCache.Dispose();
    }

    [Test]
    public async Task GetByIdAsync_ReturnsProduct_WhenExists()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Propietario = user, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Nombre.Should().Be("Test");
    }

    [Test]
    public async Task GetByIdAsync_ReturnsFailure_WhenNotExists()
    {
        var result = await _service.GetByIdAsync(999);

        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task GetAllAsync_ReturnsAllProducts()
    {
        var user1 = new User { Id = 1, Email = "test1@test.com", UserName = "test1" };
        var user2 = new User { Id = 2, Email = "test2@test.com", UserName = "test2" };
        _context.Products.AddRange(
            new Product { Id = 1, Nombre = "P1", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Propietario = user1, Deleted = false, CompraId = null },
            new Product { Id = 2, Nombre = "P2", Descripcion = "Desc", Precio = 200, Categoria = ProductCategory.ACCESSORIES, PropietarioId = 2, Propietario = user2, Deleted = false, CompraId = null }
        );
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Test]
    public async Task SearchAsync_FiltersByName()
    {
        var user1 = new User { Id = 1, Email = "test1@test.com", UserName = "test1" };
        var user2 = new User { Id = 2, Email = "test2@test.com", UserName = "test2" };
        _context.Products.AddRange(
            new Product { Id = 1, Nombre = "iPhone", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Propietario = user1, Deleted = false, CompraId = null },
            new Product { Id = 2, Nombre = "Samsung", Descripcion = "Desc", Precio = 200, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Propietario = user2, Deleted = false, CompraId = null }
        );
        _context.Users.AddRange(user1, user2);
        await _context.SaveChangesAsync();

        var result = await _service.SearchAsync("iPhone", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Nombre.Should().Be("iPhone");
    }

    [Test]
    public async Task CreateAsync_CreatesProduct()
    {
        var product = new Product { Nombre = "New", Descripcion = "Desc", Precio = 150, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };

        var result = await _service.CreateAsync(product);

        result.IsSuccess.Should().BeTrue();
        result.Value.Nombre.Should().Be("New");
        _context.Products.Should().HaveCount(1);
    }

    [Test, Explicit("Skip: EF Core in-memory database limitation with Include on null navigation properties")]
    public async Task DeleteAsync_MarksAsDeleted()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Propietario = user, Deleted = false, CompraId = null, Compra = null };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(1, 1, false);

        result.IsSuccess.Should().BeTrue();
        var deletedProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == 1);
        deletedProduct.Should().NotBeNull();
        deletedProduct!.Deleted.Should().BeTrue();
    }
}
