/**
 * GESTIÓN DE PRODUCTOS (EDICIÓN Y SUBIDA DE ARCHIVOS)
 * 
 * OBJETIVO: Probar la edición de entidades y el servicio de almacenamiento (IStorageService).
 * TECNOLOGÍAS TESTEADAS: IFormFile, ImageSharp (Redimensionado), SQLite Persistencia.
 */
describe('Módulo de Gestión de Productos', () => {
  
  beforeEach(() => {
    // Login con el usuario dueño de productos (prueba)
    cy.visit('/Auth/Login');
    cy.get('.card-body #Email').type('prueba@prueba.com');
    cy.get('.card-body #Password').type('prueba');
    cy.get('.card-body form').find('button[type="submit"]').click();
  });

  it('Edición: Debe permitir cambiar descripción, precio y subir una imagen nueva', () => {
    // 1. Navegamos a la zona privada de productos del usuario
    cy.visit('/Product/MyProducts');

    // 2. Clic en editar del primer producto disponible
    cy.get('a.btn-warning').first().click();

    // 3. Modificamos los campos del formulario
    const nuevaDesc = 'Descripción generada por Cypress ' + Date.now();
    cy.get('#Descripcion').clear().type(nuevaDesc);
    cy.get('#Precio').clear().type('125.50');

    // 4. SUBIDA DE ARCHIVO (FIXTURE)
    // Simulamos la subida de un fichero real usando el fixture preparado
    cy.get('input[type="file"]').selectFile('cypress/fixtures/test-product.svg');

    // 5. Guardamos cambios
    // Usamos selectores de 'main form' para no clicar accidentalmente el buscador de la navbar
    cy.get('main form').find('button[type="submit"]').click();

    // 6. VERIFICACIÓN DE ÉXITO
    // Comprobamos el mensaje de éxito enviado por el TempData de .NET
    cy.get('body').should('contain', 'actualizado');
    
    // 7. VERIFICACIÓN DE PERSISTENCIA
    // Comprobamos que el detalle muestra los nuevos valores (incluyendo el formato decimal español)
    cy.get('main').should('contain', nuevaDesc);
    cy.get('main').should('contain', '125,50');
  });
});