using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Shared.Services.Rating;

/// <summary>
/// Contrato para el almacén de estado de valoraciones.
/// </summary>
public interface IRatingStore
{
    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Observable del estado completo de valoraciones.
    /// </summary>
    IObservable<RatingState> State { get; }

    // ═══════════════════════════════════════════════════════════════
    // GETTERS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Observable de la lista de valoraciones.
    /// </summary>
    IObservable<List<Models.Rating>> Ratings { get; }
    
    /// <summary>
    /// Observable del promedio de puntuación.
    /// </summary>
    IObservable<double> Average { get; }
    
    /// <summary>
    /// Observable del conteo de valoraciones.
    /// </summary>
    IObservable<int> Count { get; }
    
    /// <summary>
    /// Observable que indica si hay valoraciones cargadas.
    /// </summary>
    IObservable<bool> HasRatings { get; }

    // ═══════════════════════════════════════════════════════════════
    // ACTIONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// Obtiene el estado actual de forma sincrónica.
    /// </summary>
    RatingState GetState();
    
    /// <summary>
    /// Asegura que las valoraciones estén cargadas para un producto específico.
    /// Evita llamadas redundantes si ya están cargadas.
    /// </summary>
    Task EnsureLoadedAsync(long productId);
    
    /// <summary>
    /// Recarga las valoraciones desde el servicio.
    /// </summary>
    Task RefreshAsync(long productId);
    
    /// <summary>
    /// Añade una nueva valoración al estado.
    /// </summary>
    Task<Models.Rating?> AddRatingAsync(long userId, long productId, int puntuacion, string? comentario);
    
    /// <summary>
    /// Selector personalizado para observar una parte específica del estado.
    /// </summary>
    IObservable<T> Select<T>(Func<RatingState, T> selector);
}
