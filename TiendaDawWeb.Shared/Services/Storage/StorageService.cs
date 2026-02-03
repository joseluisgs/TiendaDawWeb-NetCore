using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using CSharpFunctionalExtensions;
using TiendaDawWeb.Shared.Errors;
using TiendaDawWeb.Shared.Services.Storage;

namespace TiendaDawWeb.Shared.Services.Storage;

/// <summary>
///     Servicio de gestión de almacenamiento de archivos
///     Guarda archivos en wwwroot para acceso web directo
/// </summary>
public class StorageService : IStorageService {
    private readonly string[] _allowedExtensions;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<StorageService> _logger;
    private readonly long _maxFileSize;
    private readonly string _uploadPath;

    public StorageService(IWebHostEnvironment environment, IConfiguration configuration, ILogger<StorageService> logger) {
        _environment = environment;
        _logger = logger;
        _uploadPath = configuration["Storage:UploadPath"] ?? "uploads";
        _maxFileSize = configuration.GetValue<long>("Storage:MaxFileSize", 5242880);
        _allowedExtensions = configuration.GetSection("Storage:AllowedExtensions").Get<string[]>() ?? [".jpg", ".jpeg", ".png", ".gif"];
    }

    public async Task<Result<string, DomainError>> SaveFileAsync(IFormFile file, string folder) {
        try {
            if (file == null || file.Length == 0) return Result.Failure<string, DomainError>(ProductError.InvalidData("El archivo esta vacio"));
            if (file.Length > _maxFileSize) return Result.Failure<string, DomainError>(ProductError.InvalidData($"El archivo excede el tamaño máximo de {_maxFileSize / 1024 / 1024}MB"));
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension)) return Result.Failure<string, DomainError>(ProductError.InvalidData($"Extension no permitida: {string.Join(", ", _allowedExtensions)}"));
            var uploadDir = Path.Combine(_environment.WebRootPath, _uploadPath, folder);
            Directory.CreateDirectory(uploadDir);
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadDir, fileName);
            await using (var stream = new FileStream(filePath, FileMode.Create)) await file.CopyToAsync(stream);
            var relativePath = $"/{_uploadPath}/{folder}/{fileName}";
            return Result.Success<string, DomainError>(relativePath);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error guardando archivo");
            return Result.Failure<string, DomainError>(ProductError.InvalidData($"Error: {ex.Message}"));
        }
    }

    public async Task<Result<bool, DomainError>> DeleteFileAsync(string filePath) {
        try {
            if (string.IsNullOrEmpty(filePath)) return Result.Success<bool, DomainError>(true);
            var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
            if (File.Exists(fullPath)) {
                await Task.Run(() => File.Delete(fullPath));
                _logger.LogInformation("Archivo eliminado: {FilePath}", filePath);
            }
            return Result.Success<bool, DomainError>(true);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error eliminando archivo {FilePath}", filePath);
            return Result.Failure<bool, DomainError>(ProductError.InvalidData($"Error: {ex.Message}"));
        }
    }

    public bool FileExists(string filePath) {
        if (string.IsNullOrEmpty(filePath)) return false;
        var fullPath = Path.Combine(_environment.WebRootPath, filePath.TrimStart('/'));
        return File.Exists(fullPath);
    }
}
