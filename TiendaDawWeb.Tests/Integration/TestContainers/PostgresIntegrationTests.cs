using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;

namespace TiendaDawWeb.Tests.Integration.TestContainers;

/// <summary>
/// Tests de integración con PostgreSQL real (TestContainers).
/// Cada test usa una base de datos aislada para evitar interferencias.
/// </summary>
[TestFixture]
public class PostgresIntegrationTests
{
    private PostgresFixture _fixture = null!;
    private int _dbCounter;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _fixture = new PostgresFixture();
        await _fixture.StartAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _fixture.StopAsync();
    }

    private ApplicationDbContext CreateContext()
    {
        var dbName = $"tiendadaw_test_{Interlocked.Increment(ref _dbCounter)}";
        return _fixture.CreateIsolatedContext(dbName);
    }

    [Test]
    public async Task Product_Create_SavesToPostgres()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var user = new User { UserName = "testuser", Email = "test@test.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var product = new Product
        {
            Nombre = "Laptop Dell",
            Precio = 999.99m,
            Descripcion = "Laptop de prueba",
            Categoria = ProductCategory.LAPTOPS,
            PropietarioId = user.Id,
            Deleted = false
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var saved = await context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        saved.Should().NotBeNull();
        saved!.Nombre.Should().Be("Laptop Dell");
        saved.Precio.Should().Be(999.99m);
    }

    [Test]
    public async Task Product_SoftDelete_MarksAsDeleted()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var user = new User { UserName = "user2", Email = "user2@test.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var product = new Product
        {
            Nombre = "Producto a borrar",
            Precio = 10m,
            Descripcion = "Test",
            Categoria = ProductCategory.AUDIO,
            PropietarioId = user.Id,
            Deleted = false
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        product.SoftDelete("test");
        await context.SaveChangesAsync();

        var deleted = await context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
        deleted!.Deleted.Should().BeTrue();
    }

    [Test]
    public async Task Purchase_Create_SavesToPostgres()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var user = new User { UserName = "buyer", Email = "buyer@test.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var purchase = new Purchase
        {
            CompradorId = user.Id,
            Total = 150m,
            FechaCompra = DateTime.UtcNow
        };
        context.Purchases.Add(purchase);
        await context.SaveChangesAsync();

        var saved = await context.Purchases.FirstOrDefaultAsync(p => p.Id == purchase.Id);
        saved.Should().NotBeNull();
        saved!.Total.Should().Be(150m);
    }

    [Test]
    public async Task Rating_Create_SavesToPostgres()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var user = new User { UserName = "rater", Email = "rater@test.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var product = new Product
        {
            Nombre = "Producto rateable",
            Precio = 50m,
            Descripcion = "Test",
            Categoria = ProductCategory.GAMING,
            PropietarioId = user.Id,
            Deleted = false
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var rating = new Rating
        {
            UsuarioId = user.Id,
            ProductoId = product.Id,
            Puntuacion = 5,
            Comentario = "Excelente",
            CreatedAt = DateTime.UtcNow
        };
        context.Ratings.Add(rating);
        await context.SaveChangesAsync();

        var saved = await context.Ratings.FirstOrDefaultAsync(r => r.Id == rating.Id);
        saved.Should().NotBeNull();
        saved!.Puntuacion.Should().Be(5);
    }

    [Test]
    public async Task Favorite_AddAndQuery_Works()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var user = new User { UserName = "favuser", Email = "fav@test.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        var product = new Product
        {
            Nombre = "Favorito",
            Precio = 30m,
            Descripcion = "Test",
            Categoria = ProductCategory.ACCESSORIES,
            PropietarioId = user.Id,
            Deleted = false
        };
        context.Products.Add(product);
        await context.SaveChangesAsync();

        var favorite = new Favorite
        {
            UsuarioId = user.Id,
            ProductoId = product.Id
        };
        context.Favorites.Add(favorite);
        await context.SaveChangesAsync();

        var exists = await context.Favorites
            .AnyAsync(f => f.UsuarioId == user.Id && f.ProductoId == product.Id);
        exists.Should().BeTrue();
    }

    [Test]
    public async Task Product_QueryWithFilters_Works()
    {
        await using var context = CreateContext();
        await context.Database.EnsureCreatedAsync();

        var user = new User { UserName = "filter", Email = "filter@test.com" };
        context.Users.Add(user);
        await context.SaveChangesAsync();

        context.Products.AddRange(
            new Product { Nombre = "Laptop", Precio = 100m, Categoria = ProductCategory.LAPTOPS, PropietarioId = user.Id, Deleted = false },
            new Product { Nombre = "Mouse", Precio = 20m, Categoria = ProductCategory.ACCESSORIES, PropietarioId = user.Id, Deleted = false },
            new Product { Nombre = "Borrado", Precio = 10m, Categoria = ProductCategory.AUDIO, PropietarioId = user.Id, Deleted = true }
        );
        await context.SaveChangesAsync();

        var active = await context.Products
            .Where(p => !p.Deleted)
            .ToListAsync();

        active.Should().HaveCount(2);

        var laptops = await context.Products
            .Where(p => p.Categoria == ProductCategory.LAPTOPS && !p.Deleted)
            .ToListAsync();

        laptops.Should().HaveCount(1);
    }
}
