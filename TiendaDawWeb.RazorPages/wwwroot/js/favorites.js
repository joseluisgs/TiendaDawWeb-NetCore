/**
 * Alterna el estado de favorito de un producto
 * @param {number} productId - ID del producto
 */
async function toggleFavorite(productId) {
    console.log("🧡 Toggling favorite for product:", productId);
    
    try {
        // 1. Obtener el token Anti-Forgery (vital para POST en ASP.NET Core)
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenInput?.value;
        
        if (!token) {
            console.error("❌ Anti-Forgery token not found!");
        }

        // 2. Realizar la petición al API - Razor Pages usa ?handler=
        const response = await fetch('/Api/Favorites?handler=Toggle&productoId=' + productId, {
            method: 'GET'
        });

        if (response.status === 401) {
            showToast('Debes iniciar sesión para añadir favoritos', 'info');
            return;
        }

        const data = await response.json();

        if (data.success) {
            // 3. Actualizar TODOS los botones de este producto en la página
            const buttons = document.querySelectorAll(`.favorite-btn[data-product-id="${productId}"]`);
            
            buttons.forEach(button => {
                const icon = button.querySelector('i');
                const textNode = Array.from(button.childNodes).find(n => n.nodeType === Node.TEXT_NODE && n.textContent.trim().length > 0);

                if (data.isFavorite) {
                    // Estado Favorito (Lleno)
                    button.classList.remove('btn-outline-danger');
                    button.classList.add('btn-danger');
                    if (icon) {
                        icon.classList.remove('bi-heart');
                        icon.classList.add('bi-heart-fill');
                    }
                } else {
                    // Estado Normal (Vacío)
                    button.classList.remove('btn-danger');
                    button.classList.add('btn-outline-danger');
                    if (icon) {
                        icon.classList.remove('bi-heart-fill');
                        icon.classList.add('bi-heart');
                    }
                }
            });

            showToast(data.message, data.isFavorite ? 'success' : 'info');
        } else {
            showToast(data.message || 'Error al actualizar favoritos', 'error');
        }
    } catch (error) {
        console.error('❌ Error toggling favorite:', error);
        showToast('Error de conexión con el servidor', 'error');
    }
}

// Inicializar estado de favoritos al cargar la página (opcional pero recomendado)
document.addEventListener('DOMContentLoaded', async () => {
    const favoriteButtons = document.querySelectorAll('.favorite-btn');
    if (favoriteButtons.length === 0) return;

    console.log("🔍 Checking initial favorite states...");
    
    // Podríamos verificar el estado de cada uno, pero es más eficiente 
    // que el servidor renderice las clases correctas inicialmente.
    // Esta función se encarga de las transiciones AJAX.
});
