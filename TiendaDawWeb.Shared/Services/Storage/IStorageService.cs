using Microsoft.AspNetCore.Http;
using CSharpFunctionalExtensions;
using TiendaDawWeb.Shared.Errors;

namespace TiendaDawWeb.Shared.Services.Storage;

/// <summary>
/// Interfaz de servicio para gestión de almacenamiento de archivos
/// </summary>
public interface IStorageService
{
    Task<Result<string, DomainError>> SaveFileAsync(IFormFile file, string folder);
    Task<Result<bool, DomainError>> DeleteFileAsync(string filePath);
    bool FileExists(string filePath);
}
