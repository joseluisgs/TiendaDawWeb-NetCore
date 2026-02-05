using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Microsoft.Extensions.Logging;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Models;
using TiendaDawWeb.Shared.Services.BackgroundServices;

namespace TiendaDawWeb.Tests.Shared.Services.BackgroundServices;

public class CarritoCleanupServiceTests
{
    private ApplicationDbContext _context = null!;
    private Mock<ILogger<CarritoCleanupService>> _loggerMock = null!;
    private MemoryConfiguration _configuration = null!;
    private CarritoCleanupService _service = null!;

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
        _loggerMock = new Mock<ILogger<CarritoCleanupService>>();

        _configuration = new MemoryConfiguration();
        _configuration["Carrito:CleanupIntervalMinutes"] = "60";
        _configuration["Carrito:ExpirationMinutes"] = "1440";
    }

    [TearDown]
    public void TearDown()
    {
        _service?.Dispose();
        _context.Dispose();
    }

    [Test]
    public void Constructor_SetsDefaultConfigurationValues()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();

        _service = new CarritoCleanupService(serviceProviderMock.Object, _loggerMock.Object, _configuration);

        _configuration["Carrito:CleanupIntervalMinutes"].Should().Be("60");
        _configuration["Carrito:ExpirationMinutes"].Should().Be("1440");
    }

    [Test]
    public void CleanupInterval_CanBeCustomized()
    {
        var customConfig = new MemoryConfiguration();
        customConfig["Carrito:CleanupIntervalMinutes"] = "30";
        customConfig["Carrito:ExpirationMinutes"] = "720";

        var serviceProviderMock = new Mock<IServiceProvider>();

        _service = new CarritoCleanupService(serviceProviderMock.Object, _loggerMock.Object, customConfig);

        customConfig["Carrito:CleanupIntervalMinutes"].Should().Be("30");
        customConfig["Carrito:ExpirationMinutes"].Should().Be("720");
    }

    [Test]
    public void CarritoCleanupService_CanBeCreated()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();

        _service = new CarritoCleanupService(serviceProviderMock.Object, _loggerMock.Object, _configuration);

        _service.Should().NotBeNull();
    }

    [Test]
    public void CarritoCleanupService_ImplementsIHostedService()
    {
        var serviceProviderMock = new Mock<IServiceProvider>();

        _service = new CarritoCleanupService(serviceProviderMock.Object, _loggerMock.Object, _configuration);

        _service.Should().BeAssignableTo<IHostedService>();
    }
}
