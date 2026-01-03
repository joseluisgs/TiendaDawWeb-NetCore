/**
 * FLUJO DE CATÁLOGO Y NAVEGACIÓN
 * 
 * OBJETIVO: Asegurar que el usuario puede encontrar productos.
 * TECNOLOGÍAS TESTEADAS: Motores de búsqueda (q), Vistas Razor, Rutas MVC.
 */
describe('Flujo de Catálogo', () => {
  
  it('Búsqueda: Debe encontrar un iPhone y entrar en su ficha técnica', () => {
    // Escaparate público
    cy.visit('/Public');

    // 1. Buscamos en la zona central (campo 'q' según el Index.cshtml)
    cy.get('main input[name="q"]').type('iPhone');
    cy.get('main button[type="submit"]').click();

    // 2. Verificamos que los resultados contienen el texto buscado
    cy.get('.card-title').first().should('contain', 'iPhone').as('primerResultado');

    // 3. Navegamos al detalle del producto
    cy.get('@primerResultado').click();

    // 4. Verificamos que la URL es la del controlador de detalles
    cy.url().should('include', '/Product/Details');
    
    // 5. El título del producto debe estar visible en grande (H1)
    cy.get('h1').should('be.visible').and('contain', 'iPhone');
  });
});
