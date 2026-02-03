// Funciones JavaScript para WalaDaw

// Auto-ocultar alertas después de 5 segundos
document.addEventListener('DOMContentLoaded', function() {
    // Auto-dismiss alerts
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            const bsAlert = new bootstrap.Alert(alert);
            bsAlert.close();
        }, 5000);
    });

    // Confirmación de eliminación
    const deleteButtons = document.querySelectorAll('[data-confirm-delete]');
    deleteButtons.forEach(button => {
        button.addEventListener('click', function(e) {
            if (!confirm('¿Estás seguro de que quieres eliminar este elemento?')) {
                e.preventDefault();
            }
        });
    });

    // Preview de imagen antes de subir
    const imageInputs = document.querySelectorAll('input[type="file"][accept*="image"]');
    imageInputs.forEach(input => {
        input.addEventListener('change', function(e) {
            const file = e.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function(e) {
                    const preview = document.getElementById('image-preview');
                    if (preview) {
                        preview.src = e.target.result;
                        preview.style.display = 'block';
                    }
                };
                reader.readAsDataURL(file);
            }
        });
    });

    // Normalizar inputs de precio antes de submit
    const forms = document.querySelectorAll('form');
    
    forms.forEach(form => {
        const priceInputs = form.querySelectorAll('input[name="Precio"]');
        if (priceInputs.length > 0) {
            form.addEventListener('submit', function(e) {
                // Reemplazar todas las comas por puntos antes de enviar
                priceInputs.forEach(input => {
                    input.value = input.value.replace(/,/g, '.');
                });
            });
        }
    });
});

// Función para formatear precio
function formatPrice(price) {
    return new Intl.NumberFormat('es-ES', {
        style: 'currency',
        currency: 'EUR'
    }).format(price);
}

/**
 * Muestra una notificación Toast de Bootstrap
 * @param {string} message - El mensaje a mostrar
 * @param {string} type - 'success', 'error', 'info', 'warning'
 * @param {string} url - URL opcional para el botón de acción
 */
function showToast(message, type = 'info', url = null) {
    let toastContainer = document.querySelector('.toast-container');
    
    // Si no existe, lo creamos
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
        toastContainer.style.zIndex = '11050';
        document.body.appendChild(toastContainer);
    }

    // Crear el elemento base del toast
    const toastEl = document.createElement('div');
    const bgClass = type === 'error' ? 'danger' : (type === 'success' ? 'success' : (type === 'warning' ? 'warning' : 'info'));
    const textClass = type === 'warning' ? 'text-dark' : 'text-white';
    
    toastEl.className = `toast align-items-center ${textClass} bg-${bgClass} border-0 shadow-lg`;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');

    // Estructura interna
    let toastHtml = `
        <div class="d-flex">
            <div class="toast-body">
                <div class="fw-bold">${message}</div>
    `;

    if (url) {
        toastHtml += `
            <div class="mt-2">
                <button type="button" class="btn btn-sm btn-light fw-bold text-dark shadow-sm btn-toast-action" data-url="${url}">
                    <i class="bi bi-eye"></i> VER PRODUCTO
                </button>
            </div>
        `;
    }

    toastHtml += `
            </div>
            <button type="button" class="btn-close ${type === 'warning' ? '' : 'btn-close-white'} me-2 m-auto" data-bs-dismiss="toast"></button>
        </div>
    `;

    toastEl.innerHTML = toastHtml;
    toastContainer.appendChild(toastEl);

    // Manejar clic en el botón de acción si existe
    const actionBtn = toastEl.querySelector('.btn-toast-action');
    if (actionBtn) {
        actionBtn.addEventListener('click', (e) => {
            const targetUrl = e.target.closest('button').dataset.url;
            if (targetUrl) window.location.href = targetUrl;
        });
    }

    // Inicializar y mostrar con Bootstrap
    const toast = new bootstrap.Toast(toastEl, { 
        autohide: true, 
        delay: 5000 
    });
    toast.show();

    // Limpiar el DOM al ocultarse
    toastEl.addEventListener('hidden.bs.toast', () => {
        toastEl.remove();
    });
}
