# 07. JavaScript & AJAX: La Danza Asíncrona con Seguridad

En la web moderna, no podemos recargar la página cada vez que un usuario hace clic. **AJAX** (Asynchronous JavaScript and XML, aunque hoy usamos JSON) nos permite interactuar con el servidor sin interrupciones.

## 1. El Ciclo de Vida de una Petición AJAX (Usando Fetch API)

Cuando un usuario pulsa el botón de "Favorito" o deja una valoración:

1.  **Evento en el Cliente**: JavaScript detecta el clic en el botón.
2.  **Preparar Petición**: Se construye un objeto JavaScript con los datos a enviar (ej. `productId`).
3.  **Disparar `fetch()`**: JavaScript envía una petición HTTP al servidor en segundo plano.
    *   **Método**: `POST` (para crear/actualizar), `GET` (para leer), `DELETE` (para borrar).
    *   **Cabeceras (`Headers`)**: Información extra (ej. tipo de contenido, token de seguridad).
    *   **Cuerpo (`body`)**: Los datos a enviar (ej. `JSON.stringify({ productId: 5 })`).
4.  **Servidor Responde**: El `ApiController` de .NET procesa la petición y devuelve una respuesta, típicamente en formato JSON.
    ```json
    { "success": true, "isFavorite": true, "message": "Producto añadido a favoritos" }
    ```
5.  **Procesar Respuesta en Cliente**: JavaScript recibe el JSON, lo analiza y actualiza el DOM (la estructura HTML de la página) para reflejar los cambios.

### 1.1. Ejemplo Práctico: `favorites.js` (Toggle de Favoritos)

```javascript
// TiendaDawWeb.Web/wwwroot/js/favorites.js
async function toggleFavorite(productId) {
    try {
        const button = document.querySelector(`[data-product-id="${productId}"]`);
        // ... obtener token CSRF ...
        
        const response = await fetch('/api/favorites/toggle', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify({ productId: productId })
        });

        const data = await response.json(); // Convierte la respuesta JSON a objeto JS

        if (data.success) {
            // Actualizar la interfaz de usuario: Cambiar el icono del corazón y el color del botón
            // ... (lógica de manipulación del DOM) ...
            showToast(data.message, 'success'); // Muestra una notificación
        } else {
            showToast(data.message || 'Error al actualizar favoritos', 'error');
        }
    } catch (error) {
        console.error('Error toggling favorite:', error);
        showToast('Error de conexión', 'error');
    }
}
```

---

## 2. Protección CSRF (Anti-Falsificación de Peticiones): Un Escudo Esencial

CSRF (Cross-Site Request Forgery) es un tipo de ataque donde un sitio web malicioso engaña a tu navegador para que envíe una petición no deseada a una web en la que estás autenticado.

### 2.1. El Problema con AJAX:
Los formularios tradicionales de ASP.NET Core MVC incluyen automáticamente un campo oculto con un token Anti-Forgery. Si usas `fetch()` o `XMLHttpRequest` directamente, este token no se envía automáticamente.
-   **Consecuencia**: El servidor de .NET detecta una petición sin token y devuelve un error **403 Forbidden** por seguridad.

### 2.2. La Solución (Implementada en `favorites.js` y `ratings.js`):
Debes extraer el token del DOM y añadirlo manualmente a las cabeceras de tu petición `fetch()`:

```javascript
// 1. En tu _Layout.cshtml, ASP.NET Core genera este input oculto automáticamente:
// <input name="__RequestVerificationToken" type="hidden" value="[EL_TOKEN_GENERADO]" />

// 2. En tu JavaScript, lo lees del DOM:
const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
const token = tokenInput?.value; // Obtenemos el valor del token

// 3. Lo envías en la cabecera de la petición AJAX:
const response = await fetch('/api/favorites/toggle', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/json',
        'RequestVerificationToken': token // ¡ESTA ES LA CABECERA MÁGICA!
    },
    body: JSON.stringify({ productId: productId })
});
```
**Lección de Supervivencia**: Si tu `fetch('POST', ...)` da un 403 Forbidden y no hay un `[Authorize]` en el controlador, la causa más probable es el token Anti-Forgery.

---

## 3. Blindaje de Scripts: Cláusulas de Guarda (Robustez del Código)

Muchos errores de JavaScript ocurren porque el script intenta interactuar con elementos HTML que no existen en la página actual.

### 3.1. El Problema:
Tu archivo `ratings.js` se incluye en el `_Layout.cshtml`, lo que significa que se ejecuta en **todas** las páginas de tu web. Si entra en la página de inicio (`/Home/Index`), intentará buscar un `div` con `id="ratingSectionAJAX"` (que solo existe en la página de detalles del producto) y fallará con un `TypeError: Cannot set properties of null`.

### 3.2. La Solución Senior: Cláusulas de Guarda
Al principio de tus funciones JavaScript, añade una comprobación para ver si los elementos necesarios existen:

```javascript
// TiendaDawWeb.Web/wwwroot/js/ratings.js
document.addEventListener('DOMContentLoaded', function () {
    // Si no estamos en una página con valoraciones (no existe el contenedor), salimos.
    const ratingContainer = document.getElementById("ratingSectionAJAX");
    if (!ratingContainer) {
        return; // Detiene la ejecución de este script en esta página
    }

    // A partir de aquí, sabemos que estamos en la página correcta.
    // ... el resto de tu lógica ...
    loadRatings();
    loadRatingForm();
});

async function loadRatings() {
    // También se puede poner una guarda interna si la función es llamada de forma independiente
    const ratingsListDiv = document.getElementById("ratingsListAJAX");
    if (!ratingsListDiv) {
        console.warn("Elemento #ratingsListAJAX no encontrado, saltando carga de ratings.");
        return;
    }
    // ... lógica de carga de ratings ...
}
```
**Lección de Supervivencia**: Implementar cláusulas de guarda te ayuda a crear scripts JavaScript robustos que no colapsan la consola del navegador y se comportan correctamente en cualquier página donde sean incluidos.

---

## 4. Manipulación del DOM: Sincronización masiva de elementos

Cuando un usuario interactúa con un elemento (ej. un botón de favorito), el servidor le dice el nuevo estado. JavaScript debe reflejar ese cambio en la interfaz de usuario.

### 4.1. El Reto: Múltiples Elementos
¿Qué ocurre si el mismo producto aparece en la vista de detalles y en un listado de "productos relacionados"? Si el usuario lo marca como favorito en el listado, el botón en el detalle debe actualizarse también.

### 4.2. La Solución: `document.querySelectorAll` y Bucle `forEach`
En lugar de `document.querySelector` (que solo devuelve el primer elemento), usamos `document.querySelectorAll` para obtener todos los elementos que coincidan y luego los actualizamos uno por uno:

```javascript
// TiendaDawWeb.Web/wwwroot/js/favorites.js
// ... después de recibir 'data.isFavorite' del servidor ...
const buttons = document.querySelectorAll(`.favorite-btn[data-product-id="${productId}"]`);
buttons.forEach(button => {
    const icon = button.querySelector('i');
    if (data.isFavorite) {
        // Añadir clases de "favorito"
        button.classList.remove('btn-outline-danger');
        button.classList.add('btn-danger');
        icon?.classList.replace('bi-heart', 'bi-heart-fill');
    } else {
        // Quitar clases de "favorito"
        button.classList.remove('btn-danger');
        button.classList.add('btn-outline-danger');
        icon?.classList.replace('bi-heart-fill', 'bi-heart');
    }
});
```
**Lección de Supervivencia**: Las interacciones de usuario en una SPA deben ser coherentes en toda la página. Anticipa dónde se pueden ver múltiples representaciones de un mismo estado.

---

## 5. Feedback Visual: Toasts de Bootstrap (La Elegancia del Feedback)

Las alertas `alert()` son toscas y rompen la experiencia de usuario. Usamos los componentes **Toast de Bootstrap 5** para notificaciones elegantes y no intrusivas.

### 5.1. Creación Dinámica de Toasts
En lugar de tener los Toasts escondidos en el HTML, los creamos y destruimos con JavaScript:

```javascript
// TiendaDawWeb.Web/wwwroot/js/favorites.js (Función showToast)
function showToast(message, type = 'info') {
    const toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        console.error('Toast container not found!');
        return;
    }

    const toastEl = document.createElement('div'); // Crea un nuevo div
    toastEl.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} border-0`;
    // ... (configura atributos y HTML interno del toast) ...

    toastContainer.appendChild(toastEl); // Lo añade al DOM
    const toast = new bootstrap.Toast(toastEl); // Activa el componente Bootstrap
    toast.show(); // Lo muestra

    // Lo elimina del DOM una vez que se ha ocultado
    toastEl.addEventListener('hidden.bs.toast', () => { toastEl.remove(); });
}
```
**Lección de Supervivencia**: Crear elementos de UI dinámicamente con JavaScript para el feedback mejora drásticamente la experiencia de usuario. Mantén tu HTML lo más limpio posible de elementos ocultos.

---

Este volumen te ha guiado por las profundidades del JavaScript moderno y las técnicas de AJAX para construir una interfaz de usuario fluida, reactiva y segura.