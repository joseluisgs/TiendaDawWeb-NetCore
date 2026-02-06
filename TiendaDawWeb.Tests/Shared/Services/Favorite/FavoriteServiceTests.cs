using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Favorite;

namespace TiendaDawWeb.Tests.Shared.Services;

public class FavoriteServiceTests
{
    private ApplicationDbContext _context = null!;
    private FavoriteService _service = null!;
    private Mock<ILogger<FavoriteService>> _loggerMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<FavoriteService>>();
        _service = new FavoriteService(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    #region AddFavoriteAsync Tests

    [Test]
    public async Task AddFavoriteAsync_AddsFavorite()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Product", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddFavoriteAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        _context.Favorites.Should().HaveCount(1);
    }

    [Test]
    public async Task AddFavoriteAsync_ProductNotExists_AddsFavorite()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.AddFavoriteAsync(1, 999);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task AddFavoriteAsync_UserNotExists_AddsFavorite()
    {
        var product = new Product { Id = 1, Nombre = "Product", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddFavoriteAsync(999, 1);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task AddFavoriteAsync_AlreadyFavorite_ReturnsFailure()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Product", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.Favorites.Add(new Favorite { UsuarioId = 1, ProductoId = 1 });
        await _context.SaveChangesAsync();

        var result = await _service.AddFavoriteAsync(1, 1);

        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task AddFavoriteAsync_OwnProduct_Succeeds()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Product", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddFavoriteAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region RemoveFavoriteAsync Tests

    [Test]
    public async Task RemoveFavoriteAsync_RemovesFavorite()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Product", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.Favorites.Add(new Favorite { UsuarioId = 1, ProductoId = 1 });
        await _context.SaveChangesAsync();

        var result = await _service.RemoveFavoriteAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        _context.Favorites.Should().BeEmpty();
    }

    [Test]
    public async Task RemoveFavoriteAsync_ReturnsFailure_WhenNotFavorite()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Product", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.RemoveFavoriteAsync(1, 1);

        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task RemoveFavoriteAsync_ReturnsFailure_WhenFavoriteNotExists()
    {
        var result = await _service.RemoveFavoriteAsync(1, 1);

        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region IsFavoriteAsync Tests

    [Test]
    public async Task IsFavoriteAsync_ReturnsTrue_WhenIsFavorite()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var favorite = new Favorite { UsuarioId = 1, ProductoId = 1 };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.Favorites.Add(favorite);
        await _context.SaveChangesAsync();

        var result = await _service.IsFavoriteAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Test]
    public async Task IsFavoriteAsync_ReturnsFalse_WhenNotFavorite()
    {
        var result = await _service.IsFavoriteAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Test]
    public async Task IsFavoriteAsync_ReturnsFalse_WhenUserNotExists()
    {
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.IsFavoriteAsync(999, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    #endregion

    #region GetUserFavoritesAsync Tests

    [Test]
    public async Task GetUserFavoritesAsync_ReturnsEmpty_WhenNoFavorites()
    {
        var result = await _service.GetUserFavoritesAsync(999);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion
}
