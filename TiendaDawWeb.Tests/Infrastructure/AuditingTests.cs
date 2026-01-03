using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Data;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;

namespace TiendaDawWeb.Tests.Infrastructure;

/// <summary>
/// OBJETIVO: Validar que la auditoría automática de EF Core funciona.
/// LO QUE BUSCA: Asegurar que CreatedAt y UpdatedAt se rellenan solos sin intervención manual.
/// </summary>
[TestFixture]
public class AuditingTests
{
    private ApplicationDbContext _context;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task Should_Set_CreatedAt_On_Insert()
    {
        // Arrange
        var product = new Product
        {
            Nombre = "Audit Test",
            Descripcion = "Test",
            Precio = 10,
            Categoria = ProductCategory.GAMING,
            PropietarioId = 1
        };

        // Act
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Assert
        Assert.That(product.CreatedAt, Is.Not.EqualTo(default(DateTime)));
        Assert.That(product.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
        Assert.That(product.UpdatedAt, Is.Null);
    }

    [Test]
    public async Task Should_Set_UpdatedAt_On_Update()
    {
        // Arrange
        var product = new Product
        {
            Nombre = "Initial",
            Descripcion = "Initial",
            Precio = 10,
            Categoria = ProductCategory.GAMING,
            PropietarioId = 1
        };
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        var originalCreated = product.CreatedAt;
        await Task.Delay(100); // Pequeña espera para asegurar cambio de timestamp

        // Act
        product.Nombre = "Modified";
        await _context.SaveChangesAsync();

        // Assert
        Assert.That(product.CreatedAt, Is.EqualTo(originalCreated)); // El creado no cambia
        Assert.That(product.UpdatedAt, Is.Not.Null);
        Assert.That(product.UpdatedAt, Is.GreaterThan(originalCreated));
    }
}
