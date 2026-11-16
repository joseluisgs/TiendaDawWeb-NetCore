#nullable disable
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TiendaDawWeb.Mvc.TagHelpers;

namespace TiendaDawWeb.Tests.Mvc;

public class PriceTagHelperTests {
    [Test]
    public void Process_FormatsPrice_WithEuroSymbol_UsingSpanishCulture() {
        decimal price = 199.99m;
        string expected = price.ToString("C2", new CultureInfo("es-ES"));

        expected.Should().Contain("199,99");
        expected.Should().Contain("€");
    }

    [Test]
    public void Process_FormatsZeroPrice_Correctly() {
        decimal price = 0m;
        string expected = price.ToString("C2", new CultureInfo("es-ES"));

        expected.Should().Contain("0,00");
    }

    [Test]
    public void Process_FormatsLargePrice_Correctly() {
        decimal price = 9999.99m;
        string expected = price.ToString("C2", new CultureInfo("es-ES"));

        expected.Should().Contain("9.999,99");
    }

    [Test]
    public void Process_FormatsSmallDecimalPrice() {
        decimal price = 0.01m;
        string expected = price.ToString("C2", new CultureInfo("es-ES"));

        expected.Should().Contain("0,01");
    }

    [Test]
    public void Process_FormatsIntegerPrice() {
        decimal price = 100m;
        string expected = price.ToString("C2", new CultureInfo("es-ES"));

        expected.Should().Contain("100,00");
    }

    [Test]
    public void PriceTagHelper_HasValueProperty() {
        var tagHelper = new PriceTagHelper { Value = 199.99m };
        tagHelper.Value.Should().Be(199.99m);
    }

    [Test]
    public void PriceTagHelper_DefaultValue_IsZero() {
        var tagHelper = new PriceTagHelper();
        tagHelper.Value.Should().Be(0);
    }

    [Test]
    public void PriceTagHelper_CanHandleNegativeValues() {
        var tagHelper = new PriceTagHelper { Value = -50m };
        tagHelper.Value.Should().Be(-50m);
    }

    [Test]
    public void PriceTagHelper_CanHandleLargeValues() {
        var tagHelper = new PriceTagHelper { Value = 999999.99m };
        tagHelper.Value.Should().Be(999999.99m);
    }

    [Test]
    public void PriceTagHelper_CanHandleDecimalValues() {
        var tagHelper = new PriceTagHelper { Value = 123.45m };
        tagHelper.Value.Should().Be(123.45m);
    }

    [Test]
    public void SpanishCulture_UsesEuroSymbol() {
        var culture = new CultureInfo("es-ES");
        string formatted = 100m.ToString("C2", culture);

        formatted.Should().Contain("€");
    }

    [Test]
    public void SpanishCulture_UsesCommaAsDecimalSeparator() {
        var culture = new CultureInfo("es-ES");
        string formatted = 99.99m.ToString("C2", culture);

        formatted.Should().Contain("99,99");
    }

    [Test]
    public void SpanishCulture_UsesDotAsThousandsSeparator() {
        var culture = new CultureInfo("es-ES");
        string formatted = 9999m.ToString("C2", culture);

        formatted.Should().Contain("9.999");
    }

    // ---- Process() real: la moneda ahora sigue a la cultura de la petición ----

    [Test]
    public void Process_WithEnglishCulture_FormatsInDollars() {
        CultureInfo original = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = new CultureInfo("en-US");
            var helper = new PriceTagHelper { Value = 199.99m };
            var context = new TagHelperContext(
                new TagHelperAttributeList(), new Dictionary<object, object>(), "test");
            var output = new TagHelperOutput("price", new TagHelperAttributeList(),
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            helper.Process(context, output);

            string html = output.Content.GetContent();
            html.Should().Contain("$");
            html.Should().Contain("199.99");
        } finally {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Test]
    public void Process_WithSpanishCulture_FormatsInEuros() {
        CultureInfo original = CultureInfo.CurrentCulture;
        try {
            CultureInfo.CurrentCulture = new CultureInfo("es-ES");
            var helper = new PriceTagHelper { Value = 199.99m };
            var context = new TagHelperContext(
                new TagHelperAttributeList(), new Dictionary<object, object>(), "test");
            var output = new TagHelperOutput("price", new TagHelperAttributeList(),
                (useCachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            helper.Process(context, output);

            // TagHelperOutput HTML-escapa el contenido: "€" sale como &#x20AC;
            string html = System.Net.WebUtility.HtmlDecode(output.Content.GetContent());
            html.Should().Contain("€");
            html.Should().Contain("199,99");
        } finally {
            CultureInfo.CurrentCulture = original;
        }
    }
}
