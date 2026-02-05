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

    #region GetByIdAsync Tests

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
    public async Task GetByIdAsync_ReturnsFailure_WhenDeleted()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Deleted", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Propietario = user, Deleted = true };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(1);

        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region GetAllAsync Tests

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
    public async Task GetAllAsync_ReturnsEmpty_WhenNoProducts()
    {
        var result = await _service.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task GetAllAsync_ExcludesDeletedProducts()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        _context.Products.AddRange(
            new Product { Id = 1, Nombre = "Active", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false },
            new Product { Id = 2, Nombre = "Deleted", Descripcion = "Desc", Precio = 200, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = true }
        );
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Nombre.Should().Be("Active");
    }

    [Test]
    public async Task GetAllAsync_ExcludesSoldProducts()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        _context.Products.AddRange(
            new Product { Id = 1, Nombre = "Available", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false, CompraId = null },
            new Product { Id = 2, Nombre = "Sold", Descripcion = "Desc", Precio = 200, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false, CompraId = 1 }
        );
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.GetAllAsync();

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Nombre.Should().Be("Available");
    }

    #endregion

    #region SearchAsync Tests

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
    public async Task SearchAsync_FiltersByCategory()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        _context.Products.AddRange(
            new Product { Id = 1, Nombre = "Phone", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false, CompraId = null },
            new Product { Id = 2, Nombre = "Cable", Descripcion = "Desc", Precio = 50, Categoria = ProductCategory.ACCESSORIES, PropietarioId = 1, Deleted = false, CompraId = null }
        );
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.SearchAsync(null, "SMARTPHONES");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Categoria.Should().Be(ProductCategory.SMARTPHONES);
    }

    [Test]
    public async Task SearchAsync_FiltersByNameAndCategory()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        _context.Products.AddRange(
            new Product { Id = 1, Nombre = "iPhone", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false, CompraId = null },
            new Product { Id = 2, Nombre = "iPhone Case", Descripcion = "Desc", Precio = 50, Categoria = ProductCategory.ACCESSORIES, PropietarioId = 1, Deleted = false, CompraId = null }
        );
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.SearchAsync("iPhone", "SMARTPHONES");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Nombre.Should().Be("iPhone");
    }

    [Test]
    public async Task SearchAsync_ReturnsEmpty_WhenNoMatch()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        _context.Products.Add(
            new Product { Id = 1, Nombre = "Phone", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false, CompraId = null }
        );
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.SearchAsync("NonExistent", "SMARTPHONES");

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task SearchAsync_CaseInsensitive()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        _context.Products.Add(
            new Product { Id = 1, Nombre = "IPHONE", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false, CompraId = null }
        );
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.SearchAsync("IPHONE", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    #endregion

    #region CreateAsync Tests

    [Test]
    public async Task CreateAsync_CreatesProduct()
    {
        var product = new Product { Nombre = "New", Descripcion = "Desc", Precio = 150, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };

        var result = await _service.CreateAsync(product);

        result.IsSuccess.Should().BeTrue();
        result.Value.Nombre.Should().Be("New");
        _context.Products.Should().HaveCount(1);
    }

    [Test]
    public async Task CreateAsync_GeneratesId()
    {
        var product = new Product { Nombre = "New", Descripcion = "Desc", Precio = 150, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };

        var result = await _service.CreateAsync(product);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().BeGreaterThan(0);
    }

    [Test]
    public async Task CreateAsync_SetsCreatedAt()
    {
        var before = DateTime.UtcNow.AddSeconds(-1);
        var product = new Product { Nombre = "New", Descripcion = "Desc", Precio = 150, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };

        var result = await _service.CreateAsync(product);

        result.IsSuccess.Should().BeTrue();
        result.Value.CreatedAt.Should().BeAfter(before);
    }

    #endregion

    #region UpdateAsync Tests

    [Test]
    public async Task UpdateAsync_UpdatesProduct()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Old Name", Descripcion = "Old Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var update = new Product { Id = 1, Nombre = "New Name", Descripcion = "New Desc", Precio = 150, Categoria = ProductCategory.ACCESSORIES, PropietarioId = 1, Deleted = false };

        var result = await _service.UpdateAsync(1, update, 1);

        result.IsSuccess.Should().BeTrue();
        var updated = await _context.Products.FindAsync(1L);
        updated!.Nombre.Should().Be("New Name");
        updated.Precio.Should().Be(150);
    }

    [Test]
    public async Task UpdateAsync_ReturnsFailure_WhenNotExists()
    {
        var product = new Product { Id = 999, Nombre = "Ghost", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };

        var result = await _service.UpdateAsync(999, product, 1);

        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task UpdateAsync_OnlyUpdatesAllowedFields()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Original", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var beforeCreatedAt = product.CreatedAt;
        var update = new Product { Id = 1, Nombre = "Updated", Descripcion = "Updated Desc", Precio = 200, Categoria = ProductCategory.ACCESSORIES, PropietarioId = 1, Deleted = false };

        var result = await _service.UpdateAsync(1, update, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Nombre.Should().Be("Updated");
        result.Value.Precio.Should().Be(200);
    }

    #endregion

    #region DeleteAsync Tests

    [Test]
    public async Task DeleteAsync_MarksAsDeleted()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(1, 1, false);

        result.IsSuccess.Should().BeTrue();
        var deletedProduct = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == 1);
        deletedProduct.Should().NotBeNull();
        deletedProduct!.Deleted.Should().BeTrue();
    }

    [Test]
    public async Task DeleteAsync_ReturnsFailure_WhenNotExists()
    {
        var result = await _service.DeleteAsync(999, 1, false);

        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task DeleteAsync_AdminCanDeleteAnyProduct()
    {
        var owner = new User { Id = 1, Email = "owner@test.com", UserName = "owner" };
        var admin = new User { Id = 2, Email = "admin@test.com", UserName = "admin" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Users.AddRange(owner, admin);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteAsync(1, 2, true);

        result.IsSuccess.Should().BeTrue();
        var deletedProduct = await _context.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == 1);
        deletedProduct.Should().NotBeNull();
        deletedProduct!.Deleted.Should().BeTrue();
    }

    #endregion
}
