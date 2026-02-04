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
            // Success - redirect to cart
            showToast('Producto añadido al carrito', 'success');
            setTimeout(() => {
                window.location.href = '/app/carrito';
            }, 500);
            
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
        console.log('Updating cart badge...');
        const response = await fetch('/Api/CartCount', {
            method: 'GET'
        });

        console.log('Response status:', response.status);
        
        if (response.ok) {
            const data = await response.json();
            console.log('Cart count:', data.count);
            const badge = document.querySelector('[data-testid="cart-count"]');
            
            if (badge) {
                badge.textContent = data.count;
                if (data.count > 0) {
                    badge.classList.remove('d-none');
                } else {
                    badge.classList.add('d-none');
                }
            }
        } else {
            console.error('Failed to fetch cart count:', response.status);
        }
    } catch (error) {
        console.error('Error updating cart badge:', error);
    }
}

// Initialize cart functionality on page load
document.addEventListener('DOMContentLoaded', function() {
    console.log('Cart functionality initialized');
});
