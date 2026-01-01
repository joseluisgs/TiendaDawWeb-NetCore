using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Data;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using TiendaDawWeb.Services.Implementations;

namespace TiendaDawWeb.Tests.Services;

[TestFixture]
public class RatingServiceTests
{
    private ApplicationDbContext _context = null!;
    private RatingService _service = null!;
    private Mock<ILogger<RatingService>> _loggerMock = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<RatingService>>();
        _service = new RatingService(_context, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task AddRatingAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var usuario = new User
        {
            Id = 1,
            Nombre = "Test",
            Apellidos = "User",
            UserName = "testuser",
            Email = "test@test.com",
            Rol = "USER"
        };
        
        var propietario = new User
        {
            Id = 2,
            Nombre = "Owner",
            Apellidos = "User",
            UserName = "owner",
            Email = "owner@test.com",
            Rol = "USER"
        };

        var producto = new Product
        {
            Id = 1,
            Nombre = "Test Product",
            Descripcion = "Description",
            Precio = 100,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = 2
        };

        var compra = new Purchase
        {
            Id = 1,
            CompradorId = 1,
            FechaCompra = DateTime.UtcNow,
            Total = 100
        };

        _context.Users.AddRange(usuario, propietario);
        _context.Products.Add(producto);
        _context.Purchases.Add(compra);
        await _context.SaveChangesAsync();

        // Simular que el producto fue comprado
        producto.CompraId = 1;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AddRatingAsync(1, 1, 5, "Excelente producto");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Puntuacion.Should().Be(5);
        result.Value.Comentario.Should().Be("Excelente producto");
    }

    [Test]
    public async Task AddRatingAsync_WithoutPurchase_ReturnsFailure()
    {
        // Arrange
        var usuario = new User
        {
            Id = 1,
            Nombre = "Test",
            Apellidos = "User",
            UserName = "testuser",
            Email = "test@test.com",
            Rol = "USER"
        };

        var producto = new Product
        {
            Id = 1,
            Nombre = "Test Product",
            Descripcion = "Description",
            Precio = 100,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = 2
        };

        _context.Users.Add(usuario);
        _context.Products.Add(producto);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AddRatingAsync(1, 1, 5, "Excelente");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("PRODUCT_NOT_PURCHASED");
    }

    [Test]
    public async Task AddRatingAsync_WithInvalidRating_ReturnsFailure()
    {
        // Arrange
        var usuario = new User { Id = 1, Nombre = "Test", Apellidos = "User", UserName = "test", Email = "test@test.com", Rol = "USER" };
        var producto = new Product { Id = 1, Nombre = "Test", Descripcion = "Test", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2 };

        _context.Users.Add(usuario);
        _context.Products.Add(producto);
        await _context.SaveChangesAsync();

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
        var producto = new Product
        {
            Id = 1,
            Nombre = "Test Product",
            Descripcion = "Description",
            Precio = 100,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = 1
        };

        _context.Products.Add(producto);
        await _context.SaveChangesAsync();

        var ratings = new[]
        {
            new Rating { ProductoId = 1, UsuarioId = 1, Puntuacion = 5, CreatedAt = DateTime.UtcNow },
            new Rating { ProductoId = 1, UsuarioId = 2, Puntuacion = 4, CreatedAt = DateTime.UtcNow },
            new Rating { ProductoId = 1, UsuarioId = 3, Puntuacion = 3, CreatedAt = DateTime.UtcNow }
        };

        _context.Ratings.AddRange(ratings);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAverageRatingAsync(1);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(4.0);
    }

    [Test]
    public async Task DeleteRatingAsync_AsOwner_ReturnsSuccess()
    {
        // Arrange
        var rating = new Rating
        {
            Id = 1,
            ProductoId = 1,
            UsuarioId = 1,
            Puntuacion = 5,
            Comentario = "Test",
            CreatedAt = DateTime.UtcNow
        };

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteRatingAsync(1, 1, false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var deletedRating = await _context.Ratings.FindAsync(1L);
        deletedRating.Should().BeNull();
    }

    [Test]
    public async Task DeleteRatingAsync_AsNonOwner_ReturnsFailure()
    {
        // Arrange
        var rating = new Rating
        {
            Id = 1,
            ProductoId = 1,
            UsuarioId = 1,
            Puntuacion = 5,
            Comentario = "Test",
            CreatedAt = DateTime.UtcNow
        };

        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteRatingAsync(1, 2, false); // Different user

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UNAUTHORIZED");
    }
}
