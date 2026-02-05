#nullable disable
using FluentAssertions;
using TiendaDawWeb.Shared.Helpers;

namespace TiendaDawWeb.Tests.Shared;

public class PriceFormatterTests
{
    [Test]
    public void FormatPrice_FormatsZeroPrice()
    {
        var result = PriceFormatter.FormatPrice(0m);
        result.Should().Contain("0");
    }

    [Test]
    public void FormatPrice_FormatsPositivePrice()
    {
        var result = PriceFormatter.FormatPrice(99.99m);
        result.Should().Contain("99");
    }

    [Test]
    public void FormatPrice_FormatsLargePrice()
    {
        var result = PriceFormatter.FormatPrice(9999.99m);
        // El resultado contiene el número, verificamos que no esté en formato inglés
        result.Should().MatchRegex(@"\d{1,3}(\.\d{3})*,\d{2}");
    }

    [Test]
    public void FormatPriceWithSymbol_FormatsPrice_WithEuroSymbol()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(199.99m);
        result.Should().Contain("199,99");
        result.Should().Contain("€");
    }

    [Test]
    public void FormatPriceWithSymbol_FormatsZeroPrice()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(0m);
        result.Should().Contain("0,00");
        result.Should().Contain("€");
    }

    [Test]
    public void FormatPriceWithSymbol_FormatsLargePrice()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(999999.99m);
        result.Should().Contain("999.999,99");
        result.Should().Contain("€");
    }

    [Test]
    public void FormatPriceWithSymbol_FormatsIntegerPrice()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(100m);
        result.Should().Contain("100,00");
        result.Should().Contain("€");
    }

    [Test]
    public void FormatPriceWithSymbol_FormatsSmallDecimal()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(0.01m);
        result.Should().Contain("0,01");
        result.Should().Contain("€");
    }
}
