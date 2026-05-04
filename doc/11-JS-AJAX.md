# 11. JavaScript y AJAX

## Índice

[11. JavaScript y AJAX](#11-javascript-y-ajax)
  - [11.1. Fetch API](#111-fetch-api)
  - [11.2. XMLHttpRequest (Legacy)](#112-xmlhttprequest-legacy)
  - [11.3. jQuery AJAX](#113-jquery-ajax)
  - [11.4. Seguridad CSRF](#114-seguridad-csrf)
  - [11.5. Fetch con Token JWT](#115-fetch-con-token-jwt)
  - [11.6. Manejo de Errores](#116-manejo-de-errores)

---

## 11.1. Fetch API

```javascript
async function toggleFavorite(productId) {
    const response = await fetch(`/Favorites/Toggle/${productId}`, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        }
    });
    
    const result = await response.json();
    if (result.success) {
        // Actualizar UI
    }
}

// GET con manejo de errores
async function loadProducts() {
    try {
        const response = await fetch('/api/products');
        
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }
        
        const products = await response.json();
        return products;
    } catch (error) {
        console.error('Error cargando productos:', error);
        return [];
    }
}
```

---

## 11.2. XMLHttpRequest (Legacy)

```javascript
function loadProductsXhr() {
    var xhr = new XMLHttpRequest();
    
    xhr.open('GET', '/api/products', true);
    
    xhr.onreadystatechange = function() {
        if (xhr.readyState === 4) {
            if (xhr.status === 200) {
                var products = JSON.parse(xhr.responseText);
                console.log(products);
            } else {
                console.error('Error:', xhr.statusText);
            }
        }
    };
    
    xhr.send();
}
```

---

## 11.3. jQuery AJAX

```javascript
// GET
$.ajax({
    url: '/api/products',
    method: 'GET',
    dataType: 'json',
    success: function(products) {
        console.log(products);
    },
    error: function(xhr, status, error) {
        console.error('Error:', error);
    }
});

// POST con CSRF
$.ajax({
    url: '/cart/add',
    method: 'POST',
    data: {
        productId: 123,
        quantity: 2,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    },
    success: function(response) {
        console.log('Agregado al carrito');
    }
});
```

---

## 11.4. Seguridad CSRF

### AntiForgeryToken en Formularios

```html
<form id="myForm">
    @Html.AntiForgeryToken()
    <input type="text" name="name" />
    <button type="submit">Enviar</button>
</form>
```

### Con Fetch API

```javascript
async function submitWithCsrf(url, data) {
    const token = document.querySelector('input[name="__RequestVerificationToken"]').value;
    
    const response = await fetch(url, {
        method: 'POST',
        headers: {
            'RequestVerificationToken': token,
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(data)
    });
    
    return response.json();
}
```

### Validar en Controller

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult Create(ProductDto dto)
{
    // El token se valida automáticamente
    // Si es inválido, devuelve 400 Bad Request
}
```

---

## 11.5. Fetch con Token JWT

```javascript
// Obtener token del almacenamiento
function getAuthHeaders() {
    const token = localStorage.getItem('jwt_token');
    
    return {
        'Content-Type': 'application/json',
        'Authorization': `Bearer ${token}`
    };
}

// Petición autenticada
async function getProtectedData() {
    const response = await fetch('/api/protected', {
        method: 'GET',
        headers: getAuthHeaders()
    });
    
    if (response.status === 401) {
        // Token expirado o inválido
        window.location.href = '/login';
        return;
    }
    
    return response.json();
}
```

---

## 11.6. Manejo de Errores

```javascript
async function safeFetch(url, options = {}) {
    try {
        const response = await fetch(url, options);
        
        if (!response.ok) {
            const error = await response.json().catch(() => ({}));
            throw new Error(error.message || `HTTP ${response.status}`);
        }
        
        return await response.json();
    } catch (error) {
        console.error('Fetch error:', error);
        throw error;
    }
}

// Uso
try {
    const products = await safeFetch('/api/products');
    console.log(products);
} catch (error) {
    showNotification('Error cargando productos', 'error');
}
```

---

## Resumen

| API              | Pros                                    | Contras                                |
| --------------- | -------------------------------------- | -------------------------------------- |
| **Fetch**        | Moderno, nativo, Promises              | Sin soporte nativo para progreso        |
| **XMLHttpRequest** | Compatibilidad con legacy              | API verbosa, callbacks                |
| **jQuery AJAX** | Facilita trabajo con DOM, histórico    | Dependencia externa, menos relevante    |

---

**Anterior**: [10. Internacionalización (I18n)](../10-I18n.md)  
**Próximo**: [12. Razor vs AJAX vs Blazor](../12-BlazorVsRazor.md)
