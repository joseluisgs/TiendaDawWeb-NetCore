using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TiendaDawWeb.Services.Implementations;
using FluentAssertions;
using System.Text;

namespace TiendaDawWeb.Tests.Services;

[TestFixture]
public class StorageServiceTests
{
    private Mock<IWebHostEnvironment> _envMock;
    private Mock<IConfiguration> _configMock;
    private Mock<ILogger<StorageService>> _loggerMock;
    private StorageService _storageService;
    private string _tempPath;

    [SetUp]
    public void Setup()
    {
        // Crear un directorio temporal para cada test
        _tempPath = Path.Combine(Path.GetTempPath(), "TiendaDawWebTests_" + Guid.NewGuid());
        Directory.CreateDirectory(_tempPath);

        // Mock Environment
        _envMock = new Mock<IWebHostEnvironment>();
        _envMock.Setup(e => e.WebRootPath).Returns(_tempPath);

        // Mock Configuration
        _configMock = new Mock<IConfiguration>();

        // Mock Logger
        _loggerMock = new Mock<ILogger<StorageService>>();

        // Mock "Storage:UploadPath" (Indexer)
        _configMock.Setup(c => c["Storage:UploadPath"]).Returns("uploads");

        // Mock "Storage:MaxFileSize" via GetSection
        var maxFileSizeSection = new Mock<IConfigurationSection>();
        maxFileSizeSection.Setup(s => s.Value).Returns("1024");
        _configMock.Setup(c => c.GetSection("Storage:MaxFileSize")).Returns(maxFileSizeSection.Object);

        // Mock "Storage:AllowedExtensions" via GetSection
        var extensionsSection = new Mock<IConfigurationSection>();
        // Get<string[]> uses Bind internally, which iterates children. 
        // Simpler way: Mock GetSection to return a real ConfigurationSection from an in-memory collection
        // or just use a real ConfigurationRoot for the whole thing as it's easier than mocking deep structure.
        
        var inMemorySettings = new Dictionary<string, string?> {
            {"Storage:UploadPath", "uploads"},
            {"Storage:MaxFileSize", "1024"},
            {"Storage:AllowedExtensions:0", ".jpg"},
            {"Storage:AllowedExtensions:1", ".png"}
        };
        
        IConfiguration realConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Use real configuration for tests instead of mocking complexities
        // We only mock IConfiguration interface
        _storageService = new StorageService(_envMock.Object, realConfiguration, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        // Limpiar directorio temporal
        if (Directory.Exists(_tempPath))
        {
            Directory.Delete(_tempPath, true);
        }
    }

    [Test]
    public async Task SaveFileAsync_ShouldSaveFile_WhenValid()
    {
        // Arrange
        var content = "Fake image content";
        var fileName = "test.jpg";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns(fileName);
        fileMock.Setup(f => f.Length).Returns(stream.Length);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Callback<Stream, CancellationToken>((target, token) => stream.CopyTo(target))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _storageService.SaveFileAsync(fileMock.Object, "products");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().StartWith("/uploads/products/");
        result.Value.Should().EndWith(".jpg");

        // Verify file exists on disk
        var savedPath = Path.Combine(_tempPath, result.Value.TrimStart('/'));
        File.Exists(savedPath).Should().BeTrue();
    }

    [Test]
    public async Task SaveFileAsync_ShouldReturnError_WhenExtensionNotAllowed()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("malicious.exe");
        fileMock.Setup(f => f.Length).Returns(100);

        // Act
        var result = await _storageService.SaveFileAsync(fileMock.Object, "products");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("Extensión de archivo no permitida");
    }

    [Test]
    public async Task SaveFileAsync_ShouldReturnError_WhenFileTooLarge()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.FileName).Returns("large.jpg");
        fileMock.Setup(f => f.Length).Returns(2048); // 2KB > 1KB limit

        // Act
        var result = await _storageService.SaveFileAsync(fileMock.Object, "products");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Message.Should().Contain("tamaño máximo");
    }

    [Test]
    public async Task DeleteFileAsync_ShouldDeleteFile_WhenExists()
    {
        // Arrange: Create a dummy file first
        var relativePath = "uploads/products/todelete.jpg";
        var fullPath = Path.Combine(_tempPath, relativePath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        await File.WriteAllTextAsync(fullPath, "content");

        // Act
        var result = await _storageService.DeleteFileAsync("/" + relativePath);

        // Assert
        result.IsSuccess.Should().BeTrue();
        File.Exists(fullPath).Should().BeFalse();
    }

    [Test]
    public async Task DeleteFileAsync_ShouldReturnSuccess_WhenFileDoesNotExist()
    {
        // Act
        var result = await _storageService.DeleteFileAsync("/uploads/nonexistent.jpg");

        // Assert
        result.IsSuccess.Should().BeTrue(); // Idempotencia: Si no existe, "ya está borrado"
    }
}
