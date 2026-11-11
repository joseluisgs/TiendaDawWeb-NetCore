using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Moq;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Services.Storage;

namespace TiendaDawWeb.Tests.Shared.Services.Storage;

public class StorageServiceTests {
    private MemoryConfigurationRoot _configuration = null!;
    private Mock<IWebHostEnvironment> _environmentMock = null!;
    private Mock<ILogger<StorageService>> _loggerMock = null!;
    private StorageService _service = null!;
    private string _webRootPath = null!;

    [SetUp]
    public void Setup() {
        _environmentMock = new Mock<IWebHostEnvironment>();
        _loggerMock = new Mock<ILogger<StorageService>>();
        _webRootPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}");
        _environmentMock.Setup(e => e.WebRootPath).Returns(_webRootPath);

        _configuration = new MemoryConfigurationRoot();
        _configuration.Add("Storage:UploadPath", "uploads");
        _configuration.Add("Storage:MaxFileSize", "5242880");
        _configuration.Add("Storage:AllowedExtensions:0", ".jpg");
        _configuration.Add("Storage:AllowedExtensions:1", ".jpeg");
        _configuration.Add("Storage:AllowedExtensions:2", ".png");
        _configuration.Add("Storage:AllowedExtensions:3", ".gif");

        _service = new StorageService(_environmentMock.Object, _configuration, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown() {
        if (Directory.Exists(_webRootPath)) {
            Directory.Delete(_webRootPath, true);
        }
    }

    private class MemoryConfigurationRoot : IConfiguration {
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

        public void Add(string key, string value) {
            _values[key] = value;
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

    #region SaveFileAsync Tests

    [Test]
    public async Task SaveFileAsync_ReturnsFailure_WhenFileIsNull() {
        Result<string, DomainError> result = await _service.SaveFileAsync(null!, "products");

        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task SaveFileAsync_ReturnsFailure_WhenFileIsEmpty() {
        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.Length).Returns(0);

        Result<string, DomainError> result = await _service.SaveFileAsync(formFile.Object, "products");

        result.IsFailure.Should().BeTrue();
    }

    [Test]
    public async Task SaveFileAsync_ReturnsFailure_WhenExtensionNotAllowed() {
        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.Length).Returns(1000);
        formFile.Setup(f => f.FileName).Returns("test.exe");

        Result<string, DomainError> result = await _service.SaveFileAsync(formFile.Object, "products");

        result.IsFailure.Should().BeTrue();
        result.Error.ToString().Should().Contain("Extension no permitida");
    }

    #endregion

    #region DeleteFileAsync Tests

    [Test]
    public async Task DeleteFileAsync_DeletesExistingFile_Success() {
        string folder = Path.Combine(_webRootPath, "uploads", "products");
        Directory.CreateDirectory(folder);
        string filePath = Path.Combine(folder, "test.jpg");
        File.WriteAllText(filePath, "test content");

        string relativePath = "/uploads/products/test.jpg";
        Result<bool, DomainError> result = await _service.DeleteFileAsync(relativePath);

        result.IsSuccess.Should().BeTrue();
        File.Exists(filePath).Should().BeFalse();
    }

    [Test]
    public async Task DeleteFileAsync_ReturnsSuccess_WhenFileNotExists() {
        Result<bool, DomainError> result = await _service.DeleteFileAsync("/uploads/products/nonexistent.jpg");

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task DeleteFileAsync_ReturnsSuccess_WhenPathIsNull() {
        Result<bool, DomainError> result = await _service.DeleteFileAsync(null!);

        result.IsSuccess.Should().BeTrue();
    }

    [Test]
    public async Task DeleteFileAsync_ReturnsSuccess_WhenPathIsEmpty() {
        Result<bool, DomainError> result = await _service.DeleteFileAsync("");

        result.IsSuccess.Should().BeTrue();
    }

    #endregion

    #region FileExists Tests

    [Test]
    public void FileExists_ReturnsTrue_WhenFileExists() {
        string folder = Path.Combine(_webRootPath, "uploads", "products");
        Directory.CreateDirectory(folder);
        string filePath = Path.Combine(folder, "test.jpg");
        File.WriteAllText(filePath, "test content");

        bool result = _service.FileExists("/uploads/products/test.jpg");

        result.Should().BeTrue();
    }

    [Test]
    public void FileExists_ReturnsFalse_WhenFileNotExists() {
        bool result = _service.FileExists("/uploads/products/nonexistent.jpg");

        result.Should().BeFalse();
    }

    [Test]
    public void FileExists_ReturnsFalse_WhenPathIsNull() {
        bool result = _service.FileExists(null!);

        result.Should().BeFalse();
    }

    [Test]
    public void FileExists_ReturnsFalse_WhenPathIsEmpty() {
        bool result = _service.FileExists("");

        result.Should().BeFalse();
    }

    #endregion
}
