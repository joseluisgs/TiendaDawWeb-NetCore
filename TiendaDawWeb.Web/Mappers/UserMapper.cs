using TiendaDawWeb.Models;
using TiendaDawWeb.ViewModels;

namespace TiendaDawWeb.Web.Mappers;

/// <summary>
/// OBJETIVO: Centralizar la conversión de usuarios y perfiles.
/// </summary>
public static class UserMapper
{
    /// <summary>
    /// Mapea un RegisterViewModel a una entidad User de Identity.
    /// </summary>
    public static User ToEntity(this RegisterViewModel model)
    {
        return new User
        {
            UserName = model.Email,
            Email = model.Email,
            Nombre = model.Nombre,
            Apellidos = model.Apellidos,
            Avatar = model.Avatar ?? $"https://robohash.org/{model.Email}?size=150x150",
            Rol = "USER"
        };
    }
}
