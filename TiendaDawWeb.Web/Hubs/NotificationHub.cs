using Microsoft.AspNetCore.SignalR;

namespace TiendaDawWeb.Web.Hubs;

/// <summary>
/// OBJETIVO: Servidor de mensajería en tiempo real.
/// UBICACIÓN: /Hubs
/// RAZÓN: Actúa como el punto central de conexión entre el servidor y todos los navegadores abiertos.
/// Permite enviar mensajes a usuarios específicos o a todo el mundo (Broadcast).
/// </summary>
public class NotificationHub : Hub
{
    // Este Hub puede estar vacío si solo enviamos mensajes del Server -> Client.
    // Pero podríamos añadir métodos aquí si los clientes quisieran enviarse mensajes entre sí.
}
