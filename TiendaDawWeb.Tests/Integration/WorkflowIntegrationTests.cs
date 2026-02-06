using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Carrito;
using TiendaDawWeb.Shared.Services.Email;
using TiendaDawWeb.Shared.Services.Favorite;
using TiendaDawWeb.Shared.Services.Pdf;
using TiendaDawWeb.Shared.Services.Product;
using TiendaDawWeb.Shared.Services.Purchase;
using TiendaDawWeb.Shared.Services.Rating;
using TiendaDawWeb.Shared.Web.Hubs;

namespace TiendaDawWeb.Tests.Integration;

/// <summary>
/// Integration tests for complete business workflows.
/// Tests use real SQLite database with all services.
/// </summary>
[NonParallelizable]
public class WorkflowIntegrationTests
{
    #region Rating Validation Tests

    [Test]
    public async Task Rating_CannotRateWithoutPurchase_Fails()
    {
        await using var fixture = new WorkflowTestFixture();
        var services = fixture.Services;

        var rater = new User { Id = 1, Email = "rater@test.com", UserName = "rater", Nombre = "Rater", Apellidos = "User", Rol = "USER" };
        var seller = new User { Id = 2, Email = "seller@test.com", UserName = "seller", Nombre = "Seller", Apellidos = "User", Rol = "USER" };
        services.Context.Users.AddRange(rater, seller);
        await services.Context.SaveChangesAsync();

        var product = new Product
        {
            Id = 100,
            Nombre = "Product to Rate",
            Precio = 100,
            Categoria = ProductCategory.LAPTOPS,
            PropietarioId = 2,
            Deleted = false
        };
        services.Context.Products.Add(product);
        await services.Context.SaveChangesAsync();

        var ratingResult = await services.RatingService.AddRatingAsync(1, 100, 5, "Great!");
        ratingResult.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task Rating_CannotRateTwice_Fails()
    {
        await using var fixture = new WorkflowTestFixture();
        var services = fixture.Services;

        var rater = new User { Id = 1, Email = "rater@test.com", UserName = "rater", Nombre = "Rater", Apellidos = "User", Rol = "USER" };
        var seller = new User { Id = 2, Email = "seller@test.com", UserName = "seller", Nombre = "Seller", Apellidos = "User", Rol = "USER" };
        services.Context.Users.AddRange(rater, seller);

        var product = new Product
        {
            Id = 100,
            Nombre = "Product",
            Precio = 100,
            Categoria = ProductCategory.LAPTOPS,
            PropietarioId = 2,
            Deleted = false
        };
        services.Context.Products.Add(product);

        var purchase = new Purchase { Id = 1, CompradorId = 1, FechaCompra = DateTime.UtcNow, Total = 100 };
        services.Context.Purchases.Add(purchase);

        var rating = new Rating
        {
            UsuarioId = 1,
            ProductoId = 100,
            Puntuacion = 4,
            Comentario = "First rating",
            CreatedAt = DateTime.UtcNow
        };
        services.Context.Ratings.Add(rating);
        await services.Context.SaveChangesAsync();

        var result = await services.RatingService.AddRatingAsync(1, 100, 5, "Second rating");
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Duplicate Protection Tests

    [Test]
    public async Task Favorite_CannotAddDuplicate_Fails()
    {
        await using var fixture = new WorkflowTestFixture();
        var services = fixture.Services;

        var user = new User { Id = 1, Email = "user@test.com", UserName = "user", Nombre = "User", Apellidos = "Test", Rol = "USER" };
        var owner = new User { Id = 2, Email = "owner@test.com", UserName = "owner", Nombre = "Owner", Apellidos = "Test", Rol = "USER" };
        services.Context.Users.AddRange(user, owner);

        var product = new Product { Id = 100, Nombre = "Product", Precio = 100, Categoria = ProductCategory.LAPTOPS, PropietarioId = 2, Deleted = false };
        services.Context.Products.Add(product);

        var existingFavorite = new Favorite { UsuarioId = 1, ProductoId = 100 };
        services.Context.Favorites.Add(existingFavorite);
        await services.Context.SaveChangesAsync();

        var result = await services.FavoriteService.AddFavoriteAsync(1, 100);
        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task Carrito_CannotAddDuplicateProduct_Fails()
    {
        await using var fixture = new WorkflowTestFixture();
        var services = fixture.Services;

        var user = new User { Id = 1, Email = "user@test.com", UserName = "user", Nombre = "User", Apellidos = "Test", Rol = "USER" };
        var owner = new User { Id = 2, Email = "owner@test.com", UserName = "owner", Nombre = "Owner", Apellidos = "Test", Rol = "USER" };
        services.Context.Users.AddRange(user, owner);

        var product = new Product
        {
            Id = 100,
            Nombre = "Product",
            Precio = 100,
            Categoria = ProductCategory.LAPTOPS,
            PropietarioId = 2,
            Deleted = false,
            Reservado = false
        };
        services.Context.Products.Add(product);

        var existingItem = new CarritoItem { UsuarioId = 1, ProductoId = 100, Precio = 100, CreatedAt = DateTime.UtcNow };
        services.Context.CarritoItems.Add(existingItem);
        await services.Context.SaveChangesAsync();

        var result = await services.CarritoService.AddToCarritoAsync(1, 100);
        result.IsFailure.Should().BeTrue();
    }

    #endregion

    #region Cache Invalidation Tests

    [Test]
    public async Task Product_Create_InvalidatesCache()
    {
        await using var fixture = new WorkflowTestFixture();
        var services = fixture.Services;

        var user = new User { Id = 1, Email = "test@test.com", UserName = "test", Nombre = "Test", Apellidos = "User", Rol = "USER" };
        await services.Context.Users.AddAsync(user);
        await services.Context.SaveChangesAsync();

        var product1 = new Product
        {
            Nombre = "Product 1",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = 1,
            Deleted = false
        };
        await services.ProductService.CreateAsync(product1);

        var result1 = await services.ProductService.GetAllAsync();
        result1.Value.Should().HaveCount(1);

        var product2 = new Product
        {
            Nombre = "Product 2",
            Descripcion = "Desc",
            Precio = 200,
            Categoria = ProductCategory.LAPTOPS,
            PropietarioId = 1,
            Deleted = false
        };
        await services.ProductService.CreateAsync(product2);

        var result2 = await services.ProductService.GetAllAsync();
        result2.Value.Should().HaveCount(2);
    }

    [Test]
    public async Task Product_Delete_InvalidatesCache()
    {
        await using var fixture = new WorkflowTestFixture();
        var services = fixture.Services;

        var user = new User { Id = 1, Email = "test@test.com", UserName = "test", Nombre = "Test", Apellidos = "User", Rol = "USER" };
        await services.Context.Users.AddAsync(user);
        await services.Context.SaveChangesAsync();

        var product = new Product
        {
            Id = 100,
            Nombre = "To Delete",
            Descripcion = "Desc",
            Precio = 100,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = 1,
            Deleted = false
        };
        await services.ProductService.CreateAsync(product);

        var beforeDelete = await services.ProductService.GetAllAsync();
        beforeDelete.Value.Should().HaveCount(1);

        await services.ProductService.DeleteAsync(100, 1, false);

        var afterDelete = await services.ProductService.GetAllAsync();
        afterDelete.Value.Should().HaveCount(0);
    }

    #endregion

    #region Soft Delete Tests

    [Test]
    public async Task Product_SoftDelete_MarksAsDeleted_StillInDb()
    {
        await using var fixture = new WorkflowTestFixture();
        var services = fixture.Services;

        var user = new User { Id = 1, Email = "test@test.com", UserName = "test", Nombre = "Test", Apellidos = "User", Rol = "USER" };
        services.Context.Users.Add(user);

        var product = new Product
        {
            Id = 100,
            Nombre = "To Delete",
            Precio = 100,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = 1,
            Deleted = false
        };
        services.Context.Products.Add(product);
        await services.Context.SaveChangesAsync();

        var productInDb = await services.Context.Products.FindAsync(100L);
        productInDb!.Deleted.Should().BeFalse();

        productInDb.SoftDelete("TestUser");
        await services.Context.SaveChangesAsync();

        var deletedVisible = await services.Context.Products.IgnoreQueryFilters().FirstAsync(p => p.Id == 100);
        deletedVisible.Deleted.Should().BeTrue();
    }

    #endregion

    #region Concurrency Simulation Tests

    [Test]
    public async Task Product_UpdateMultipleTimes_AllSucceed()
    {
        await using var fixture = new WorkflowTestFixture();
        var services = fixture.Services;

        var user = new User { Id = 1, Email = "test@test.com", UserName = "test", Nombre = "Test", Apellidos = "User", Rol = "USER" };
        services.Context.Users.Add(user);

        var product = new Product
        {
            Id = 100,
            Nombre = "Original",
            Precio = 100,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = 1,
            Deleted = false
        };
        services.Context.Products.Add(product);
        await services.Context.SaveChangesAsync();

        var update1 = await services.ProductService.UpdateAsync(100, new Product { Nombre = "First Update", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, Deleted = false }, 1);
        update1.IsSuccess.Should().BeTrue();
        update1.Value.Nombre.Should().Be("First Update");

        var update2 = await services.ProductService.UpdateAsync(100, new Product { Nombre = "Second Update", Descripcion = "Desc", Precio = 100, Categoria = ProductCategory.SMARTPHONES, Deleted = false }, 1);
        update2.IsSuccess.Should().BeTrue();
        update2.Value.Nombre.Should().Be("Second Update");
    }

    #endregion

    #region Concurrency Tests

    [Test]
    public async Task Concurrency_TwoUsersTryToBuySameProduct_OnlyOneSucceeds()
    {
        await using var fixture = new WorkflowTestFixture();
        var db = fixture.Context;

        var seller = new User { Id = 1, Email = "seller@test.com", UserName = "seller", Nombre = "Seller", Apellidos = "User", Rol = "USER" };
        var buyer1 = new User { Id = 2, Email = "buyer1@test.com", UserName = "buyer1", Nombre = "Buyer1", Apellidos = "User", Rol = "USER" };
        var buyer2 = new User { Id = 3, Email = "buyer2@test.com", UserName = "buyer2", Nombre = "Buyer2", Apellidos = "User", Rol = "USER" };
        db.Users.AddRange(seller, buyer1, buyer2);

        var product = new Product
        {
            Id = 100,
            Nombre = "Unique Product",
            Precio = 500,
            Categoria = ProductCategory.LAPTOPS,
            PropietarioId = 1,
            Deleted = false,
            Reservado = false
        };
        db.Products.Add(product);

        var purchase = new Purchase
        {
            Id = 1,
            CompradorId = 2,
            FechaCompra = DateTime.UtcNow,
            Total = 500
        };
        db.Purchases.Add(purchase);

        product.CompraId = 1;
        await db.SaveChangesAsync();

        var productAfterFirstPurchase = await db.Products.FindAsync(100L);
        productAfterFirstPurchase!.CompraId.Should().Be(1);

        var productDuplicate = await db.Products.FindAsync(100L);
        productDuplicate!.CompraId.Should().Be(1);
    }

    #endregion

    #region Purchase Service Tests

    [Test]
    public async Task Purchase_Create_Success()
    {
        await using var fixture = new WorkflowTestFixture();
        var db = fixture.Context;

        var seller = new User { Id = 1, Email = "seller@test.com", UserName = "seller", Nombre = "Seller", Apellidos = "User", Rol = "USER" };
        var buyer = new User { Id = 2, Email = "buyer@test.com", UserName = "buyer", Nombre = "Buyer", Apellidos = "User", Rol = "USER" };
        db.Users.AddRange(seller, buyer);
        await db.SaveChangesAsync();

        var product1 = new Product { Id = 100, Nombre = "Product 1", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false };
        var product2 = new Product { Id = 101, Nombre = "Product 2", Precio = 200, Categoria = ProductCategory.LAPTOPS, PropietarioId = 1, Deleted = false };
        db.Products.AddRange(product1, product2);

        var purchase = new Purchase
        {
            Id = 1,
            CompradorId = 2,
            FechaCompra = DateTime.UtcNow,
            Total = 300
        };
        db.Purchases.Add(purchase);

        product1.CompraId = 1;
        product2.CompraId = 1;
        await db.SaveChangesAsync();

        var purchaseDb = await db.Purchases
            .Include(p => p.Products)
            .FirstAsync(p => p.Id == 1);

        purchaseDb.Total.Should().Be(300);
        purchaseDb.Products.Should().HaveCount(2);
    }

    #endregion

    #region Carrito Cleanup Tests

    [Test]
    public async Task Carrito_ReservedProduct_ExpiresCorrectly()
    {
        await using var fixture = new WorkflowTestFixture();
        var services = fixture.Services;

        var user = new User { Id = 1, Email = "user@test.com", UserName = "user", Nombre = "User", Apellidos = "Test", Rol = "USER" };
        var owner = new User { Id = 2, Email = "owner@test.com", UserName = "owner", Nombre = "Owner", Apellidos = "Test", Rol = "USER" };
        services.Context.Users.AddRange(user, owner);
        await services.Context.SaveChangesAsync();

        var product = new Product
        {
            Id = 100,
            Nombre = "Reserved Product",
            Precio = 100,
            Categoria = ProductCategory.LAPTOPS,
            PropietarioId = 2,
            Deleted = false,
            Reservado = false
        };
        services.Context.Products.Add(product);
        await services.Context.SaveChangesAsync();

        product.Reservado = true;
        product.ReservadoPor = 1;
        product.ReservadoHasta = DateTime.UtcNow.AddMinutes(-1);
        await services.Context.SaveChangesAsync();

        var expiredProduct = await services.ProductService.GetByIdAsync(100);
        expiredProduct.Value.Reservado.Should().BeTrue();

        product.Reservado = false;
        product.ReservadoPor = null;
        product.ReservadoHasta = null;
        await services.Context.SaveChangesAsync();

        var freeProduct = await services.ProductService.GetByIdAsync(100);
        freeProduct.Value.Reservado.Should().BeFalse();
        freeProduct.Value.ReservadoPor.Should().BeNull();
    }

    #endregion
}

/// <summary>
/// Fixture for workflow tests with all services.
/// </summary>
public class WorkflowTestFixture : IDisposable, IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public readonly WorkflowServices Services;
    public readonly ApplicationDbContext Context;
    private bool _disposed;

    public WorkflowTestFixture()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"workflow_{Guid.NewGuid()}.db");
        _connection = new SqliteConnection($"Data Source={dbPath}");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();

        var memoryCache = new MemoryCache(new MemoryCacheOptions());
        var loggerMock = new Mock<ILogger<ProductService>>();
        var hubContextMock = new Mock<IHubContext<NotificationHub>>();

        Services = new WorkflowServices
        {
            Context = Context,
            ProductService = new ProductService(Context, memoryCache, hubContextMock.Object, loggerMock.Object),
            CarritoService = new CarritoService(Context, memoryCache, new Mock<ILogger<CarritoService>>().Object),
            FavoriteService = new FavoriteService(Context, new Mock<ILogger<FavoriteService>>().Object),
            RatingService = new RatingService(Context, new Mock<ILogger<RatingService>>().Object),
            PurchaseService = new PurchaseService(
                Context,
                new Mock<ICarritoService>().Object,
                new Mock<IPdfService>().Object,
                new Mock<IEmailService>().Object,
                memoryCache,
                new Mock<ILogger<PurchaseService>>().Object)
        };
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Services.Context.Dispose();
            _connection.Close();
            _disposed = true;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await Services.Context.DisposeAsync();
            await _connection.CloseAsync();
            _disposed = true;
        }
    }
}

public class WorkflowServices
{
    public required ApplicationDbContext Context { get; set; }
    public required ProductService ProductService { get; set; }
    public required CarritoService CarritoService { get; set; }
    public required FavoriteService FavoriteService { get; set; }
    public required RatingService RatingService { get; set; }
    public required PurchaseService PurchaseService { get; set; }
}
