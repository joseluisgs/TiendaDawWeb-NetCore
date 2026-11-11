// Cliente SignalR para notificaciones globales en WalaDaw

const connection = new signalR.HubConnectionBuilder()
  .withUrl("/notificationHub")
  .withAutomaticReconnect()
  .build();

// Escuchar el evento 'ReceiveNotification' enviado desde el servidor
connection.on("ReceiveNotification", (title, message, productId) => {
  console.log("🔔 SignalR Data Received:", {title, message, productId});

  // Generar la URL relativa al raíz
  const detailUrl = productId ? `/Product/Details/${productId}` : null;

  if (typeof showToast === "function") {
    showToast(`${title}: ${message}`, 'info', detailUrl);
  }
});

// Iniciar la conexión
connection.start()
  .then(() => console.log("✅ Conectado al Hub de Notificaciones"))
  .catch(err => console.error("❌ Error al conectar a SignalR:", err));

