// AJAX Ratings para WalaDaw Product Detail

// Variables globales
document.addEventListener('DOMContentLoaded', function () {
    // Si no estamos en una página con valoraciones (no existe el contenedor), no hacemos nada
    if (!document.getElementById("ratingSectionAJAX")) {
        return;
    }

    // Reemplaza por el valor razor generado
    const isAuthenticated = window.isAuthenticated !== undefined ? window.isAuthenticated : (document.body.dataset.isauthenticated === "true");
    const isOwner = window.isOwner !== undefined ? window.isOwner : false;
    const productId = window.productId !== undefined ? window.productId : (document.body.dataset.productid || null);

    loadRatings();
    loadRatingForm();

    // ----------- FUNCIONES UTIL -----------

    function renderStars(val, size = 1.5) {
        val = Math.round(val * 2) / 2;
        let html = "";
        for (let i = 1; i <= 5; i++) {
            if (i <= val)
                html += `<i class="bi bi-star-fill text-warning" style="font-size:${size}rem"></i>`;
            else if (i - val === 0.5)
                html += `<i class="bi bi-star-half text-warning" style="font-size:${size}rem"></i>`;
            else
                html += `<i class="bi bi-star" style="color:#ccc;font-size:${size}rem"></i>`;
        }
        return html;
    }

    // ----------- MEDIA Y LISTA DE VALORACIONES -----------

    async function loadRatings() {
        if (!document.getElementById("ratingSectionAJAX") || !productId) return;

        let ratingsListDiv = document.getElementById("ratingsListAJAX");
        let headDiv = document.getElementById("averageRatingHeader");
        try {
            let resp = await fetch(`/api/ratings/product/${productId}?_=${new Date().getTime()}`);
            let data = await resp.json();
            let ratings = (data.success && data.ratings) ? data.ratings : [];

            if (ratings.length > 0) {
                let sum = ratings.reduce((a, b) => a + b.puntuacion, 0);
                let avg = sum / ratings.length;
                headDiv.innerHTML = `
                    <div class="mb-2 d-flex align-items-center">
                        <span class="me-2">${renderStars(avg, 2)}</span>
                        <span class="fw-bold fs-5">${avg.toFixed(1)}</span>
                        <span class="text-muted ms-2">(${ratings.length} valoraciones)</span>
                    </div>`;
            } else {
                headDiv.innerHTML = `<span class="text-muted"><i class="bi bi-star"></i> Sin valoraciones aún</span>`;
            }

            // Listado opiniones
            if (ratings.length === 0) {
                ratingsListDiv.innerHTML = `<div class="alert alert-light border"><i class="bi bi-info-circle"></i> No hay valoraciones todavía. Sé el primero en opinar.</div>`;
            } else {
                let html = "";
                ratings.forEach(rating => {
                    html += `
                    <div class="card mb-3 shadow-sm">
                        <div class="card-body">
                            <div class="d-flex align-items-center mb-2">
                                <span>${renderStars(rating.puntuacion, 1.15)}</span>
                                <strong class="ms-2">${rating.usuario.nombre}</strong>
                                <span class="text-muted ms-2 small">${new Date(rating.fecha).toLocaleString()}</span>
                            </div>
                            ${rating.comentario && rating.comentario.trim()
                        ? `<p class="mb-0 text-muted">${rating.comentario}</p>`
                        : '<p class="mb-0 text-muted fst-italic">Sin comentario</p>'
                    }
                        </div>
                    </div>`;
                });
                ratingsListDiv.innerHTML = html;
            }
        } catch (error) {
            console.error(error);
            ratingsListDiv.innerHTML = `<div class="alert alert-danger">Error al cargar valoraciones.</div>`;
        }
    }

    // ----------- FORMULARIO (con bloqueo tras votar) -----------

    async function loadRatingForm() {
        const container = document.getElementById("ratingSectionAJAX");
        if (!container || !productId) return;

        container.innerHTML = "";

        if (!isAuthenticated || isAuthenticated === "false") {
            container.innerHTML = `
                <div class="alert alert-warning">
                    <i class="bi bi-exclamation-triangle"></i> Debes <a href="/Identity/Account/Login" class="alert-link">iniciar sesión</a> para dejar una valoración.
                </div>`;
            return;
        }

        if (isOwner) {
            container.innerHTML = `
                <div class="alert alert-info">
                    <i class="bi bi-info-circle"></i> No puedes valorar tu propio producto.
                </div>`;
            return;
        }

        try {
            let res = await fetch(`/api/ratings/user/${productId}?t=${new Date().getTime()}`);
            let data = await res.json();

            if (data.success && data.rating) {
                // Ya ha votado
                renderMyRatingCard(container, data.rating);
                return;
            }

            // No ha votado: pinta el form
            renderRatingForm(container);

        } catch (error) {
            console.error(error);
            container.innerHTML = `<div class="alert alert-danger">Error verificando estado de valoración.</div>`;
        }
    }

    function renderMyRatingCard(container, rating) {
        container.innerHTML = `
            <div class="p-4 rounded border border-success bg-light mb-4 fade-in">
                <h5 class="text-success"><i class="bi bi-check-circle-fill"></i> ¡Gracias por tu valoración!</h5>
                <div class="d-flex align-items-center mb-2 mt-3">
                    <span>${renderStars(rating.puntuacion, 1.5)}</span>
                    <span class="ms-2 fw-bold">${rating.puntuacion} / 5</span>
                    <span class="text-muted ms-3 small">${new Date(rating.fecha || Date.now()).toLocaleDateString()}</span>
                </div>
                <p class="mb-0 text-muted">${rating.comentario || '<i>Sin comentario</i>'}</p>
            </div>`;
    }

    function renderRatingForm(container) {
        container.innerHTML = `
            <div class="card border-primary mb-4" id="cardFormulario">
                <div class="card-header bg-primary text-white">
                    <i class="bi bi-pencil-square"></i> Escribe tu valoración
                </div>
                <div class="card-body">
                    <form id="ratingForm">
                        <div class="mb-3">
                            <label class="form-label fw-bold">Tu puntuación:</label>
                            <div class="star-rating-input">
                                <i class="bi bi-star star-item" data-value="1"></i>
                                <i class="bi bi-star star-item" data-value="2"></i>
                                <i class="bi bi-star star-item" data-value="3"></i>
                                <i class="bi bi-star star-item" data-value="4"></i>
                                <i class="bi bi-star star-item" data-value="5"></i>
                                <input type="hidden" name="Puntuacion" id="inputPuntuacion" value="0">
                            </div>
                            <div id="starError" class="text-danger small mt-1" style="display:none;">
                                Debes seleccionar una puntuación.
                            </div>
                        </div>
                        <div class="mb-3">
                            <label class="form-label fw-bold">Comentario (opcional):</label>
                            <textarea class="form-control" name="Comentario" rows="3" maxlength="500" placeholder="¿Qué te ha parecido este producto?"></textarea>
                        </div>
                        <div class="d-grid">
                            <button type="submit" class="btn btn-primary" id="btnSubmitRating">
                                Enviar Valoración
                            </button>
                        </div>
                        <div id="formStatus" class="mt-2"></div>
                    </form>
                </div>
            </div>`;
        initStarLogic();
        initFormSubmit();
    }

    function initStarLogic() {
        const stars = document.querySelectorAll('.star-rating-input .star-item');
        const input = document.getElementById('inputPuntuacion');
        let selectedValue = 0;
        const fillStars = (limit) => {
            stars.forEach(star => {
                const val = parseInt(star.getAttribute('data-value'));
                if (val <= limit) {
                    star.classList.remove('bi-star');
                    star.classList.add('bi-star-fill', 'text-warning');
                } else {
                    star.classList.remove('bi-star-fill', 'text-warning');
                    star.classList.add('bi-star');
                }
            });
        };
        stars.forEach(star => {
            star.addEventListener('mouseenter', () => fillStars(parseInt(star.getAttribute('data-value'))));
            star.addEventListener('click', () => {
                selectedValue = parseInt(star.getAttribute('data-value'));
                input.value = selectedValue;
                star.style.transform = "scale(1.2)";
                setTimeout(() => star.style.transform = "scale(1)", 200);
            });
        });
        document.querySelector('.star-rating-input').addEventListener('mouseleave', () => fillStars(selectedValue));
    }

    function initFormSubmit() {
        const form = document.getElementById("ratingForm");
        form.addEventListener("submit", async (e) => {
            e.preventDefault();

            const statusDiv = document.getElementById("formStatus");
            const btnSubmit = document.getElementById("btnSubmitRating");
            const errorDiv = document.getElementById("starError");

            const puntuacionVal = document.getElementById("inputPuntuacion").value;
            const comentarioVal = form.querySelector('textarea[name="Comentario"]').value;

            if (puntuacionVal == "0") {
                errorDiv.style.display = "block";
                return;
            }
            errorDiv.style.display = "none";

            btnSubmit.disabled = true;
            btnSubmit.innerHTML = `<span class="spinner-border spinner-border-sm"></span> Enviando...`;

            try {
                const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value || '';
                const response = await fetch('/api/ratings', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': token
                    },
                    body: JSON.stringify({
                        productId: productId,
                        puntuacion: parseInt(puntuacionVal),
                        comentario: comentarioVal
                    })
                });
                const result = await response.json();
                if (result.success) {
                    renderMyRatingCard(document.getElementById("ratingSectionAJAX"), {
                        puntuacion: puntuacionVal,
                        comentario: comentarioVal,
                        fecha: new Date().toISOString()
                    });
                    loadRatings();
                } else {
                    statusDiv.innerHTML = `<div class="alert alert-danger mt-2">${result.message || 'Error al guardar'}</div>`;
                    btnSubmit.disabled = false;
                    btnSubmit.innerHTML = `Enviar Valoración`;
                }
            } catch (error) {
                statusDiv.innerHTML = `<div class="alert alert-danger mt-2">Error de conexión</div>`;
                btnSubmit.disabled = false;
                btnSubmit.innerHTML = `Enviar Valoración`;
            }
        });
    }
});