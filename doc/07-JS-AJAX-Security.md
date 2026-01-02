# 07. JavaScript & AJAX: La Web sin Esperas

Blazor es potente para el Admin, pero para la web pública usamos Vanilla JS por rendimiento y agilidad.

## 1. El ciclo de vida AJAX
1. Usuario pulsa el botón de "Favorito".
2. JavaScript dispara un `fetch()` asíncrono.
3. El servidor responde con un JSON limpio.
4. El JS actualiza el icono del corazón en el navegador sin que la página parpadee.

## 2. Protección contra ataques CSRF
.NET bloquea por defecto cualquier `POST` que no sea de un formulario oficial.
**La Solución para AJAX**:
Extraemos el Token de seguridad del HTML y lo enviamos en la cabecera:
```javascript
headers: { 'RequestVerificationToken': token }
```
**Tip de Supervivencia**: Sin esta línea, recibirás siempre un error 403 Forbidden.

## 3. Sincronización del DOM
¿Qué pasa si tienes el botón de Favorito en el detalle del producto y también en una lista lateral?
Si usas `document.querySelector`, solo se actualizará uno.
**La Solución Pro**: Usa `document.querySelectorAll` y recorre todos los botones del mismo producto con un bucle para que la web sea coherente visualmente.

## 4. Toasts Dinámicos (Bootstrap 5)
Usamos notificaciones tipo "Toast". Son trozos de HTML creados por código JS que se destruyen solos tras unos segundos. Es la forma profesional de confirmar una acción sin molestar al usuario.
