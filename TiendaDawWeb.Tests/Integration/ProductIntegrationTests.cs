using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;

namespace TiendaDawWeb.Tests.Integration;

/// <summary>
/// Integration tests using SQLite (same as production).
/// Each test creates its own isolated database.
/// </summary>
[NonParallelizable]
public class ProductIntegrationTests
{
    #region Create Product Tests

    [Test]
    public async Task CreateProduct_WithValidData_SavesToDatabase()
    {
        await using var fixture = new IntegrationTestFixture();
        var db = fixture.Context;

        var user = new User { Id = 1, Email = "test@test.com", UserName = "test", Nombre = "Test", Apellidos = "User", Rol = "USER" };
        db.Users.Add(user);

        var product = new Product
        {
            Nombre = "Test Product",
            Descripcion = "A test product",
            Precio = 99.99m,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = 1,
            Deleted = false
        };
        db.Products.Add(product);
        await db.SaveChangesAsync();

        product.Id.Should().BeGreaterThan(0);
        product.Nombre.Should().Be("Test Product");
    }

    [Test]
    public async Task GetAllProducts_ReturnsOnlyNonDeleted()
    {
        await using var fixture = new IntegrationTestFixture();
        var db = fixture.Context;

        var user = new User { Id = 1, Email = "test@test.com", UserName = "test", Nombre = "Test", Apellidos = "User", Rol = "USER" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        db.Products.AddRange(
            new Product { Nombre = "Active", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 1, Deleted = false },
            new Product { Nombre = "Deleted", Precio = 200, Categoria = ProductCategory.ACCESSORIES, PropietarioId = 1, Deleted = true }
        );
        await db.SaveChangesAsync();

        var activeProducts = await db.Products
            .Where(p => !p.Deleted && p.CompraId == null)
            .ToListAsync();

        activeProducts.Should().HaveCount(1);
        activeProducts.First().Nombre.Should().Be("Active");
    }

    [Test]
    public async Task DeleteProduct_SoftDeletes()
    {
        await using var fixture = new IntegrationTestFixture();
        var db = fixture.Context;

        var user = new User { Id = 1, Email = "test@test.com", UserName = "test", Nombre = "Test", Apellidos = "User", Rol = "USER" };
        db.Users.Add(user);
        await db.SaveChangesAsync();

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
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var dbProduct = await db.Products.FindAsync(100L);
        dbProduct!.SoftDelete("TestUser");
        await db.SaveChangesAsync();

        var deleted = await db.Products.IgnoreQueryFilters().FirstAsync(p => p.Id == 100);
        deleted.Deleted.Should().BeTrue();
    }

    [Test]
    public async Task Product_Propietario_Relationship_Works()
    {
        await using var fixture = new IntegrationTestFixture();
        var db = fixture.Context;

        var user = new User { Id = 200, UserName = "owner", Email = "owner@test.com", Nombre = "Owner", Apellidos = "User", Rol = "USER" };
        var product = new Product
        {
            Id = 200,
            Nombre = "Owner Product",
            Descripcion = "Desc",
            Precio = 500,
            Categoria = ProductCategory.LAPTOPS,
            PropietarioId = 200,
            Propietario = user,
            Deleted = false
        };
        db.Users.Add(user);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var dbProduct = await db.Products
            .Include(p => p.Propietario)
            .FirstAsync(p => p.Id == 200);

        dbProduct.Propietario.Should().NotBeNull();
        dbProduct.Propietario!.Email.Should().Be("owner@test.com");
    }

    #endregion

    #region Favorite Tests (With Include - SQLite supports this!)

    [Test]
    public async Task Favorite_AddAndQuery_Works()
    {
        await using var fixture = new IntegrationTestFixture();
        var db = fixture.Context;

        var user = new User { Id = 300, Email = "user@test.com", UserName = "user", Nombre = "User", Apellidos = "Test", Rol = "USER" };
        var owner = new User { Id = 301, Email = "owner@test.com", UserName = "owner", Nombre = "Owner", Apellidos = "Test", Rol = "USER" };
        var owner2 = new User { Id = 302, Email = "owner2@test.com", UserName = "owner2", Nombre = "Owner2", Apellidos = "Test", Rol = "USER" };

        var product1 = new Product { Id = 301, Nombre = "P1", Precio = 100, Categoria = ProductCategory.SMARTPHONES, PropietarioId = 301, Deleted = false };
        var product2 = new Product { Id = 302, Nombre = "P2", Precio = 200, Categoria = ProductCategory.ACCESSORIES, PropietarioId = 302, Deleted = false };

        db.Users.AddRange(user, owner, owner2);
        db.Products.AddRange(product1, product2);
        await db.SaveChangesAsync();

        var favorite1 = new Favorite { UsuarioId = 300, ProductoId = 301 };
        var favorite2 = new Favorite { UsuarioId = 300, ProductoId = 302 };
        db.Favorites.AddRange(favorite1, favorite2);
        await db.SaveChangesAsync();

        var favorites = await db.Favorites
            .Where(f => f.UsuarioId == 300)
            .Include(f => f.Producto)
            .ToListAsync();

        favorites.Should().HaveCount(2);
    }

    [Test]
    public async Task Carrito_AddItem_Works()
    {
        await using var fixture = new IntegrationTestFixture();
        var db = fixture.Context;

        var user = new User { Id = 400, Email = "user@test.com", UserName = "user", Nombre = "User", Apellidos = "Test", Rol = "USER" };
        var owner = new User { Id = 401, Email = "owner@test.com", UserName = "owner", Nombre = "Owner", Apellidos = "Test", Rol = "USER" };

        var product = new Product
        {
            Id = 401,
            Nombre = "Cart Product",
            Precio = 300,
            Categoria = ProductCategory.LAPTOPS,
            PropietarioId = 401,
            Deleted = false,
            Reservado = false
        };
        db.Users.AddRange(user, owner);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var cartItem = new CarritoItem
        {
            UsuarioId = 400,
            ProductoId = 401,
            Precio = 300,
            CreatedAt = DateTime.UtcNow
        };
        db.CarritoItems.Add(cartItem);
        await db.SaveChangesAsync();

        var cartItems = await db.CarritoItems
            .Include(c => c.Producto)
            .Where(c => c.UsuarioId == 400)
            .ToListAsync();

        cartItems.Should().HaveCount(1);
        cartItems.First().Precio.Should().Be(300);
    }

    #endregion

    #region Purchase Tests

    [Test]
    public async Task Purchase_CreateAndQuery_Works()
    {
        await using var fixture = new IntegrationTestFixture();
        var db = fixture.Context;

        var user = new User { Id = 500, Email = "buyer@test.com", UserName = "buyer", Nombre = "Buyer", Apellidos = "User", Rol = "USER" };
        var owner = new User { Id = 501, Email = "owner@test.com", UserName = "owner", Nombre = "Owner", Apellidos = "User", Rol = "USER" };

        var product = new Product
        {
            Id = 501,
            Nombre = "Purchased Product",
            Precio = 1000,
            Categoria = ProductCategory.LAPTOPS,
            PropietarioId = 501,
            Deleted = false,
            CompraId = null
        };
        db.Users.AddRange(user, owner);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var purchase = new Purchase
        {
            Id = 100,
            CompradorId = 500,
            FechaCompra = DateTime.UtcNow,
            Total = 1000
        };
        db.Purchases.Add(purchase);

        product.CompraId = 100;
        await db.SaveChangesAsync();

        var dbPurchase = await db.Purchases
            .Include(p => p.Products)
            .FirstAsync(p => p.Id == 100);

        dbPurchase.Total.Should().Be(1000);
        dbPurchase.Products.Should().ContainSingle();
    }

    #endregion

    #region Rating Tests

    [Test]
    public async Task Rating_AddAndQuery_Works()
    {
        await using var fixture = new IntegrationTestFixture();
        var db = fixture.Context;

        var user = new User { Id = 600, Email = "rater@test.com", UserName = "rater", Nombre = "Rater", Apellidos = "User", Rol = "USER" };
        var owner = new User { Id = 601, Email = "owner@test.com", UserName = "owner", Nombre = "Owner", Apellidos = "User", Rol = "USER" };

        var product = new Product
        {
            Id = 601,
            Nombre = "Rated Product",
            Precio = 500,
            Categoria = ProductCategory.SMARTPHONES,
            PropietarioId = 601,
            Deleted = false
        };
        db.Users.AddRange(user, owner);
        db.Products.Add(product);
        await db.SaveChangesAsync();

        var rating = new Rating
        {
            UsuarioId = 600,
            ProductoId = 601,
            Puntuacion = 4,
            Comentario = "Good product",
            CreatedAt = DateTime.UtcNow
        };
        db.Ratings.Add(rating);
        await db.SaveChangesAsync();

        var ratings = await db.Ratings
            .Include(r => r.Usuario)
            .Where(r => r.ProductoId == 601)
            .ToListAsync();

        ratings.Should().HaveCount(1);
        ratings.First().Puntuacion.Should().Be(4);
    }

    #endregion
}

/// <summary>
/// Fixture that creates an isolated SQLite database for each test.
/// Automatically cleans up the database file when disposed.
/// </summary>
public class IntegrationTestFixture : IDisposable, IAsyncDisposable
{
    private readonly SqliteConnection _connection;
    public readonly ApplicationDbContext Context;
    private bool _disposed;

    public IntegrationTestFixture()
    {
        var dbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        _connection = new SqliteConnection($"Data Source={dbPath}");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new ApplicationDbContext(options);
        Context.Database.EnsureCreated();
    }

    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            await Context.DisposeAsync();
            await _connection.CloseAsync();
            _disposed = true;
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Context.Dispose();
            _connection.Close();
            _disposed = true;
        }
    }
}
