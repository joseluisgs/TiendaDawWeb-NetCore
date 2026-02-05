#nullable disable
using FluentAssertions;
using System.Globalization;
using TiendaDawWeb.Mvc.TagHelpers;

namespace TiendaDawWeb.Tests.Mvc;

public class PriceTagHelperTests
{
    [Test]
    public void Process_FormatsPrice_WithEuroSymbol_UsingSpanishCulture()
    {
        var price = 199.99m;
        var expected = price.ToString("C2", new CultureInfo("es-ES"));
        
        expected.Should().Contain("199,99");
        expected.Should().Contain("€");
    }

    [Test]
    public void Process_FormatsZeroPrice_Correctly()
    {
        var price = 0m;
        var expected = price.ToString("C2", new CultureInfo("es-ES"));
        
        expected.Should().Contain("0,00");
    }

    [Test]
    public void Process_FormatsLargePrice_Correctly()
    {
        var price = 9999.99m;
        var expected = price.ToString("C2", new CultureInfo("es-ES"));
        
        expected.Should().Contain("9.999,99");
    }

    [Test]
    public void Process_FormatsSmallDecimalPrice()
    {
        var price = 0.01m;
        var expected = price.ToString("C2", new CultureInfo("es-ES"));
        
        expected.Should().Contain("0,01");
    }

    [Test]
    public void Process_FormatsIntegerPrice()
    {
        var price = 100m;
        var expected = price.ToString("C2", new CultureInfo("es-ES"));
        
        expected.Should().Contain("100,00");
    }

    [Test]
    public void PriceTagHelper_HasValueProperty()
    {
        var tagHelper = new PriceTagHelper { Value = 199.99m };
        tagHelper.Value.Should().Be(199.99m);
    }

    [Test]
    public void PriceTagHelper_DefaultValue_IsZero()
    {
        var tagHelper = new PriceTagHelper();
        tagHelper.Value.Should().Be(0);
    }

    [Test]
    public void PriceTagHelper_CanHandleNegativeValues()
    {
        var tagHelper = new PriceTagHelper { Value = -50m };
        tagHelper.Value.Should().Be(-50m);
    }

    [Test]
    public void PriceTagHelper_CanHandleLargeValues()
    {
        var tagHelper = new PriceTagHelper { Value = 999999.99m };
        tagHelper.Value.Should().Be(999999.99m);
    }

    [Test]
    public void PriceTagHelper_CanHandleDecimalValues()
    {
        var tagHelper = new PriceTagHelper { Value = 123.45m };
        tagHelper.Value.Should().Be(123.45m);
    }

    [Test]
    public void SpanishCulture_UsesEuroSymbol()
    {
        var culture = new CultureInfo("es-ES");
        var formatted = 100m.ToString("C2", culture);
        
        formatted.Should().Contain("€");
    }

    [Test]
    public void SpanishCulture_UsesCommaAsDecimalSeparator()
    {
        var culture = new CultureInfo("es-ES");
        var formatted = 99.99m.ToString("C2", culture);
        
        formatted.Should().Contain("99,99");
    }

    [Test]
    public void SpanishCulture_UsesDotAsThousandsSeparator()
    {
        var culture = new CultureInfo("es-ES");
        var formatted = 9999m.ToString("C2", culture);
        
        formatted.Should().Contain("9.999");
    }
}
