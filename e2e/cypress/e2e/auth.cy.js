/**
 * MÓDULO DE AUTENTICACIÓN
 * 
 * OBJETIVO: Validar que el flujo de acceso (Login) sea seguro y funcional.
 * TECNOLOGÍAS TESTEADAS: ASP.NET Core Identity, DataAnnotations (Validación).
 */
describe('Módulo de Autenticación', () => {
  
  // Se ejecuta antes de cada test dentro de este bloque
  beforeEach(() => {
    // Navega a la URL de login definida en AuthController
    cy.visit('/Auth/Login');
  });

  it('Validación de campos vacíos: Debe detectar errores de servidor', () => {
    // Intentamos enviar el formulario sin datos
    // Usamos selectores precisos para evitar el botón de búsqueda de la navbar
    cy.get('.card-body form').find('button[type="submit"]').click();
    
    // Verificamos que aparezcan los mensajes de error de las DataAnnotations de C#
    cy.get('.text-danger').should('be.visible');
  });

  it('Login exitoso: El administrador debe entrar correctamente', () => {
    // Rellenamos los campos con las credenciales del SeedData
    cy.get('.card-body #Email').type('admin@waladaw.com');
    cy.get('.card-body #Password').type('admin');
    
    // Enviamos el formulario
    cy.get('.card-body form').find('button[type="submit"]').click();
    
    // Verificamos que la URL ha cambiado (ya no estamos en Login)
    cy.url().should('not.contain', '/Auth/Login');
    
    // Verificamos que la Navbar se ha actualizado para mostrar el menú de Admin
    cy.get('.navbar').should('contain', 'Admin');
  });

  it('Seguridad: Debe rechazar credenciales incorrectas', () => {
    cy.get('.card-body #Email').type('hacker@maligno.com');
    cy.get('.card-body #Password').type('123456');
    cy.get('.card-body form').find('button[type="submit"]').click();
    
    // El sistema debe responder con el mensaje de error definido en Identity
    cy.get('.text-danger').should('contain.text', 'incorrectos');
  });
});
