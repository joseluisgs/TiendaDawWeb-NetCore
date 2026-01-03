# 13 - Pruebas de Extremo a Extremo (E2E) con Cypress

En este volumen exploramos la capa superior de nuestra estrategia de calidad: los tests E2E. Mientras que NUnit y bUnit prueban piezas aisladas, Cypress actúa como un "usuario robot" que navega por la web real.

---

## 1. ¿Por qué Cypress?

Cypress es el estándar moderno para pruebas de interfaz. Lo hemos elegido para este proyecto porque:
- **Snapshot Dinámico**: Permite ver exactamente qué hacía el robot en cada paso.
- **Espera Automática**: Maneja muy bien la carga asíncrona de datos.
- **Aislamiento**: Se ejecuta en un entorno Node.js independiente, sin ensuciar la solución .NET.

---

## 2. Estructura de la carpeta `e2e/`

Hemos seguido un diseño de **Aislamiento Total**:
```text
e2e/
├── cypress/
│   ├── e2e/             # Aquí residen los archivos .cy.js (las pruebas)
│   └── screenshots/     # Capturas automáticas si un test falla (ignorado por git)
├── cypress.config.js    # Configuración (URL base, resolución de pantalla)
└── package.json         # Dependencias de Node.js
```

---

## 3. Casos de Uso Críticos en WalaDaw

Los tests E2E no deben probarlo todo, sino los **flujos de valor**:

### A. Autenticación (`auth.cy.js`)
Verifica que el flujo de Login no se rompa tras cambios en el middleware de seguridad o en el proveedor de identidad.

### B. Interacción Reactiva Blazor (`blazor_ratings.cy.js`)
Este es el test más avanzado. El robot:
1. Inicia sesión.
2. Entra en un producto.
3. Clica en una estrella de Blazor.
4. Escribe un comentario.
5. Verifica que la cabecera (otro componente) se actualiza por SignalR.

### C. Flujo de Compra (`product_flow.cy.js`)
Simula el "Happy Path" de un cliente: Buscar -> Seleccionar -> Carrito. Asegura que el motor de búsqueda y la persistencia en RAM (SQLite) funcionan coordinados.

---

## 4. Instrucciones para el Alumno

### Preparación del entorno
Es necesario tener instalado [Node.js](https://nodejs.org/).
```bash
cd e2e
npm install
```

### Ejecución de las pruebas
1. Asegúrate de que la aplicación .NET está corriendo (ej. `dotnet watch`).
2. Abre la interfaz visual de Cypress:
```bash
npx cypress open
```
3. O ejecútalos en modo "Headless" (sin ventana, ideal para CI/CD):
```bash
npx cypress run
```

---

## 5. Conclusión

Dominar los tests E2E permite al desarrollador hacer refactors masivos en la lógica de negocio con la total seguridad de que la experiencia del usuario final sigue siendo impecable.
