using CSharpFunctionalExtensions;
using TiendaDawWeb.Errors;

namespace TiendaDawWeb.Services.Interfaces;

/// <summary>
/// Interfaz de servicio para gestión de almacenamiento de archivos
/// </summary>
public interface IStorageService
{
    Task<Result<string, DomainError>> SaveFileAsync(IFormFile file, string folder);
    Task<Result<bool, DomainError>> DeleteFileAsync(string filePath);
    bool FileExists(string filePath);
}
