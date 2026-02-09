using System.Reactive.Linq;
using System.Reactive.Subjects;
using TiendaDawWeb.Shared.Models;

namespace TiendaDawWeb.Shared.Services.Rating;

/// <summary>
/// Implementación del almacén de valoraciones usando BehaviorSubject.
/// </summary>
public class RatingStore : IRatingStore
{
    // ═══════════════════════════════════════════════════════════════
    // STATE
    // ═══════════════════════════════════════════════════════════════
    
    /// <summary>
    /// BehaviorSubject que almacena el estado de las valoraciones.
    /// </summary>
    private readonly BehaviorSubject<RatingState> _state;
    
    // ═══════════════════════════════════════════════════════════════
    // GETTERS
    // ═══════════════════════════════════════════════════════════════
    
    /// <inheritdoc />
    public IObservable<RatingState> State => _state.AsObservable();

    /// <inheritdoc />
    public IObservable<List<Models.Rating>> Ratings => 
        _state.Select(s => s.Ratings).DistinctUntilChanged();

    /// <inheritdoc />
    public IObservable<double> Average => 
        _state.Select(s => s.Average).DistinctUntilChanged();

    /// <inheritdoc />
    public IObservable<int> Count => 
        _state.Select(s => s.Count).DistinctUntilChanged();

    /// <inheritdoc />
    public IObservable<bool> HasRatings => 
        _state.Select(s => s.HasRatings).DistinctUntilChanged();

    /// <summary>
    /// Constructor que inicializa el estado vacío.
    /// </summary>
    /// <param name="ratingService">Servicio de valoraciones.</param>
    public RatingStore(IRatingService ratingService)
    {
        _ratingService = ratingService;
        _state = new BehaviorSubject<RatingState>(new RatingState());
    }
    
    // ═══════════════════════════════════════════════════════════════
    // ACTIONS
    // ═══════════════════════════════════════════════════════════════
    
    /// <inheritdoc />
    public RatingState GetState() => _state.Value;

    /// <inheritdoc />
    public Task EnsureLoadedAsync(long productId)
    {
        if (_state.Value.CurrentProductId == productId && _state.Value.Ratings.Any())
            return Task.CompletedTask;
        return RefreshAsync(productId);
    }
    
    /// <inheritdoc />
    public Task RefreshAsync(long productId)
    {
        return Task.Run(async () =>
        {
            var result = await _ratingService.GetByProductoIdAsync(productId);
            if (result.IsSuccess)
                _state.OnNext(new RatingState(result.Value.ToList(), productId));
        });
    }
    
    /// <inheritdoc />
    public async Task<Models.Rating?> AddRatingAsync(
        long userId, 
        long productId, 
        int puntuacion, 
        string? comentario)
    {
        var result = await _ratingService.AddRatingAsync(userId, productId, puntuacion, comentario);
        if (result.IsSuccess && result.Value != null)
        {
            var ratings = _state.Value.Ratings.ToList();
            ratings.Add(result.Value);
            _state.OnNext(new RatingState(ratings, productId));
        }
        return result.Value;
    }
    
    /// <inheritdoc />
    public IObservable<T> Select<T>(Func<RatingState, T> selector) 
        => _state.Select(selector).DistinctUntilChanged();

    private readonly IRatingService _ratingService;
}
