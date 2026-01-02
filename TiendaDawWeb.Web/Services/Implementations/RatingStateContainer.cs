using System;

namespace TiendaDawWeb.Services.Implementations
{
    /// <summary>
    /// OBJETIVO: Actuar como un bus de eventos (State Container) para la comunicación entre componentes Blazor.
    /// UBICACIÓN: Services/Implementations/
    /// RAZÓN: Aunque no tiene interfaz, es un servicio de infraestructura que mantiene el estado dinámico 
    /// en la sesión del usuario (Scoped), permitiendo que componentes desacoplados se enteren de cambios mutuos.
    /// </summary>
    public class RatingStateContainer
    {
        public event Action? OnChange;
        
        public void NotifyRatingChanged() => OnChange?.Invoke();
    }
}
