// Cart AJAX functionality for WalaDaw

/**
 * Add product to cart via AJAX
 * @param {number} productId - The product ID
 * @param {HTMLElement} button - The button element that triggered the action
 */
async function addToCart(productId, button = null) {
    try {
        // Disable button during request
        if (button) {
            button.disabled = true;
            button.innerHTML = '<span class="spinner-border spinner-border-sm" role="status"></span> Añadiendo...';
        }

        // Get CSRF token
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenInput?.value;
        
        if (!token) {
            console.warn('CSRF token not found, request may fail');
        }
        
        const response = await fetch('/Carrito/Add', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token || ''
            },
            body: `productoId=${productId}&__RequestVerificationToken=${encodeURIComponent(token || '')}`
        });

        if (response.redirected) {
            // Handle redirect (probably to login)
            window.location.href = response.url;
            return;
        }

        const text = await response.text();
        
        if (response.ok) {
            // Success - show notification and update cart badge
            showToast('Producto añadido al carrito', 'success');
            updateCartBadge();
            
            // Re-enable button
            if (button) {
                button.disabled = false;
                button.innerHTML = '<i class="bi bi-cart-plus"></i> Añadir al Carrito';
            }
        } else {
            // Error
            showToast('Error al añadir al carrito', 'error');
            if (button) {
                button.disabled = false;
                button.innerHTML = '<i class="bi bi-cart-plus"></i> Añadir al Carrito';
            }
        }
    } catch (error) {
        console.error('Error adding to cart:', error);
        showToast('Error de conexión', 'error');
        if (button) {
            button.disabled = false;
            button.innerHTML = '<i class="bi bi-cart-plus"></i> Añadir al Carrito';
        }
    }
}

/**
 * Remove item from cart via AJAX
 * @param {number} itemId - The cart item ID
 * @param {HTMLElement} element - The element to remove from DOM
 */
async function removeFromCart(itemId, element = null) {
    try {
        // Get CSRF token
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenInput?.value;
        
        if (!token) {
            console.warn('CSRF token not found, request may fail');
        }
        
        const response = await fetch('/app/carrito/remove', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/x-www-form-urlencoded',
                'RequestVerificationToken': token || ''
            },
            body: `itemId=${itemId}&__RequestVerificationToken=${encodeURIComponent(token || '')}`
        });

        if (response.ok) {
            // Success - remove from DOM and update cart badge
            if (element) {
                element.remove();
            }
            showToast('Producto eliminado del carrito', 'success');
            updateCartBadge();
            
            // Reload page if cart is now empty
            const remainingItems = document.querySelectorAll('.cart-item, .carrito-item');
            if (remainingItems.length === 0) {
                location.reload();
            }
        } else {
            showToast('Error al eliminar del carrito', 'error');
        }
    } catch (error) {
        console.error('Error removing from cart:', error);
        showToast('Error de conexión', 'error');
    }
}

/**
 * Update cart badge with current count
 */
async function updateCartBadge() {
    try {
        // The badge updates on page reload or via server-side rendering
        // For SPA-like behavior, we could add an API endpoint here
        // For now, we'll just assume server-side updates
        console.log('Cart updated - badge will update on next page load');
    } catch (error) {
        console.error('Error updating cart badge:', error);
    }
}

// Initialize cart functionality on page load
document.addEventListener('DOMContentLoaded', function() {
    console.log('Cart functionality initialized');
});
