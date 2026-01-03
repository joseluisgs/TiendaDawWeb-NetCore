using Microsoft.Playwright;
using Microsoft.Playwright.NUnit;
using NUnit.Framework;
using System.IO;

namespace TiendaDawWeb.Tests.E2E;

/**
 * MÓDULO DE GESTIÓN DE PRODUCTOS (E2E)
 * 
 * OBJETIVO: Probar la edición de entidades y el servicio de almacenamiento (IStorageService).
 * TECNOLOGÍAS TESTEADAS: IFormFile (Subida de archivos), Playwright SetInputFiles, Locale es-ES.
 */
[Parallelizable(ParallelScope.Self)]
[TestFixture]
public class ProductManagementTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";

    /// <summary>
    /// Configuración del contexto: Forzamos locale español para validar formatos decimales.
    /// </summary>
    public override BrowserNewContextOptions ContextOptions()
    {
        return new BrowserNewContextOptions
        {
            Locale = "es-ES",
            TimezoneId = "Europe/Madrid"
        };
    }

    /// <summary>
    /// Configuración previa: Login como usuario propietario.
    /// </summary>
    [SetUp]
    public async Task Setup()
    {
        await Page.GotoAsync($"{BaseUrl}/Auth/Login");
        await Page.FillAsync("#Email", "prueba@prueba.com");
        await Page.FillAsync("#Password", "prueba");
        await Page.ClickAsync(".card-body form button[type='submit']");
        
        await Expect(Page.Locator(".navbar")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("Prueba", System.Text.RegularExpressions.RegexOptions.IgnoreCase));
    }

    /// <summary>
    /// Test: Edición completa. Permite cambiar datos y subir una imagen (fixture local).
    /// </summary>
    [Test]
    public async Task EditProduct_ShouldUpdateValuesAndUploadImage()
    {
        // 1. Acción: Ir a Mis Productos y seleccionar el primero
        await Page.GotoAsync($"{BaseUrl}/Product/MyProducts");
        var firstEditBtn = Page.Locator("a.btn-warning").First;
        await Expect(firstEditBtn).ToBeVisibleAsync();
        await firstEditBtn.ClickAsync();

        // 2. Acción: Modificar campos de texto y precio
        string nuevaDesc = "Descripción generada por Playwright " + System.DateTimeOffset.Now.ToUnixTimeMilliseconds();
        await Page.FillAsync("#Descripcion", nuevaDesc);
        await Page.FillAsync("#Precio", "125.50");

        // 3. Acción: Subida de archivo usando fixture local
        var projectRoot = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..", ".."));
        var fixturePath = Path.Combine(projectRoot, "Fixtures", "test-product.svg");
        Assert.That(File.Exists(fixturePath), Is.True, $"El fixture no existe en: {fixturePath}");
        await Page.SetInputFilesAsync("#ImagenFile", fixturePath);

        // 4. Acción: Guardar cambios
        await Page.ClickAsync("main form button[type='submit']");

        // 5. Verificación: Mensaje de éxito del TempData
        await Expect(Page.Locator("body")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("exitosamente|actualizado", System.Text.RegularExpressions.RegexOptions.IgnoreCase));

        // 6. Verificación: Persistencia y formato decimal (coma)
        await Expect(Page.Locator("main")).ToContainTextAsync(nuevaDesc);
        await Expect(Page.Locator("main")).ToContainTextAsync(new System.Text.RegularExpressions.Regex("125[.,]50"));
    }
}
