using TiendaDawWeb.Models;
using TiendaDawWeb.ViewModels;

namespace TiendaDawWeb.Web.Mappers;

/// <summary>
/// OBJETIVO: Centralizar la conversión de valoraciones (Ratings).
/// </summary>
public static class RatingMapper
{
    public static RatingViewModel ToViewModel(this Rating rating)
    {
        return new RatingViewModel
        {
            Puntuacion = rating.Puntuacion,
            Comentario = rating.Comentario,
            ProductoId = rating.ProductoId
        };
    }

    public static Rating ToEntity(this RatingViewModel model, long usuarioId)
    {
        return new Rating
        {
            Puntuacion = model.Puntuacion,
            Comentario = model.Comentario,
            ProductoId = model.ProductoId,
            UsuarioId = usuarioId
        };
    }
}
