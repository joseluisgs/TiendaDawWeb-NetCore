using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Implementations;
using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using FluentAssertions;

namespace TiendaDawWeb.Tests.Services;

/// <summary>
/// Pruebas unitarias para FavoriteService
/// </summary>
[TestFixture]
public class FavoriteServiceTests
{
    private ApplicationDbContext _context = null!;
    private Mock<ILogger<FavoriteService>> _loggerMock = null!;
    private FavoriteService _favoriteService = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        // Seed data
        var user = new User
        {
            Id = 1,
            Nombre = "User",
            Apellidos = "Test",
            Email = "user@test.com",
            UserName = "user@test.com",
            Rol = "USER"
        };
        _context.Users.Add(user);

        var product = new Product
        {
            Id = 1,
            Nombre = "Producto Test",
            Descripcion = "Desc",
            Precio = 10.0m,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = 1,
            Propietario = user,
            CreatedAt = DateTime.UtcNow
        };
        _context.Products.Add(product);
        _context.SaveChanges();

        _loggerMock = new Mock<ILogger<FavoriteService>>();
        _favoriteService = new FavoriteService(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddFavoriteAsync_ShouldAddFavorite_WhenNotExists()
    {
        var userId = 1L;
        var productId = 1L;

        var result = await _favoriteService.AddFavoriteAsync(userId, productId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UsuarioId.Should().Be(userId);
        result.Value.ProductoId.Should().Be(productId);

        var inDb = await _context.Favorites.FirstOrDefaultAsync(f => f.UsuarioId == userId && f.ProductoId == productId);
        inDb.Should().NotBeNull();
    }

    [Test]
    public async Task AddFavoriteAsync_ShouldReturnError_WhenAlreadyExists()
    {
        var userId = 1L;
        var productId = 1L;
        await _favoriteService.AddFavoriteAsync(userId, productId);

        var result = await _favoriteService.AddFavoriteAsync(userId, productId);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FavoriteError.AlreadyExists);
    }

    [Test]
    public async Task IsFavoriteAsync_ShouldReturnTrue_WhenExists()
    {
        var userId = 1L;
        var productId = 1L;
        await _favoriteService.AddFavoriteAsync(userId, productId);

        var result = await _favoriteService.IsFavoriteAsync(userId, productId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Test]
    public async Task IsFavoriteAsync_ShouldReturnFalse_WhenNotExists()
    {
        var result = await _favoriteService.IsFavoriteAsync(1L, 1L);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Test]
    public async Task RemoveFavoriteAsync_ShouldRemove_WhenExists()
    {
        var userId = 1L;
        var productId = 1L;
        await _favoriteService.AddFavoriteAsync(userId, productId);

        var result = await _favoriteService.RemoveFavoriteAsync(userId, productId);

        result.IsSuccess.Should().BeTrue();
        
        var inDb = await _context.Favorites.AnyAsync(f => f.UsuarioId == userId && f.ProductoId == productId);
        inDb.Should().BeFalse();
    }

    [Test]
    public async Task RemoveFavoriteAsync_ShouldReturnError_WhenNotExists()
    {
        var result = await _favoriteService.RemoveFavoriteAsync(1L, 1L);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(FavoriteError.NotFound);
    }

    [Test]
    public async Task GetUserFavoritesAsync_ShouldReturnList()
    {
        var userId = 1L;
        var productId = 1L;
        await _favoriteService.AddFavoriteAsync(userId, productId);

        var result = await _favoriteService.GetUserFavoritesAsync(userId);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value.First().Id.Should().Be(productId);
    }
}
