using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Rating;
using RatingModel = TiendaDawWeb.Shared.Models.Rating;
using PurchaseModel = TiendaDawWeb.Shared.Models.Purchase;

namespace TiendaDawWeb.Tests.Shared.Services;

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

    #region AddRatingAsync Tests

    [Test]
    public async Task AddRatingAsync_AddsRating_Success()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddRatingAsync(1, 1, 5, "Great product!");

        result.IsSuccess.Should().BeTrue();
        result.Value.Puntuacion.Should().Be(5);
        result.Value.Comentario.Should().Be("Great product!");
        _context.Ratings.Should().HaveCount(1);
    }

    [Test]
    public async Task AddRatingAsync_ReturnsFailure_InvalidRating()
    {
        var result = await _service.AddRatingAsync(1, 1, 6, "Invalid rating");

        result.IsSuccess.Should().BeFalse();
        _context.Ratings.Should().BeEmpty();
    }

    [Test]
    public async Task AddRatingAsync_ReturnsFailure_ProductNotFound()
    {
        var result = await _service.AddRatingAsync(1, 999, 5, "Product not found");

        result.IsSuccess.Should().BeFalse();
        _context.Ratings.Should().BeEmpty();
    }

    [Test]
    public async Task AddRatingAsync_ReturnsFailure_AlreadyRated()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var existingRating = new RatingModel { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 4 };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.Ratings.Add(existingRating);
        await _context.SaveChangesAsync();

        var result = await _service.AddRatingAsync(1, 1, 5, "Second rating");

        result.IsSuccess.Should().BeFalse();
        _context.Ratings.Should().HaveCount(1);
    }

    [Test]
    public async Task AddRatingAsync_AllowsZeroComment()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.AddRatingAsync(1, 1, 3, null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Comentario.Should().BeNull();
    }

    [Test]
    public async Task AddRatingAsync_RatingBoundaries()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var resultMin = await _service.AddRatingAsync(1, 1, 1, "Min rating");

        resultMin.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region GetByIdAsync Tests

    [Test]
    public async Task GetByIdAsync_ReturnsRating_WhenExists()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var rating = new RatingModel { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 5, Comentario = "Great!" };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        var result = await _service.GetByIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Puntuacion.Should().Be(5);
    }

    [Test]
    public async Task GetByIdAsync_ReturnsFailure_WhenNotExists()
    {
        var result = await _service.GetByIdAsync(999);

        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region GetByProductoIdAsync Tests

    [Test]
    public async Task GetByProductoIdAsync_ReturnsProductRatings()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.Ratings.AddRange(
            new RatingModel { Id = 1, ProductoId = 1, UsuarioId = 1, Puntuacion = 4 },
            new RatingModel { Id = 2, ProductoId = 1, UsuarioId = 1, Puntuacion = 5 }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetByProductoIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
    }

    [Test]
    public async Task GetByProductoIdAsync_ReturnsEmpty_WhenNoRatings()
    {
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.GetByProductoIdAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Test]
    public async Task GetByProductoIdAsync_ReturnsEmpty_WhenProductNotExists()
    {
        var result = await _service.GetByProductoIdAsync(999);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    #endregion

    #region UpdateRatingAsync Tests

    [Test]
    public async Task UpdateRatingAsync_UpdatesRating_Success()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var rating = new RatingModel { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 3, Comentario = "Average" };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        var result = await _service.UpdateRatingAsync(1, 1, 5, "Updated comment");

        result.IsSuccess.Should().BeTrue();
        result.Value.Puntuacion.Should().Be(5);
        result.Value.Comentario.Should().Be("Updated comment");
    }

    [Test]
    public async Task UpdateRatingAsync_ReturnsFailure_Unauthorized()
    {
        var user1 = new User { Id = 1, Email = "test1@test.com", UserName = "test1" };
        var user2 = new User { Id = 2, Email = "test2@test.com", UserName = "test2" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var rating = new RatingModel { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 3 };
        _context.Users.AddRange(user1, user2);
        _context.Products.Add(product);
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        var result = await _service.UpdateRatingAsync(1, 2, 5, "Hacked update");

        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task UpdateRatingAsync_ReturnsFailure_NotExists()
    {
        var result = await _service.UpdateRatingAsync(999, 1, 5, "Ghost rating");

        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region DeleteRatingAsync Tests

    [Test]
    public async Task DeleteRatingAsync_DeletesRating_Success()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var rating = new RatingModel { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 5 };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteRatingAsync(1, 1, false);

        result.IsSuccess.Should().BeTrue();
        _context.Ratings.Should().BeEmpty();
    }

    [Test]
    public async Task DeleteRatingAsync_AdminCanDeleteAny_Success()
    {
        var user1 = new User { Id = 1, Email = "test1@test.com", UserName = "test1" };
        var user2 = new User { Id = 2, Email = "test2@test.com", UserName = "test2" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var rating = new RatingModel { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 5 };
        _context.Users.AddRange(user1, user2);
        _context.Products.Add(product);
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteRatingAsync(1, 2, true);

        result.IsSuccess.Should().BeTrue();
        _context.Ratings.Should().BeEmpty();
    }

    [Test]
    public async Task DeleteRatingAsync_ReturnsFailure_Unauthorized()
    {
        var user1 = new User { Id = 1, Email = "test1@test.com", UserName = "test1" };
        var user2 = new User { Id = 2, Email = "test2@test.com", UserName = "test2" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        var rating = new RatingModel { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 5 };
        _context.Users.AddRange(user1, user2);
        _context.Products.Add(product);
        _context.Ratings.Add(rating);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteRatingAsync(1, 2, false);

        result.IsSuccess.Should().BeFalse();
    }

    [Test]
    public async Task DeleteRatingAsync_ReturnsFailure_NotExists()
    {
        var result = await _service.DeleteRatingAsync(999, 1, false);

        result.IsSuccess.Should().BeFalse();
    }

    #endregion

    #region GetAverageRatingAsync Tests

    [Test]
    public async Task GetAverageRatingAsync_ReturnsAverage_Success()
    {
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Products.Add(product);
        _context.Ratings.AddRange(
            new RatingModel { Id = 1, ProductoId = 1, UsuarioId = 1, Puntuacion = 4 },
            new RatingModel { Id = 2, ProductoId = 1, UsuarioId = 2, Puntuacion = 5 },
            new RatingModel { Id = 3, ProductoId = 1, UsuarioId = 3, Puntuacion = 3 }
        );
        await _context.SaveChangesAsync();

        var result = await _service.GetAverageRatingAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeApproximately(4.0, 0.01);
    }

    [Test]
    public async Task GetAverageRatingAsync_ReturnsZero_WhenNoRatings()
    {
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.GetAverageRatingAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(0.0);
    }

    [Test]
    public async Task GetAverageRatingAsync_SingleRating_ReturnsValue()
    {
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Products.Add(product);
        _context.Ratings.Add(new RatingModel { Id = 1, ProductoId = 1, UsuarioId = 1, Puntuacion = 5 });
        await _context.SaveChangesAsync();

        var result = await _service.GetAverageRatingAsync(1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(5.0);
    }

    #endregion

    #region CanUserRateProductAsync Tests

    [Test]
    public async Task CanUserRateProductAsync_ReturnsTrue_WhenCanRate()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var owner = new User { Id = 2, Email = "owner@test.com", UserName = "owner" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        _context.Users.AddRange(user, owner);
        _context.Products.Add(product);
        var purchase = new PurchaseModel { Id = 1, CompradorId = 1, Total = 100, FechaCompra = DateTime.UtcNow };
        purchase.Products.Add(product);
        _context.Purchases.Add(purchase);
        await _context.SaveChangesAsync();

        var result = await _service.CanUserRateProductAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Test]
    public async Task CanUserRateProductAsync_ReturnsFalse_WhenAlreadyRated()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 2, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        _context.Ratings.Add(new RatingModel { Id = 1, UsuarioId = 1, ProductoId = 1, Puntuacion = 4 });
        await _context.SaveChangesAsync();

        var result = await _service.CanUserRateProductAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    [Test]
    public async Task CanUserRateProductAsync_ReturnsFalse_WhenOwner()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test" };
        var product = new Product { Id = 1, Nombre = "Test", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        _context.Users.Add(user);
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        var result = await _service.CanUserRateProductAsync(1, 1);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeFalse();
    }

    #endregion
}
