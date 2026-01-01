// Favorites AJAX functionality for WalaDaw

/**
 * Toggle favorite status for a product
 * @param {number} productId - The product ID
 */
async function toggleFavorite(productId) {
    try {
        const button = document.querySelector(`[data-product-id="${productId}"]`);
        if (!button) {
            console.error('Favorite button not found');
            return;
        }

        // Get CSRF token
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        
        const response = await fetch('/api/favorites/toggle', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token || ''
            },
            body: JSON.stringify({ productId: productId })
        });

        const data = await response.json();

        if (data.success) {
            // Update button appearance
            const icon = button.querySelector('i');
            if (data.isFavorite) {
                icon.classList.remove('bi-heart');
                icon.classList.add('bi-heart-fill');
                button.classList.remove('btn-outline-danger');
                button.classList.add('btn-danger');
            } else {
                icon.classList.remove('bi-heart-fill');
                icon.classList.add('bi-heart');
                button.classList.remove('btn-danger');
                button.classList.add('btn-outline-danger');
            }

            // Show toast notification
            showToast(data.message, 'success');
        } else {
            showToast(data.message || 'Error al actualizar favoritos', 'error');
        }
    } catch (error) {
        console.error('Error toggling favorite:', error);
        showToast('Error de conexión', 'error');
    }
}

/**
 * Check favorite status on page load and update buttons
 */
async function initializeFavorites() {
    const buttons = document.querySelectorAll('.favorite-btn[data-product-id]');
    
    for (const button of buttons) {
        const productId = button.dataset.productId;
        try {
            const response = await fetch(`/api/favorites/check/${productId}`);
            const data = await response.json();
            
            if (data.success && data.isFavorite) {
                const icon = button.querySelector('i');
                icon.classList.remove('bi-heart');
                icon.classList.add('bi-heart-fill');
                button.classList.remove('btn-outline-danger');
                button.classList.add('btn-danger');
            }
        } catch (error) {
            console.error(`Error checking favorite status for product ${productId}:`, error);
        }
    }
}

/**
 * Show toast notification
 * @param {string} message - The message to display
 * @param {string} type - Type of notification ('success', 'error', 'info')
 */
function showToast(message, type = 'info') {
    // Check if Bootstrap toasts are available
    const toastContainer = document.querySelector('.toast-container');
    
    if (toastContainer) {
        const toastEl = document.createElement('div');
        toastEl.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} border-0`;
        toastEl.setAttribute('role', 'alert');
        toastEl.setAttribute('aria-live', 'assertive');
        toastEl.setAttribute('aria-atomic', 'true');
        
        toastEl.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
            </div>
        `;
        
        toastContainer.appendChild(toastEl);
        const toast = new bootstrap.Toast(toastEl);
        toast.show();
        
        // Remove toast after it's hidden
        toastEl.addEventListener('hidden.bs.toast', () => {
            toastEl.remove();
        });
    } else {
        // Fallback to alert
        alert(message);
    }
}

// Initialize favorites on page load
document.addEventListener('DOMContentLoaded', function() {
    initializeFavorites();
});
