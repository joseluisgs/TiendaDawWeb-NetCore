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
/// <summary>
/// OBJETIVO: Validar las reglas de negocio para la gestión de valoraciones (Ratings).
/// LO QUE BUSCA: Asegurar que los usuarios pueden valorar productos, calcular promedios
/// y que se respetan los permisos de edición y borrado.
/// </summary>
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

    /// <summary>
    /// PRUEBA: Creación de valoración válida.
    /// OBJETIVO: Confirmar que un usuario puede valorar satisfactoriamente un producto.
    /// </summary>
    [Test]
    public async Task AddRatingAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var usuario = new User { Id = 1, Nombre = "Test", Apellidos = "User", UserName = "testuser", Email = "test@test.com", Rol = "USER" };
        var propietario = new User { Id = 2, Nombre = "Owner", Apellidos = "User", UserName = "owner", Email = "owner@test.com", Rol = "USER" };
        var producto = new Product { Id = 1, Nombre = "Test Product", Descripcion = "Description", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2 };
        var compra = new Purchase { Id = 1, CompradorId = 1, FechaCompra = DateTime.UtcNow, Total = 100 };

        _context.Users.AddRange(usuario, propietario);
        _context.Products.Add(producto);
        _context.Purchases.Add(compra);
        await _context.SaveChangesAsync();

        producto.CompraId = 1;
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AddRatingAsync(1, 1, 5, "Excelente producto");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Puntuacion.Should().Be(5);
    }

    /// <summary>
    /// PRUEBA: Valoración sin compra previa.
    /// OBJETIVO: Verificar que (según la lógica actual) se permite valorar aunque no haya registro de compra.
    /// </summary>
    [Test]
    public async Task AddRatingAsync_WithoutPurchase_ReturnsSuccess()
    {
        // Arrange
        var usuario = new User { Id = 1, Nombre = "Test", Apellidos = "User", UserName = "testuser", Email = "test@test.com", Rol = "USER" };
        var producto = new Product { Id = 1, Nombre = "Test Product", Descripcion = "Description", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2 };

        _context.Users.Add(usuario);
        _context.Products.Add(producto);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.AddRatingAsync(1, 1, 5, "Excelente");

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    /// <summary>
    /// PRUEBA: Validación de rango de puntuación.
    /// OBJETIVO: Asegurar que el sistema rechaza puntuaciones fuera del rango 1-5 (ej. 6).
    /// </summary>
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

    /// <summary>
    /// PRUEBA: Cálculo de promedio.
    /// OBJETIVO: Validar que el promedio matemático de varias notas es correcto.
    /// </summary>
    [Test]
    public async Task GetAverageRatingAsync_WithMultipleRatings_ReturnsCorrectAverage()
    {
        // Arrange
        var producto = new Product { Id = 1, Nombre = "P1", Descripcion = "D", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1 };
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
        result.Value.Should().Be(4.0); // (5+4+3)/3 = 4
    }

    /// <summary>
    /// PRUEBA: Eliminación por propietario.
    /// OBJETIVO: Confirmar que un usuario puede borrar su propia valoración.
    /// </summary>
    [Test]
    public async Task DeleteRatingAsync_AsOwner_ReturnsSuccess()
    {
        // Arrange
        var usuario = new User { Id = 1, Nombre = "U1", Apellidos = "A", UserName = "u1", Email = "u1@t.com", Rol = "USER" };
        var producto = new Product { Id = 1, Nombre = "P1", Descripcion = "D", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1 };
        var rating = new Rating { Id = 1, ProductoId = 1, UsuarioId = 1, Puntuacion = 5, CreatedAt = DateTime.UtcNow };

        _context.Users.Add(usuario);
        _context.Products.Add(producto);
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteRatingAsync(1, 1, false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        (await _context.Ratings.CountAsync()).Should().Be(0);
    }

    /// <summary>
    /// PRUEBA: Protección de eliminación.
    /// OBJETIVO: Verificar que un usuario NO puede borrar la valoración de otra persona.
    /// </summary>
    [Test]
    public async Task DeleteRatingAsync_AsNonOwner_ReturnsFailure()
    {
        // Arrange
        var u1 = new User { Id = 1, Nombre = "U1", UserName = "u1", Email = "u1@t.com", Rol = "USER" };
        var u2 = new User { Id = 2, Nombre = "U2", UserName = "u2", Email = "u2@t.com", Rol = "USER" };
        var producto = new Product { Id = 1, Nombre = "P1", PropietarioId = 1 };
        var rating = new Rating { Id = 1, ProductoId = 1, UsuarioId = 1, Puntuacion = 5, CreatedAt = DateTime.UtcNow };

        _context.Users.AddRange(u1, u2);
        _context.Products.Add(producto);
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        // Act - El usuario 2 intenta borrar la nota del usuario 1
        var result = await _service.DeleteRatingAsync(1, 2, false);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("UNAUTHORIZED");
    }
}
