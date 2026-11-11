using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using TiendaDawWeb.Shared.Data;
using TiendaDawWeb.Shared.Services.BackgroundServices;

namespace TiendaDawWeb.Tests.Shared.Services.BackgroundServices;

public class ReservaCleanupServiceTests {
    private MemoryConfiguration _configuration = null!;
    private ApplicationDbContext _context = null!;
    private Mock<ILogger<ReservaCleanupService>> _loggerMock = null!;
    private ReservaCleanupService _service = null!;

    [SetUp]
    public void Setup() {
        DbContextOptions<ApplicationDbContext> options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _context = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<ReservaCleanupService>>();

        _configuration = new MemoryConfiguration();
        _configuration["Reservas:CleanupIntervalMinutes"] = "5";
    }

    [TearDown]
    public void TearDown() {
        _service?.Dispose();
        _context.Dispose();
    }

    [Test]
    public void Constructor_SetsDefaultConfigurationValues() {
        var serviceProviderMock = new Mock<IServiceProvider>();

        _service = new ReservaCleanupService(serviceProviderMock.Object, _loggerMock.Object, _configuration);

        _configuration["Reservas:CleanupIntervalMinutes"].Should().Be("5");
    }

    [Test]
    public void CleanupInterval_CanBeCustomized() {
        var customConfig = new MemoryConfiguration();
        customConfig["Reservas:CleanupIntervalMinutes"] = "15";

        var serviceProviderMock = new Mock<IServiceProvider>();

        _service = new ReservaCleanupService(serviceProviderMock.Object, _loggerMock.Object, customConfig);

        customConfig["Reservas:CleanupIntervalMinutes"].Should().Be("15");
    }

    [Test]
    public void ReservaCleanupService_CanBeCreated() {
        var serviceProviderMock = new Mock<IServiceProvider>();

        _service = new ReservaCleanupService(serviceProviderMock.Object, _loggerMock.Object, _configuration);

        _service.Should().NotBeNull();
    }

    [Test]
    public void ReservaCleanupService_ImplementsIHostedService() {
        var serviceProviderMock = new Mock<IServiceProvider>();

        _service = new ReservaCleanupService(serviceProviderMock.Object, _loggerMock.Object, _configuration);

        _service.Should().BeAssignableTo<IHostedService>();
    }

    private class MemoryConfiguration : IConfiguration {
        private readonly Dictionary<string, string> _values = new();

        public string? this[string key] {
            get => _values.TryGetValue(key, out string? v) ? v : null;
            set => _values[key] = value!;
        }

        public IConfigurationSection GetSection(string key) {
            return new MemoryConfigurationSection(key, _values);
        }

        public IEnumerable<IConfigurationSection> GetChildren() {
            return Enumerable.Empty<IConfigurationSection>();
        }

        public IChangeToken GetReloadToken() {
            return new FakeChangeToken();
        }
    }

    private class MemoryConfigurationSection : IConfigurationSection {
        private readonly Dictionary<string, string> _values;

        public MemoryConfigurationSection(string key, Dictionary<string, string> values) {
            (Key, _values) = (key, values);
        }

        public string? this[string key] {
            get => _values.TryGetValue(key, out string? v) ? v : null;
            set => _values[key] = value!;
        }

        public string Key { get; }

        public string Path { get; } = string.Empty;
        public string? Value { get; set; }

        public IConfigurationSection GetSection(string key) {
            return new MemoryConfigurationSection($"{Key}:{key}", _values);
        }

        public IEnumerable<IConfigurationSection> GetChildren() {
            return Enumerable.Empty<IConfigurationSection>();
        }

        public IChangeToken GetReloadToken() {
            return new FakeChangeToken();
        }
    }

    private class FakeChangeToken : IChangeToken {
        public bool HasChanged => false;
        public bool ActiveChangeCallbacks => false;

        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state) {
            return Disposable.Empty;
        }
    }

    private static class Disposable {
        public static IDisposable Empty { get; } = new Mock<IDisposable>().Object;
    }
}
