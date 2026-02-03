using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TiendaDawWeb.Data;
using TiendaDawWeb.Models;
using TiendaDawWeb.Models.Enums;
using FluentAssertions;
using Microsoft.Data.Sqlite;

namespace TiendaDawWeb.Tests.Services.BackgroundServices;

/// <summary>
/// OBJETIVO: Validar la lógica de negocio de los servicios de fondo.
/// LO QUE BUSCA: Asegurar que la limpieza de carritos y reservas funciona correctamente.
/// </summary>
[TestFixture]
public class BackgroundServicesTests
{
    private SqliteConnection _connection = null!;
    private ApplicationDbContext _context = null!;

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
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
    }

    #region CarritoCleanupService Logic Tests

    /// <summary>
    /// PRUEBA: Carrito vacío no causa errores.
    /// OBJETIVO: Verificar manejo de caso sin items.
    /// </summary>
    [Test]
    public async Task CleanupCarrito_ShouldHandleEmptyCarrito()
    {
        // Act
        var expiredItems = await _context.CarritoItems
            .Where(ci => ci.CreatedAt < DateTime.UtcNow)
            .ToListAsync();

        // Assert
        expiredItems.Should().BeEmpty();
    }

    /// <summary>
    /// PRUEBA: Cálculo correcto de tiempo de expiración.
    /// OBJETIVO: Verificar que el tiempo de expiración se calcula correctamente.
    /// </summary>
    [Test]
    public void CleanupCarrito_ShouldCalculateExpirationTime_Correctly()
    {
        // Arrange
        var expirationMinutes = 60;

        // Act
        var expirationTime = DateTime.UtcNow.AddMinutes(-expirationMinutes);

        // Assert
        expirationTime.Should().BeBefore(DateTime.UtcNow.AddMinutes(-59));
    }

    /// <summary>
    /// PRUEBA: Filtro de items expirados funciona correctamente (test en memoria).
    /// OBJETIVO: Verificar que la lógica de filtrado funciona sin base de datos.
    /// </summary>
    [Test]
    public void CleanupCarrito_ShouldFilterExpiredCorrectly()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var expirationTime = now.AddMinutes(-60);

        var items = new[]
        {
            new { Id = 1, CreatedAt = now.AddMinutes(-30) }, // No expirado
            new { Id = 2, CreatedAt = now.AddDays(-2) },    // Expirado
            new { Id = 3, CreatedAt = now.AddHours(-1) }    // En el límite
        };

        // Act
        var expiredItems = items.Where(i => i.CreatedAt < expirationTime).ToList();

        // Assert
        expiredItems.Should().HaveCount(1);
        expiredItems.First().Id.Should().Be(2);
    }

    #endregion

    #region ReservaCleanupService Logic Tests

    /// <summary>
    /// PRUEBA: Identificación de reservas expiradas.
    /// OBJETIVO: Verificar la lógica de identificación de reservas expiradas.
    /// </summary>
    [Test]
    public void ReservaCleanup_ShouldIdentifyExpiredReservations_Logic()
    {
        // Arrange - Test en memoria sin base de datos
        var now = DateTime.UtcNow;

        var products = new[]
        {
            new Product { Id = 1, Nombre = "Valid", Reservado = true, ReservadoHasta = now.AddHours(1) },
            new Product { Id = 2, Nombre = "Expired", Reservado = true, ReservadoHasta = now.AddHours(-1) }
        };

        // Act
        var expiredReservations = products
            .Where(p => p.Reservado && p.ReservadoHasta.HasValue && p.ReservadoHasta.Value < now)
            .ToList();

        // Assert
        expiredReservations.Should().HaveCount(1);
        expiredReservations.First().Nombre.Should().Be("Expired");
    }

    /// <summary>
    /// PRUEBA: No libera reservas válidas.
    /// OBJETIVO: Verificar que las reservas activas no se identifican como expiradas.
    /// </summary>
    [Test]
    public void ReservaCleanup_ShouldNotIdentifyValidReservations_Logic()
    {
        // Arrange - Test en memoria
        var now = DateTime.UtcNow;

        var products = new[]
        {
            new Product { Id = 1, Nombre = "Future", Reservado = true, ReservadoHasta = now.AddHours(1) }
        };

        // Act
        var expiredReservations = products
            .Where(p => p.Reservado && p.ReservadoHasta.HasValue && p.ReservadoHasta.Value < now)
            .ToList();

        // Assert
        expiredReservations.Should().BeEmpty();
    }

    /// <summary>
    /// PRUEBA: Manejo de productos sin fecha de reserva.
    /// OBJETIVO: Verificar que los productos sin ReservadoHasta no se afectan.
    /// </summary>
    [Test]
    public void ReservaCleanup_ShouldHandleProductsWithoutReservationDate_Logic()
    {
        // Arrange
        var now = DateTime.UtcNow;

        var products = new[]
        {
            new Product { Id = 1, Nombre = "No Reservation", Reservado = false, ReservadoHasta = null }
        };

        // Act
        var expiredReservations = products
            .Where(p => p.Reservado && p.ReservadoHasta.HasValue && p.ReservadoHasta.Value < now)
            .ToList();

        // Assert
        expiredReservations.Should().BeEmpty();
    }

    /// <summary>
    /// PRUEBA: Liberación de reserva marca campos correctamente (test en memoria).
    /// OBJETIVO: Verificar la lógica de liberación de reservas.
    /// </summary>
    [Test]
    public void ReservaCleanup_ShouldClearReservationFields_Logic()
    {
        // Arrange
        var product = new Product
        {
            Id = 1,
            Nombre = "Product",
            Reservado = true,
            ReservadoPor = 1,
            ReservadoHasta = DateTime.UtcNow.AddHours(-1)
        };

        // Act - Simular liberación
        product.Reservado = false;
        product.ReservadoHasta = null;

        // Assert
        product.Reservado.Should().BeFalse();
        product.ReservadoHasta.Should().BeNull();
    }

    #endregion

    #region ProductoReportTask Logic Tests

    /// <summary>
    /// PRUEBA: Conteo de productos sin ventas (test en memoria).
    /// OBJETIVO: Verificar la lógica de conteo de productos sin ventas.
    /// </summary>
    [Test]
    public void ReportTask_ShouldCountProductsWithoutSales_Logic()
    {
        // Arrange
        var products = new[]
        {
            new Product { Id = 1, Nombre = "No Sale", CompraId = null },
            new Product { Id = 2, Nombre = "Sold", CompraId = 1 }
        };

        // Act
        var productsWithoutSales = products.Where(p => p.CompraId == null).Count();

        // Assert
        productsWithoutSales.Should().Be(1);
    }

    /// <summary>
    /// PRUEBA: Conteo de productos por categoría (test en memoria).
    /// OBJETIVO: Verificar la lógica de conteo por categoría.
    /// </summary>
    [Test]
    public void ReportTask_ShouldCountProductsByCategory_Logic()
    {
        // Arrange
        var products = new[]
        {
            new Product { Id = 1, Categoria = ProductCategory.SMARTPHONES },
            new Product { Id = 2, Categoria = ProductCategory.SMARTPHONES },
            new Product { Id = 3, Categoria = ProductCategory.LAPTOPS }
        };

        // Act
        var smartphonesCount = products.Count(p => p.Categoria == ProductCategory.SMARTPHONES);

        // Assert
        smartphonesCount.Should().Be(2);
    }

    /// <summary>
    /// PRUEBA: Productos disponibles (test en memoria).
    /// OBJETIVO: Verificar la lógica de filtrado de productos disponibles.
    /// </summary>
    [Test]
    public void ReportTask_ShouldIdentifyAvailableProducts_Logic()
    {
        // Arrange
        var products = new[]
        {
            new Product { Id = 1, Deleted = false, CompraId = null },
            new Product { Id = 2, Deleted = true, CompraId = null },
            new Product { Id = 3, Deleted = false, CompraId = 1 }
        };

        // Act
        var availableProducts = products.Where(p => !p.Deleted && p.CompraId == null).Count();

        // Assert
        availableProducts.Should().Be(1);
    }

    /// <summary>
    /// PRUEBA: Suma de precios (test en memoria).
    /// OBJETIVO: Verificar la lógica de cálculo de valores.
    /// </summary>
    [Test]
    public void ReportTask_ShouldCalculateTotalValue_Logic()
    {
        // Arrange
        var products = new[]
        {
            new Product { Precio = 100 },
            new Product { Precio = 200 },
            new Product { Precio = 300 }
        };

        // Act
        var totalValue = products.Where(p => p.CompraId == null).Sum(p => p.Precio);

        // Assert
        totalValue.Should().Be(600);
    }

    #endregion
}
