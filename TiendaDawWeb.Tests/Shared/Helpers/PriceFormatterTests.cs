using FluentAssertions;
using TiendaDawWeb.Shared.Helpers;

namespace TiendaDawWeb.Tests.Shared.Helpers;

public class PriceFormatterTests
{
    #region FormatPrice Tests

    [Test]
    public void FormatPrice_FormatsZero_Correctly()
    {
        var result = PriceFormatter.FormatPrice(0m);

        result.Should().Contain("0");
    }

    [Test]
    public void FormatPrice_FormatsOne_Correctly()
    {
        var result = PriceFormatter.FormatPrice(1m);

        result.Should().Contain("1");
    }

    [Test]
    public void FormatPrice_Formats99_Correctly()
    {
        var result = PriceFormatter.FormatPrice(99m);

        result.Should().Contain("99");
    }

    [Test]
    public void FormatPrice_Formats100_Correctly()
    {
        var result = PriceFormatter.FormatPrice(100m);

        result.Should().Contain("100");
    }

    [Test]
    public void FormatPrice_Formats999_Correctly()
    {
        var result = PriceFormatter.FormatPrice(999m);

        result.Should().Contain("999");
    }

    [Test]
    public void FormatPrice_Formats1000_Correctly()
    {
        var result = PriceFormatter.FormatPrice(1000m);

        result.Should().Contain("1.000");
    }

    [Test]
    public void FormatPrice_Formats9999_Correctly()
    {
        var result = PriceFormatter.FormatPrice(9999m);

        result.Should().Contain("999");
        result.Should().Contain("999");
    }

    [Test]
    public void FormatPrice_Formats99999_Correctly()
    {
        var result = PriceFormatter.FormatPrice(99999m);

        result.Should().Contain("99");
        result.Should().Contain("999");
    }

    [Test]
    public void FormatPrice_FormatsDecimal_Correctly()
    {
        var result = PriceFormatter.FormatPrice(99.99m);

        result.Should().Contain("99");
        result.Should().Contain("99");
    }

    [Test]
    public void FormatPrice_FormatsLargeNumber_Correctly()
    {
        var result = PriceFormatter.FormatPrice(1234567.89m);

        result.Should().Contain("1");
        result.Should().Contain("234");
        result.Should().Contain("567");
    }

    [Test]
    public void FormatPrice_ContainsEuroSymbol()
    {
        var result = PriceFormatter.FormatPrice(100m);

        result.Should().Contain("€");
    }

    #endregion

    #region FormatPriceWithSymbol Tests

    [Test]
    [TestCase(0, "0,00 €")]
    [TestCase(1, "1,00 €")]
    [TestCase(99.99, "99,99 €")]
    [TestCase(100, "100,00 €")]
    [TestCase(999.99, "999,99 €")]
    [TestCase(1000, "1.000,00 €")]
    [TestCase(1999.99, "1.999,99 €")]
    [TestCase(10000, "10.000,00 €")]
    [TestCase(999999.99, "999.999,99 €")]
    public void FormatPriceWithSymbol_FormatsCorrectly(decimal input, string expected)
    {
        var result = PriceFormatter.FormatPriceWithSymbol(input);

        result.Should().Be(expected);
    }

    [Test]
    public void FormatPriceWithSymbol_HandlesLargeNumbers_Success()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(1234567.89m);

        result.Should().Be("1.234.567,89 €");
    }

    [Test]
    public void FormatPriceWithSymbol_HandlesNegativeNumbers_Success()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(-100m);

        result.Should().Be("-100,00 €");
    }

    [Test]
    public void FormatPriceWithSymbol_HandlesSingleDigit_Success()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(5m);

        result.Should().Be("5,00 €");
    }

    [Test]
    public void FormatPriceWithSymbol_HandlesDecimalValues_Success()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(10.50m);

        result.Should().Be("10,50 €");
    }

    [Test]
    public void FormatPriceWithSymbol_HandlesPreciseDecimals_Success()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(99.99m);

        result.Should().Be("99,99 €");
    }

    [Test]
    public void FormatPriceWithSymbol_AlwaysEndsWithEuroSymbol()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(100m);

        result.Should().EndWith("€");
    }

    [Test]
    public void FormatPriceWithSymbol_ContainsTwoDecimalPlaces()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(1m);

        result.Should().MatchRegex(@"\d+,\d{2} €");
    }

    [Test]
    public void FormatPriceWithSymbol_FormatsOneMillion_Correctly()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(1000000m);

        result.Should().Be("1.000.000,00 €");
    }

    [Test]
    public void FormatPriceWithSymbol_FormatsCeroPuntoCero_Correctly()
    {
        var result = PriceFormatter.FormatPriceWithSymbol(0.00m);

        result.Should().Be("0,00 €");
    }

    #endregion
}
