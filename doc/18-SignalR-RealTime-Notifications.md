# 18 - Interactividad Pro: Notificaciones en Tiempo Real con SignalR

En este volumen aprendemos a romper el modelo tradicional de "Petición-Respuesta" para permitir que el servidor envíe información al cliente sin que este la solicite (**Push Notifications**).

---

## 1. ¿Qué es SignalR?

SignalR es una librería de Microsoft que abstrae el uso de **WebSockets**. Permite una comunicación bidireccional permanente entre el servidor y todos los navegadores conectados.

---

## 2. Componentes de la Solución

### A. El Servidor (The Hub)
Hemos creado un `NotificationHub.cs`. Este es el centro de mando. Cuando el servidor quiere avisar de algo, le envía el mensaje al Hub, y el Hub lo reparte a los clientes.

### B. El Mensajero (`IHubContext`)
Para enviar mensajes desde fuera del Hub (por ejemplo, desde un Controlador), usamos la inyección de dependencias para obtener un `IHubContext`.

### C. El Receptor (JavaScript)
Hemos creado `notifications.js`, que:
1.  Se conecta al Hub al cargar la página.
2.  Queda a la escucha de un evento llamado `ReceiveNotification`.
3.  Cuando llega el evento, muestra un **Toast de Bootstrap** dinámicamente.

---

## 3. Caso de Uso: Broadcast de Nuevo Producto

Cada vez que un usuario publica un producto en `ProductController.Create`:
1.  El servidor procesa el guardado.
2.  Dispara un mensaje masivo a todos los usuarios: `"¡Nuevo Producto!: [Nombre]"`.
3.  Incluso si otro usuario está en otra página, verá aparecer el aviso en su pantalla al instante.

---

## 4. Diferencia con Blazor (Senior Tip)

Aunque Blazor Server usa SignalR internamente para sincronizar el DOM, usar **SignalR puro** nos da un control total para tareas transversales (como notificaciones) que afectan a toda la web, no solo a un componente específico. Es la herramienta ideal para alertas, chats o sistemas de monitorización.

---

## 5. Conclusión para el Alumno

Añadir tiempo real a una aplicación web la eleva a una categoría superior de experiencia de usuario. SignalR hace que WalaDaw se sienta como una aplicación moderna, reactiva y "viva".
