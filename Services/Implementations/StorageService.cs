using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations;

/// <summary>
/// Servicio de gestión de almacenamiento de archivos
/// Guarda archivos en wwwroot para acceso web directo
/// </summary>
public class StorageService : IStorageService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<StorageService> _logger;
    private readonly string _uploadPath;
    private readonly long _maxFileSize;
    private readonly string[] _allowedExtensions;

    public StorageService(
        IWebHostEnvironment environment, 
        IConfiguration configuration,
        ILogger<StorageService> logger)
    {
        _environment = environment;
        _logger = logger;
        _uploadPath = configuration["Storage:UploadPath"] ?? "uploads";
        _maxFileSize = configuration.GetValue<long>("Storage:MaxFileSize", 5242880); // 5MB default
        _allowedExtensions = configuration.GetSection("Storage:AllowedExtensions").Get<string[]>() 
            ?? new[] { ".jpg", ".jpeg", ".png", ".gif" };
        
        _logger.LogInformation("📁 Directorio de uploads: {Path}", _uploadPath);
    }

    public async Task<Result<string, DomainError>> SaveFileAsync(IFormFile file, string folder)
    {
        try
        {
            if (file == null || file.Length == 0)
                return Result.Failure<string, DomainError>(
                    ProductError.InvalidData("El archivo está vacío"));

            if (file.Length > _maxFileSize)
                return Result.Failure<string, DomainError>(
                    ProductError.InvalidData($"El archivo excede el tamaño máximo de {_maxFileSize / 1024 / 1024}MB"));

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
                return Result.Failure<string, DomainError>(
                    ProductError.InvalidData($"Extensión de archivo no permitida. Permitidas: {string.Join(", ", _allowedExtensions)}"));

            // Usar wwwroot para que los archivos sean accesibles desde la web
            var uploadDir = Path.Combine(_environment.WebRootPath, _uploadPath, folder);
            Directory.CreateDirectory(uploadDir);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);

            await using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Retornar ruta relativa desde wwwroot con / al inicio
            var relativePath = $"/{_uploadPath}/{folder}/{fileName}";
            _logger.LogInformation("Archivo guardado: {FilePath}", relativePath);
            
            return Result.Success<string, DomainError>(relativePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error guardando archivo");
            return Result.Failure<string, DomainError>(
                ProductError.InvalidData($"Error al guardar archivo: {ex.Message}"));
        }
    }

    public async Task<Result<bool, DomainError>> DeleteFileAsync(string filePath)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath))
                return Result.Success<bool, DomainError>(true);

            // Usar wwwroot
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            
            if (File.Exists(fullPath))
            {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation("Archivo eliminado: {FilePath}", filePath);
            }

            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error eliminando archivo {FilePath}", filePath);
            return Result.Failure<bool, DomainError>(
                ProductError.InvalidData($"Error al eliminar archivo: {ex.Message}"));
        }
    }

    public bool FileExists(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
        return File.Exists(fullPath);
    }
}
