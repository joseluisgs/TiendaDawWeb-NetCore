/**
 * Muestra una notificación Toast de Bootstrap
 * @param {string} message - El mensaje a mostrar
 * @param {string} type - 'success', 'error', 'info'
 * @param {string} url - URL opcional para el botón de acción
 */
function showToast(message, type = 'info', url = null) {
    const toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) return;

    // Crear el elemento base
    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center text-white bg-${type === 'error' ? 'danger' : type === 'success' ? 'success' : 'info'} border-0 shadow-lg`;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');
    toastEl.style.cursor = 'default';

    // Contenido interno con Flexbox
    const dFlex = document.createElement('div');
    dFlex.className = 'd-flex';

    const toastBody = document.createElement('div');
    toastBody.className = 'toast-body';
    
    // Mensaje
    const messageDiv = document.createElement('div');
    messageDiv.className = 'fw-bold';
    messageDiv.innerText = message;
    toastBody.appendChild(messageDiv);

    // Botón de acción (si hay URL)
    if (url) {
        const actionDiv = document.createElement('div');
        actionDiv.className = 'mt-2';
        
        const btn = document.createElement('button');
        btn.className = 'btn btn-sm btn-light fw-bold text-dark shadow-sm';
        btn.innerHTML = '<i class="bi bi-eye"></i> VER PRODUCTO';
        btn.type = 'button';
        
        // 🚨 NAVEGACIÓN DIRECTA: Usamos el evento de clic de JS puro
        btn.addEventListener('click', (e) => {
            e.preventDefault();
            e.stopPropagation(); // Evitar que el Toast intercepte el clic
            console.log("🚀 Redirigiendo a:", url);
            window.location.href = url;
        });
        
        actionDiv.appendChild(btn);
        toastBody.appendChild(actionDiv);
    }

    dFlex.appendChild(toastBody);

    // Botón de cerrar
    const closeBtn = document.createElement('button');
    closeBtn.type = 'button';
    closeBtn.className = 'btn-close btn-close-white me-2 m-auto';
    closeBtn.setAttribute('data-bs-dismiss', 'toast');
    dFlex.appendChild(closeBtn);

    toastEl.appendChild(dFlex);
    toastContainer.appendChild(toastEl);

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