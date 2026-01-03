# 16 - Cimientos de Calidad: Tests Unitarios y de Componentes

En este volumen bajamos al nivel de los "engranajes" de nuestra aplicación. Los tests unitarios aseguran que cada función, servicio y componente Blazor haga exactamente lo que debe, aislándolos de factores externos como la base de datos o la red.

---

## 1. El Tridente de Calidad (.NET Testing Stack)

Para garantizar la fiabilidad de **WalaDaw**, combinamos tres herramientas potentes:

1.  **NUnit**: El motor de ejecución de pruebas más veterano y estable. Define el ciclo de vida (`[SetUp]`, `[Test]`).
2.  **Moq**: La herramienta esencial para el **Aislamiento**. Permite crear "dobles" de servicios (ej. `IRatingService`) para probar controladores sin tocar datos reales.
3.  **bUnit**: La librería específica para **Blazor**. Permite renderizar componentes en memoria, simular clics y verificar que el HTML generado es correcto.

---

## 2. El Patrón Maestro: AAA (Arrange, Act, Assert)

Todos nuestros tests siguen esta estructura para ser legibles y mantenibles:

-   **Arrange (Preparar)**: Configuramos el escenario, instanciamos objetos y preparamos los Mocks.
-   **Act (Actuar)**: Ejecutamos el método o renderizamos el componente que queremos probar.
-   **Assert (Verificar)**: Comprobamos que el resultado es el esperado.

```csharp
[Test]
public void Should_Calculate_Cart_Total_Correctly()
{
    // Arrange
    var service = new CarritoService(...);
    // Act
    var total = service.GetTotal();
    // Assert
    Assert.That(total, Is.EqualTo(expectedValue));
}
```

---

## 3. Testeando Lógica de Negocio con Moq

En `TiendaDawWeb.Tests`, la clave es el **Aislamiento**. No probamos el servicio contra la base de datos (para eso están los de integración), sino contra interfaces mockeadas.

### Ejemplo de Mocking:
Si probamos un controlador que envía emails, no queremos enviar emails reales. Usamos Moq para verificar que el método `SendEmailAsync` fue llamado con los parámetros correctos.

---

## 4. bUnit: Entrando en el DOM de Blazor

Los componentes Blazor son especiales porque mezclan lógica C# con UI. Con bUnit podemos:
-   Verificar que un botón está deshabilitado si no hay login.
-   Simular que el usuario pulsa la 5ª estrella.
-   Verificar que el `StateContainer` notificó el cambio a otros componentes.

```csharp
// En bUnit, renderizamos el componente y buscamos elementos CSS
var cut = RenderComponent<RatingSummary>(...);
var stars = cut.FindAll("i.bi-star-fill");
Assert.That(stars.Count, Is.EqualTo(5));
```

---

## 5. Mejores Prácticas del Experto

1.  **Test de una sola cosa**: Cada método `[Test]` debe validar un único comportamiento.
2.  **Nombres Descriptivos**: `Should_RedirectToLogin_When_UserNotAuthenticated` es mejor que `Test1`.
3.  **Independencia**: Un test nunca debe depender del resultado de otro.
4.  **Aprovecha FluentAssertions**: Para que tus aserciones se lean como lenguaje natural (`result.Should().BeEquivalentTo(expected)`).

---

## 6. Conclusión

Los tests unitarios son los más rápidos de ejecutar y los que te dan feedback inmediato mientras programas. Una buena base de tests unitarios reduce el tiempo de depuración en un 80% y permite hacer refactors sin miedo.
