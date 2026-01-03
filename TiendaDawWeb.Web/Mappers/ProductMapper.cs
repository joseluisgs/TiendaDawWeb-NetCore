using TiendaDawWeb.Models;
using TiendaDawWeb.ViewModels;

namespace TiendaDawWeb.Web.Mappers;

/// <summary>
/// OBJETIVO: Centralizar la lógica de conversión entre Modelos de Dominio y ViewModels.
/// UBICACIÓN: /Mappers
/// RAZÓN: Aplica el principio DRY (Don't Repeat Yourself). Evita que los controladores 
/// tengan código repetitivo de asignación de propiedades, facilitando el mantenimiento.
/// </summary>
public static class ProductMapper
{
    /// <summary>
    /// Convierte un Producto (Base de Datos) a un ProductViewModel (Vista).
    /// </summary>
    public static ProductViewModel ToViewModel(this Product product)
    {
        return new ProductViewModel
        {
            Id = product.Id,
            Nombre = product.Nombre,
            Descripcion = product.Descripcion,
            Precio = product.Precio,
            Categoria = product.Categoria,
            ImagenUrl = product.Imagen
        };
    }

    /// <summary>
    /// Convierte un ProductViewModel (Vista) a un Producto (Base de Datos).
    /// </summary>
    /// <param name="model">El viewmodel de origen.</param>
    /// <param name="propietarioId">ID del usuario que realiza la acción.</param>
    /// <param name="imagenUrl">URL de la imagen ya procesada por el storage.</param>
    public static Product ToEntity(this ProductViewModel model, long propietarioId, string? imagenUrl)
    {
        return new Product
        {
            Id = model.Id,
            Nombre = model.Nombre,
            Descripcion = model.Descripcion,
            Precio = model.Precio,
            Categoria = model.Categoria,
            PropietarioId = propietarioId,
            Imagen = imagenUrl ?? model.ImagenUrl
        };
    }
    
    /// <summary>
    /// Actualiza una entidad existente con los datos del viewmodel.
    /// Útil para no crear objetos nuevos en memoria si no es necesario.
    /// </summary>
    public static void UpdateEntity(this ProductViewModel model, Product product, string? imagenUrl)
    {
        product.Nombre = model.Nombre;
        product.Descripcion = model.Descripcion;
        product.Precio = model.Precio;
        product.Categoria = model.Categoria;
        if (imagenUrl != null) product.Imagen = imagenUrl;
    }
}
