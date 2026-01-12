using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Rating;

namespace TiendaDawWeb.Services.Rating;

/// <summary>
/// OBJETIVO: Actuar como un bus de eventos y contenedor de estado para valoraciones.
/// Optimiza la carga evitando peticiones duplicadas entre componentes.
/// </summary>
public class RatingStateContainer {
    private readonly IRatingService _ratingService;

    public RatingStateContainer(IRatingService ratingService) {
        _ratingService = ratingService;
    }

    public List<Models.Rating>? Ratings { get; private set; }
    public long CurrentProductId { get; private set; }
    public double Average => Ratings != null && Ratings.Any() ? Ratings.Average(r => r.Puntuacion) : 0;
    public int Count => Ratings?.Count ?? 0;

    public event Action? OnChange;

    public async Task EnsureLoadedAsync(long productId) {
        if (CurrentProductId == productId && Ratings != null) return;
        await RefreshAsync(productId);
    }

    public async Task RefreshAsync(long productId) {
        CurrentProductId = productId;
        var result = await _ratingService.GetByProductoIdAsync(productId);
        if (result.IsSuccess) {
            Ratings = result.Value.ToList();
            NotifyRatingChanged();
        }
    }

    public void NotifyRatingChanged() => OnChange?.Invoke();
}
