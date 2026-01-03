using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TiendaDawWeb.Models;
using TiendaDawWeb.Services.Interfaces;

namespace TiendaDawWeb.Services.Implementations
{
    /// <summary>
    /// OBJETIVO: Actuar como un bus de eventos y contenedor de estado para valoraciones.
    /// Optimiza la carga evitando peticiones duplicadas entre componentes.
    /// </summary>
    public class RatingStateContainer
    {
        private readonly IRatingService _ratingService;

        public RatingStateContainer(IRatingService ratingService)
        {
            _ratingService = ratingService;
        }

        public List<Rating>? Ratings { get; private set; }
        public long CurrentProductId { get; private set; }
        public double Average => Ratings != null && Ratings.Any() ? Ratings.Average(r => r.Puntuacion) : 0;
        public int Count => Ratings?.Count ?? 0;

        public event Action? OnChange;

        /// <summary>
        /// Asegura que los datos estén cargados para un producto específico.
        /// Si ya están en memoria, no vuelve a consultar la DB.
        /// </summary>
        public async Task EnsureLoadedAsync(long productId)
        {
            if (CurrentProductId == productId && Ratings != null) return;

            await RefreshAsync(productId);
        }

        /// <summary>
        /// Fuerza la recarga de datos desde el servicio.
        /// </summary>
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

        public void NotifyRatingChanged() => OnChange?.Invoke();
    }
}
