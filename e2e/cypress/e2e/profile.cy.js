/**
 * MÓDULO DE PERFIL
 * 
 * OBJETIVO: Verificar que las rutas personalizadas ([Route("app/perfil")]) funcionan.
 * TECNOLOGÍAS TESTEADAS: Atributos de ruta de ASP.NET, UserManager.
 */
describe('Módulo de Perfil', () => {
  
  beforeEach(() => {
    cy.visit('/Auth/Login');
    cy.get('.card-body #Email').type('prueba@prueba.com');
    cy.get('.card-body #Password').type('prueba');
    cy.get('.card-body form').find('button[type="submit"]').click();
  });

  it('Visualización: Debe mostrar los datos reales del usuario logueado', () => {
    // Usamos la ruta custom definida en el ProfileController
    cy.visit('/app/perfil');

    // Comprobamos que la vista muestra el nombre 'prueba' del SeedData
    cy.get('main').should('contain', 'prueba');
    cy.get('main').should('contain', 'prueba@prueba.com');
  });

  it('Navegación: Debe permitir entrar al modo edición', () => {
    cy.visit('/app/perfil');
    cy.get('a').contains('Editar').click();

    // Verificamos la sub-ruta 'editar'
    cy.url().should('include', '/app/perfil/editar');
    
    // El formulario debe contener el valor actual para permitir editarlo
    cy.get('input[name="nombre"]').should('have.value', 'prueba');
  });
});
