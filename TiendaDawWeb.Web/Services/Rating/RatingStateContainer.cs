using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Rating;

namespace TiendaDawWeb.Services.Rating;

/// <summary>
///     Contenedor de estado para gestión de valoraciones en tiempo real.
///     Actúa como bus de eventos para sincronización entre componentes Blazor.
/// </summary>
public class RatingStateContainer
{
    private readonly IRatingService _ratingService;

    /// <summary>
    ///     Inicializa una nueva instancia del contenedor de estado.
    /// </summary>
    /// <param name="ratingService">Servicio de valoraciones</param>
    public RatingStateContainer(IRatingService ratingService)
    {
        _ratingService = ratingService;
    }

    /// <summary>
    ///     Lista de valoraciones cargadas
    /// </summary>
    public List<Models.Rating>? Ratings { get; private set; }

    /// <summary>
    ///     ID del producto actualmente cargado
    /// </summary>
    public long CurrentProductId { get; private set; }

    /// <summary>
    ///     Promedio de puntuación de las valoraciones actuales
    /// </summary>
    public double Average => Ratings != null && Ratings.Any() ? Ratings.Average(r => r.Puntuacion) : 0;

    /// <summary>
    ///     Cantidad total de valoraciones
    /// </summary>
    public int Count => Ratings?.Count ?? 0;

    /// <summary>
    ///     Evento disparado cuando cambia el estado de las valoraciones
    /// </summary>
    public event Action? OnChange;

    /// <summary>
    ///     Asegura que las valoraciones estén cargadas para un producto específico.
    ///     Evita cargas redundantes si el producto ya está cargado.
    /// </summary>
    /// <param name="productId">ID del producto</param>
    public async Task EnsureLoadedAsync(long productId)
    {
        if (CurrentProductId == productId && Ratings != null) return;
        await RefreshAsync(productId);
    }

    /// <summary>
    ///     Recarga las valoraciones de un producto específico.
    /// </summary>
    /// <param name="productId">ID del producto</param>
    public async Task RefreshAsync(long productId)
    {
        CurrentProductId = productId;
        var result = await _ratingService.GetByProductoIdAsync(productId);
        if (result.IsSuccess)
        {
            Ratings = result.Value.ToList();
            NotifyRatingChanged();
        }
    }

    /// <summary>
    ///     Notifica a los componentessubscriptos que las valoraciones han cambiado.
    /// </summary>
    public void NotifyRatingChanged() => OnChange?.Invoke();
}
