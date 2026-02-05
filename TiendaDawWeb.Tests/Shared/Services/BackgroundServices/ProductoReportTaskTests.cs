using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.BackgroundServices;
using TiendaDawWeb.Shared.Services.Email;
using ProductModel = TiendaDawWeb.Shared.Models.Product;

namespace TiendaDawWeb.Tests.Shared.Services.BackgroundServices;

public class ProductoReportTaskTests
{
    private ApplicationDbContext _context = null!;
    private Mock<IEmailService> _emailServiceMock = null!;
    private Mock<ILogger<ProductoReportTask>> _loggerMock = null!;
    private MemoryConfiguration _configuration = null!;
    private ProductoReportTask _service = null!;

    private class MemoryConfiguration : IConfiguration
    {
        private readonly Dictionary<string, string> _values = new();
        public string? this[string key] { get => _values.TryGetValue(key, out var v) ? v : null; set => _values[key] = value!; }
        public IConfigurationSection GetSection(string key) => new MemoryConfigurationSection(key, _values);
        public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();
        public IChangeToken GetReloadToken() => new FakeChangeToken();
    }

    private class MemoryConfigurationSection : IConfigurationSection
    {
        private readonly string _key;
        private readonly Dictionary<string, string> _values;
        public string? this[string key] { get => _values.TryGetValue(key, out var v) ? v : null; set => _values[key] = value!; }
        public string Key => _key;
        public string Path { get; set; } = string.Empty;
        public string? Value { get; set; }
        public MemoryConfigurationSection(string key, Dictionary<string, string> values) => (_key, _values) = (key, values);
        public IConfigurationSection GetSection(string key) => new MemoryConfigurationSection($"{_key}:{key}", _values);
        public IEnumerable<IConfigurationSection> GetChildren() => Enumerable.Empty<IConfigurationSection>();
        public IChangeToken GetReloadToken() => new FakeChangeToken();
    }

    private class FakeChangeToken : IChangeToken
    {
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) => Disposable.Empty;
    }

    private static class Disposable
    {
        public static IDisposable Empty { get; } = new Mock<IDisposable>().Object;
    }

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _emailServiceMock = new Mock<IEmailService>();
        _loggerMock = new Mock<ILogger<ProductoReportTask>>();

        _configuration = new MemoryConfiguration();
        _configuration["Scheduler:ProductoReportDays"] = "7";
        _configuration["IsDevelopment"] = "true";

        _service = new ProductoReportTask(_context, _emailServiceMock.Object, _loggerMock.Object, _configuration);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_ReturnsSuccess_InDevelopmentMode()
    {
        var result = await _service.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task ExecuteAsync_SkipsDatabaseQuery_InDevelopmentMode()
    {
        var user = new User { Id = 1, Email = "test@test.com", UserName = "test", EmailConfirmed = true, Deleted = false };
        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var result = await _service.ExecuteAsync();

        result.IsSuccess.Should().BeTrue();
        _emailServiceMock.Verify(e => e.EnqueueEmail(It.IsAny<EmailMessage>()), Times.Never);
    }

    [Test]
    public async Task GenerateHtmlEmail_GeneratesValidHtml()
    {
        var productos = new List<ProductModel>
        {
            new ProductModel { Id = 1, Nombre = "Product 1", Descripcion = "Description 1", Precio = 100 },
            new ProductModel { Id = 2, Nombre = "Product 2", Descripcion = "Description 2", Precio = 200 }
        };

        var html = GenerateHtmlEmail(productos, "TestUser");

        html.Should().Contain("Product 1");
        html.Should().Contain("Description 1");
        html.Should().Contain("100");
        html.Should().Contain("Product 2");
        html.Should().Contain("TestUser");
        html.Should().Contain("Novedades de la Semana");
    }

    [Test]
    public async Task GenerateHtmlEmail_HandlesEmptyProducts()
    {
        var productos = new List<ProductModel>();

        var html = GenerateHtmlEmail(productos, "TestUser");

        html.Should().Contain("TestUser");
        html.Should().Contain("Novedades de la Semana");
    }

    private static string GenerateHtmlEmail(IEnumerable<ProductModel> productos, string userName)
    {
        var productosHtml = string.Concat(productos.Select(p => string.Format(@"
            <div style=""border: 1px solid #ddd; padding: 15px; margin: 10px 0; border-radius: 8px;"">
                <h3 style=""margin: 0 0 10px 0;"">{0}</h3>
                <p style=""margin: 0; color: #666;"">{1}</p>
                <p style=""margin: 10px 0 0 0; font-weight: bold; color: #28a745;"">
                    {2}
                </p>
            </div>", p.Nombre, p.Descripcion, p.Precio.ToString("C"))));

        return string.Format(@"<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: #007bff; color: white; padding: 20px; text-align: center; border-radius: 8px 8px 0 0; }}
        .content {{ background: #f9f9f9; padding: 20px; border-radius: 0 0 8px 8px; }}
        .footer {{ text-align: center; padding: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""header"">
            <h1>Novedades de la Semana</h1>
        </div>
        <div class=""content"">
            <p>Hola <strong>{0}</strong></p>
            <p>Te presentamos los <strong>{1}</strong> productos anadidos esta semana:</p>
            {2}
        </div>
        <div class=""footer"">
            <p>WalaDaw - Tu tienda de confianza</p>
        </div>
    </div>
</body>
</html>", userName, productos.Count(), productosHtml);
    }
}
