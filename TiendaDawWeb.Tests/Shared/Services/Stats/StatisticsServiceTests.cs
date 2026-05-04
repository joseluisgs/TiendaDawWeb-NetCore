using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Dto.Stats;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Models.Enums;
using TiendaDawWeb.Shared.Services.Stats;

namespace TiendaDawWeb.Tests.Shared.Services;

public class StatisticsServiceTests
{
    private ApplicationDbContext _context = null!;
    private StatisticsService _service = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _service = new StatisticsService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    #region GetSalesByCategoryAsync Tests

    [Test]
    public async Task GetSalesByCategoryAsync_ReturnsEmpty_WhenNoProducts()
    {
        var result = await _service.GetSalesByCategoryAsync();

        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetSalesByCategoryAsync_ReturnsSalesByCategory()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        var product1 = new Product { Id = 1, Nombre = "Product1", Categoria = ProductCategory.SMARTPHONES, CompraId = 1, PropietarioId = 1, Propietario = user };
        var product2 = new Product { Id = 2, Nombre = "Product2", Categoria = ProductCategory.SMARTPHONES, CompraId = 2, PropietarioId = 1, Propietario = user };
        var product3 = new Product { Id = 3, Nombre = "Product3", Categoria = ProductCategory.LAPTOPS, CompraId = 3, PropietarioId = 1, Propietario = user };
        _context.Users.Add(user);
        _context.Products.AddRange(product1, product2, product3);
        await _context.SaveChangesAsync();

        var result = (await _service.GetSalesByCategoryAsync()).ToList();

        result.Should().HaveCount(2);
        result.First(x => x.Categoria == ProductCategory.SMARTPHONES).Cantidad.Should().Be(2);
        result.First(x => x.Categoria == ProductCategory.LAPTOPS).Cantidad.Should().Be(1);
    }

    #endregion

    #region GetMonthlySalesAsync Tests

    [Test]
    public async Task GetMonthlySalesAsync_ReturnsEmpty_WhenNoPurchases()
    {
        var result = await _service.GetMonthlySalesAsync();

        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetMonthlySalesAsync_ReturnsMonthlySales()
    {
        var user = new User { Id = 1, UserName = "testuser" };
        var fechaAnterior = DateTime.UtcNow.AddMonths(-2);
        var fechaReciente = DateTime.UtcNow.AddMonths(-1);
        var purchase1 = new TiendaDawWeb.Shared.Models.Purchase { Id = 1, CompradorId = 1, Comprador = user, FechaCompra = fechaAnterior, Total = 100 };
        var purchase2 = new TiendaDawWeb.Shared.Models.Purchase { Id = 2, CompradorId = 1, Comprador = user, FechaCompra = fechaReciente, Total = 200 };
        _context.Users.Add(user);
        _context.Purchases.AddRange(purchase1, purchase2);
        await _context.SaveChangesAsync();

        var result = (await _service.GetMonthlySalesAsync(12)).ToList();

        result.Should().HaveCount(2);
        result.Sum(x => x.TotalVentas).Should().Be(300);
    }

    #endregion

    #region GetTopBuyersAsync Tests

    [Test]
    public async Task GetTopBuyersAsync_ReturnsEmpty_WhenNoPurchases()
    {
        var result = await _service.GetTopBuyersAsync();

        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetTopBuyersAsync_ReturnsTopBuyers()
    {
        var user1 = new User { Id = 1, UserName = "buyer1" };
        var user2 = new User { Id = 2, UserName = "buyer2" };
        var purchase1 = new TiendaDawWeb.Shared.Models.Purchase { Id = 1, CompradorId = 1, Comprador = user1, Total = 100 };
        var purchase2 = new TiendaDawWeb.Shared.Models.Purchase { Id = 2, CompradorId = 1, Comprador = user1, Total = 150 };
        var purchase3 = new TiendaDawWeb.Shared.Models.Purchase { Id = 3, CompradorId = 2, Comprador = user2, Total = 200 };
        _context.Users.AddRange(user1, user2);
        _context.Purchases.AddRange(purchase1, purchase2, purchase3);
        await _context.SaveChangesAsync();

        var result = (await _service.GetTopBuyersAsync(10)).ToList();

        result.Should().HaveCount(2);
        result[0].Nombre.Should().Be("buyer1");
        result[0].TotalCompras.Should().Be(2);
        result[1].Nombre.Should().Be("buyer2");
    }

    [Test]
    public async Task GetTopBuyersAsync_ReturnsLimitedResults()
    {
        var users = Enumerable.Range(1, 15).Select(i => new User { Id = i, UserName = $"user{i}" }).ToList();
        var purchases = users.Select((u, i) => new TiendaDawWeb.Shared.Models.Purchase { Id = i + 1, CompradorId = u.Id, Comprador = u, Total = 100 }).ToList();
        _context.Users.AddRange(users);
        _context.Purchases.AddRange(purchases);
        await _context.SaveChangesAsync();

        var result = (await _service.GetTopBuyersAsync(5)).ToList();

        result.Should().HaveCount(5);
    }

    #endregion

    #region GetTopSellersAsync Tests

    [Test]
    public async Task GetTopSellersAsync_ReturnsEmpty_WhenNoSoldProducts()
    {
        var result = await _service.GetTopSellersAsync();

        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetTopSellersAsync_ReturnsTopSellers()
    {
        var user1 = new User { Id = 1, UserName = "seller1" };
        var user2 = new User { Id = 2, UserName = "seller2" };
        var product1 = new Product { Id = 1, Nombre = "P1", PropietarioId = 1, Propietario = user1, CompraId = 1 };
        var product2 = new Product { Id = 2, Nombre = "P2", PropietarioId = 1, Propietario = user1, CompraId = 2 };
        var product3 = new Product { Id = 3, Nombre = "P3", PropietarioId = 2, Propietario = user2, CompraId = 3 };
        _context.Users.AddRange(user1, user2);
        _context.Products.AddRange(product1, product2, product3);
        await _context.SaveChangesAsync();

        var result = (await _service.GetTopSellersAsync(10)).ToList();

        result.Should().HaveCount(2);
        result[0].Nombre.Should().Be("seller1");
        result[0].ProductosVendidos.Should().Be(2);
        result[1].Nombre.Should().Be("seller2");
    }

    [Test]
    public async Task GetTopSellersAsync_IgnoresUnsoldProducts()
    {
        var user = new User { Id = 1, UserName = "seller" };
        var soldProduct = new Product { Id = 1, Nombre = "Sold", PropietarioId = 1, Propietario = user, CompraId = 1 };
        var unsoldProduct = new Product { Id = 2, Nombre = "Unsold", PropietarioId = 1, Propietario = user, CompraId = null };
        _context.Users.Add(user);
        _context.Products.AddRange(soldProduct, unsoldProduct);
        await _context.SaveChangesAsync();

        var result = (await _service.GetTopSellersAsync(10)).ToList();

        result.Should().HaveCount(1);
        result[0].ProductosVendidos.Should().Be(1);
    }

    #endregion
}