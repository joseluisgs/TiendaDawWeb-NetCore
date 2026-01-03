using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Data;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Implementations;
using Microsoft.Data.Sqlite;

namespace TiendaDawWeb.Tests.Services;

[TestFixture]
public class RatingServiceTests
{
    private ApplicationDbContext _context = null!;
    private SqliteConnection _connection = null!;
    private RatingService _service = null!;
    private Mock<ILogger<RatingService>> _loggerMock = null!;

    [SetUp]
    public void Setup()
    {
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _loggerMock = new Mock<ILogger<RatingService>>();
        _service = new RatingService(_context, _loggerMock.Object);
        
        // Setup initial data if needed, but we do it in tests for clarity
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Test]
    public async Task AddRatingAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var usuario = new User { Id = 1, Nombre = "Test", Apellidos = "User", UserName = "testuser", Email = "test@test.com", Rol = "USER" };
        var propietario = new User { Id = 2, Nombre = "Owner", Apellidos = "User", UserName = "owner", Email = "owner@test.com", Rol = "USER" };
        var producto = new Product { Id = 1, Nombre = "Test Product", Descripcion = "Description", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2 };
        
        _context.Users.AddRange(usuario, propietario);
        _context.Products.Add(producto);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AddRatingAsync(1, 1, 5, "Excelente producto");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Puntuacion.Should().Be(5);
    }

    [Test]
    public async Task AddRatingAsync_WithInvalidRating_ReturnsFailure()
    {
        // Act
        var result = await _service.AddRatingAsync(1, 1, 6, "Invalid");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("INVALID_RATING");
    }

    [Test]
    public async Task GetAverageRatingAsync_WithMultipleRatings_ReturnsCorrectAverage()
    {
        // Arrange
        var u1 = new User { Id = 1, Nombre = "U1", Email = "u1@t.com" };
        var u2 = new User { Id = 2, Nombre = "U2", Email = "u2@t.com" };
        _context.Users.AddRange(u1, u2);
        
        var producto = new Product { Id = 1, Nombre = "P1", Descripcion = "D", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1 };
        _context.Products.Add(producto);
        await _context.SaveChangesAsync();

        var ratings = new[]
        {
            new Rating { ProductoId = 1, UsuarioId = 1, Puntuacion = 5, CreatedAt = DateTime.UtcNow },
            new Rating { ProductoId = 1, UsuarioId = 2, Puntuacion = 3, CreatedAt = DateTime.UtcNow }
        };

        _context.Ratings.AddRange(ratings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAverageRatingAsync(1);

        // Assert
        result.Value.Should().Be(4.0);
    }

    [Test]
    public async Task DeleteRatingAsync_AsOwner_ReturnsSuccess()
    {
        // Arrange
        var usuario = new User { Id = 1, Nombre = "U1", Apellidos = "A", UserName = "u1", Email = "u1@t.com", Rol = "USER" };
        _context.Users.Add(usuario);
        var producto = new Product { Id = 1, Nombre = "P1", PropietarioId = 1 };
        _context.Products.Add(producto);
        var rating = new Rating { Id = 1, ProductoId = 1, UsuarioId = 1, Puntuacion = 5 };
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteRatingAsync(1, 1, false);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task GetByProductoIdAsync_ShouldReturnRatings()
    {
        // Arrange
        var user = new User { Id = 1, Nombre = "U1", Email = "u1@t.com" };
        _context.Users.Add(user);
        var p = new Product { Id = 10, Nombre = "P10", PropietarioId = 1 };
        _context.Products.Add(p);
        await _context.SaveChangesAsync();

        var r1 = new Rating { Id = 1, ProductoId = 10, UsuarioId = 1, Puntuacion = 5 };
        _context.Ratings.Add(r1);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByProductoIdAsync(10);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
    }

    [Test]
    public async Task UpdateRatingAsync_ShouldSucceed_WhenOwner()
    {
        // Arrange
        var user = new User { Id = 1, Nombre = "U1", Email = "u1@t.com" };
        _context.Users.Add(user);
        var p = new Product { Id = 10, Nombre = "P10", PropietarioId = 1 };
        _context.Products.Add(p);
        await _context.SaveChangesAsync();

        var r1 = new Rating { Id = 1, ProductoId = 10, UsuarioId = 1, Puntuacion = 3 };
        _context.Ratings.Add(r1);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.UpdateRatingAsync(1, 1, 5, "Better");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Puntuacion.Should().Be(5);
    }

    [Test]
    public async Task CanUserRateProductAsync_ShouldReturnTrue_WhenPurchased()
    {
        // Arrange
        var user = new User { Id = 1, Nombre = "U1", Email = "u1@t.com" };
        var seller = new User { Id = 2, Nombre = "U2", Email = "u2@t.com" };
        _context.Users.AddRange(user, seller);
        
        var product = new Product { Id = 1, Nombre = "P1", PropietarioId = 2 };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var purchase = new Purchase { Id = 1, CompradorId = 1 };
        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        product.CompraId = 1;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CanUserRateProductAsync(1, 1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }
}