/**
 * MÓDULO DE FAVORITOS (AJAX)
 * 
 * OBJETIVO: Probar la interactividad asíncrona sin recarga de página.
 * TECNOLOGÍAS TESTEADAS: Fetch API, JavaScript (favorites.js), API Controllers.
 */
describe('Módulo de Favoritos', () => {
  
  beforeEach(() => {
    // Es necesario estar logueado para tener favoritos
    cy.visit('/Auth/Login');
    cy.get('.card-body #Email').type('prueba@prueba.com');
    cy.get('.card-body #Password').type('prueba');
    cy.get('.card-body form').find('button[type="submit"]').click();
  });

  it('Alternar favoritos: Debe cambiar el estado del botón mediante AJAX', () => {
    // Vamos al detalle de un producto que no es nuestro (ID 3 - Pixel 8)
    cy.visit('/Product/Details/3');

    // Identificamos el botón de favoritos
    cy.get('.favorite-btn').should('be.visible').as('btnCorazon');

    // Capturamos el estado inicial (clase CSS) para que el test sea determinista
    cy.get('@btnCorazon').then(($btn) => {
      const esFavoritoInicialmente = $btn.hasClass('btn-danger');

      // ACCIÓN: Clic en el corazón
      cy.get('@btnCorazon').click();

      // VERIFICACIÓN 1: El script favorites.js debe lanzar una notificación Toast de Bootstrap
      cy.get('.toast-body').should('be.visible');
      
      // VERIFICACIÓN 2: El botón debe haber cambiado su clase CSS dinámicamente
      if (esFavoritoInicialmente) {
        cy.get('@btnCorazon').should('not.have.class', 'btn-danger');
      } else {
        cy.get('@btnCorazon').should('have.class', 'btn-danger');
      }
    });
  });
});
