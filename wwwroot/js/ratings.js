// Ratings AJAX functionality for WalaDaw

/**
 * Submit a rating for a product
 * @param {number} productId - The product ID
 * @param {number} rating - The rating value (1-5)
 * @param {string} comentario - Optional comment
 */
async function submitRating(productId, rating, comentario = '') {
    try {
        // Get CSRF token
        const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
        const token = tokenInput?.value;
        
        if (!token) {
            console.warn('CSRF token not found, request may fail');
        }
        
        const response = await fetch('/api/ratings', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token || ''
            },
            body: JSON.stringify({
                productId: productId,
                puntuacion: rating,
                comentario: comentario
            })
        });

        const data = await response.json();

        if (data.success) {
            showToast('Valoración enviada correctamente', 'success');
            // Reload to show new rating
            setTimeout(() => location.reload(), 1000);
        } else {
            showToast(data.message || 'Error al enviar valoración', 'error');
        }
    } catch (error) {
        console.error('Error submitting rating:', error);
        showToast('Error de conexión', 'error');
    }
}

/**
 * Check if user has already rated a product
 * @param {number} productId - The product ID
 * @returns {Promise<boolean>} - True if user has rated
 */
async function checkUserRating(productId) {
    try {
        const response = await fetch(`/api/ratings/check/${productId}`);
        const data = await response.json();
        return data.hasRated || false;
    } catch (error) {
        console.error('Error checking user rating:', error);
        return false;
    }
}

/**
 * Initialize star rating UI
 * @param {HTMLElement} container - Container element for stars
 * @param {Function} onRate - Callback when rating is selected
 */
function initializeStarRating(container, onRate) {
    if (!container) return;
    
    const stars = container.querySelectorAll('.rating-star');
    let selectedRating = 0;
    
    stars.forEach((star, index) => {
        const rating = index + 1;
        
        // Hover effect
        star.addEventListener('mouseenter', () => {
            highlightStars(stars, rating);
        });
        
        // Click to select
        star.addEventListener('click', () => {
            selectedRating = rating;
            highlightStars(stars, rating);
            if (onRate) {
                onRate(rating);
            }
        });
    });
    
    // Reset on mouse leave
    container.addEventListener('mouseleave', () => {
        highlightStars(stars, selectedRating);
    });
}

/**
 * Highlight stars up to the given rating
 * @param {NodeList} stars - Star elements
 * @param {number} rating - Rating value (1-5)
 */
function highlightStars(stars, rating) {
    stars.forEach((star, index) => {
        if (index < rating) {
            star.classList.remove('bi-star');
            star.classList.add('bi-star-fill');
        } else {
            star.classList.remove('bi-star-fill');
            star.classList.add('bi-star');
        }
    });
}

/**
 * Create star rating display (read-only)
 * @param {number} rating - Average rating
 * @param {number} count - Number of ratings
 * @returns {string} HTML string
 */
function createStarDisplay(rating, count = 0) {
    const fullStars = Math.floor(rating);
    const hasHalfStar = rating % 1 >= 0.5;
    const emptyStars = 5 - fullStars - (hasHalfStar ? 1 : 0);
    
    let html = '';
    
    // Full stars
    for (let i = 0; i < fullStars; i++) {
        html += '<i class="bi bi-star-fill text-warning"></i>';
    }
    
    // Half star
    if (hasHalfStar) {
        html += '<i class="bi bi-star-half text-warning"></i>';
    }
    
    // Empty stars
    for (let i = 0; i < emptyStars; i++) {
        html += '<i class="bi bi-star text-warning"></i>';
    }
    
    // Rating text
    html += ` <span class="ms-2">${rating.toFixed(1)}</span>`;
    
    // Count
    if (count > 0) {
        html += ` <span class="text-muted">(${count})</span>`;
    }
    
    return html;
}

/**
 * Show toast notification
 * @param {string} message - The message to display
 * @param {string} type - Type of notification ('success', 'error', 'info')
 */
function showToast(message, type = 'info') {
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

// Initialize rating functionality on page load
document.addEventListener('DOMContentLoaded', function() {
    // Initialize star rating inputs
    const ratingContainers = document.querySelectorAll('[data-rating-input]');
    ratingContainers.forEach(container => {
        const productId = container.dataset.productId;
        let selectedRating = 0;
        
        initializeStarRating(container, (rating) => {
            selectedRating = rating;
            // Update hidden input if exists
            const hiddenInput = container.querySelector('input[name="rating"]');
            if (hiddenInput) {
                hiddenInput.value = rating;
            }
        });
        
        // Handle form submission
        const form = container.closest('form');
        if (form) {
            form.addEventListener('submit', async (e) => {
                e.preventDefault();
                const comentario = form.querySelector('textarea[name="comentario"]')?.value || '';
                await submitRating(productId, selectedRating, comentario);
            });
        }
    });
    
    console.log('Ratings functionality initialized');
});
