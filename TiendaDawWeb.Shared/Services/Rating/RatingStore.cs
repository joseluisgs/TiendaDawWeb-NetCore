using System.Reactive.Linq;
using System.Reactive.Subjects;
using Microsoft.Extensions.DependencyInjection;
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
    /// 
    /// NOTA: RatingStore es un Singleton, pero IRatingService es Scoped.
    /// Para resolver un servicio Scoped desde un Singleton, necesitamos IServiceScopeFactory.
    /// Un "Scope" crea un contenedor temporal de dependencias con instancias frescas.
    /// El scope se dispose al final del método, liberando los recursos correctamente.
    /// </summary>
    /// <param name="serviceScopeFactory">Fábrica de alcances para resolver servicios scoped.</param>
    public RatingStore(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
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
    
    /// <summary>
    /// Refresca las valoraciones de un producto.
    /// 
    /// PATRÓN: Creamos un scope temporal con "using" para resolver IRatingService.
    /// Al salir del bloque "using", el scope se dispose y libera los recursos.
    /// Esto permite que un Singleton (RatingStore) use servicios Scoped (IRatingService).
    /// </summary>
    /// <inheritdoc />
    public Task RefreshAsync(long productId)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var ratingService = scope.ServiceProvider.GetRequiredService<IRatingService>();
            var result = await ratingService.GetByProductoIdAsync(productId);
            if (result.IsSuccess)
                _state.OnNext(new RatingState(result.Value.ToList(), productId));
        });
    }
    
    /// <summary>
    /// Añade una nueva valoración.
    /// 
    /// Cada llamada crea su propio scope para obtener una instancia fresca de IRatingService.
    /// </summary>
    /// <inheritdoc />
    public async Task<Models.Rating?> AddRatingAsync(
        long userId, 
        long productId, 
        int puntuacion, 
        string? comentario)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var ratingService = scope.ServiceProvider.GetRequiredService<IRatingService>();
        var result = await ratingService.AddRatingAsync(userId, productId, puntuacion, comentario);
        if (result.IsSuccess && result.Value != null)
        {
            var ratings = _state.Value.Ratings.ToList();
            ratings.Add(result.Value);
            _state.OnNext(new RatingState(ratings, productId));
        }
        return result.Value;
    }

    /// <summary>
    /// Verifica si un usuario puede valorar un producto.
    /// </summary>
    public Task<bool> CanUserRateAsync(long userId, long productId)
    {
        return Task.Run(async () =>
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var ratingService = scope.ServiceProvider.GetRequiredService<IRatingService>();
            var result = await ratingService.CanUserRateProductAsync(userId, productId);
            return result.IsSuccess && result.Value;
        });
    }
    
    /// <inheritdoc />
    public IObservable<T> Select<T>(Func<RatingState, T> selector) 
        => _state.Select(selector).DistinctUntilChanged();

    private readonly IServiceScopeFactory _serviceScopeFactory;
}
