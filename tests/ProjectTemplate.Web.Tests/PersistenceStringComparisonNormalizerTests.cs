using System.Globalization;
using System.Text;
using ProjectTemplate.Infrastructure.Data;

namespace ProjectTemplate.Web.Tests;

public sealed class PersistenceStringComparisonNormalizerTests
{
    [Theory]
    [InlineData(" Already Normalized ", "Already Normalized")]
    [InlineData("\tTabbed Value\r\n", "Tabbed Value")]
    [InlineData("Value", "Value")]
    public void NormalizeRequiredDisplayValue_TrimsValue(string input, string expected)
    {
        string result = PersistenceStringComparisonNormalizer.NormalizeRequiredDisplayValue(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeRequiredDisplayValue_NormalizesUnicodeToFormC()
    {
        string input = "Jose\u0301 User";

        string result = PersistenceStringComparisonNormalizer.NormalizeRequiredDisplayValue(input);

        Assert.Equal("José User", result);
        Assert.True(result.IsNormalized(NormalizationForm.FormC));
    }

    [Fact]
    public void NormalizeRequiredDisplayValue_EncodedValue_IsNotDecoded()
    {
        string input = " O&amp;#39;Connor &amp;amp; Sons ";

        string result = PersistenceStringComparisonNormalizer.NormalizeRequiredDisplayValue(input);

        Assert.Equal("O&amp;#39;Connor &amp;amp; Sons", result);
        Assert.DoesNotContain("O'Connor", result, StringComparison.Ordinal);
    }

    [Fact]
    public void NormalizeRequiredDisplayValue_IsIdempotent()
    {
        string input = " Jose\u0301 User ";

        string once = PersistenceStringComparisonNormalizer.NormalizeRequiredDisplayValue(input);
        string twice = PersistenceStringComparisonNormalizer.NormalizeRequiredDisplayValue(once);

        Assert.Equal(once, twice);
        Assert.Equal("José User", twice);
    }

    [Theory]
    [InlineData("github", "GITHUB")]
    [InlineData(" GitHub ", "GITHUB")]
    [InlineData("user@example.com", "USER@EXAMPLE.COM")]
    public void NormalizeRequiredLookupValue_TrimsNormalizesAndUppercasesInvariant(string input, string expected)
    {
        string result = PersistenceStringComparisonNormalizer.NormalizeRequiredLookupValue(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeRequiredLookupValue_UsesInvariantCasing()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

            string result = PersistenceStringComparisonNormalizer.NormalizeRequiredLookupValue("i");

            Assert.Equal("I", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void NormalizeRequiredLookupValue_IsIdempotent()
    {
        string input = " Jose\u0301@example.com ";

        string once = PersistenceStringComparisonNormalizer.NormalizeRequiredLookupValue(input);
        string twice = PersistenceStringComparisonNormalizer.NormalizeRequiredLookupValue(once);

        Assert.Equal(once, twice);
        Assert.Equal("JOSÉ@EXAMPLE.COM", twice);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void NormalizeOptionalDisplayValue_NullEmptyOrWhitespace_ReturnsNull(string? input)
    {
        string? result = PersistenceStringComparisonNormalizer.NormalizeOptionalDisplayValue(input);

        Assert.Null(result);
    }

    [Fact]
    public void NormalizeOptionalDisplayValue_TrimsAndNormalizesUnicodeToFormC()
    {
        string input = " Jose\u0301 Optional ";

        string? result = PersistenceStringComparisonNormalizer.NormalizeOptionalDisplayValue(input);

        Assert.Equal("José Optional", result);
        Assert.True(result!.IsNormalized(NormalizationForm.FormC));
    }

    [Fact]
    public void NormalizeOptionalDisplayValue_EncodedValue_IsNotDecoded()
    {
        string input = " O&amp;#39;Connor &amp;amp; Sons ";

        string? result = PersistenceStringComparisonNormalizer.NormalizeOptionalDisplayValue(input);

        Assert.Equal("O&amp;#39;Connor &amp;amp; Sons", result);
        Assert.DoesNotContain("O'Connor", result, StringComparison.Ordinal);
    }

    [Fact]
    public void NormalizeOptionalDisplayValue_IsIdempotent()
    {
        string input = " Jose\u0301 Optional ";

        string? once = PersistenceStringComparisonNormalizer.NormalizeOptionalDisplayValue(input);
        string? twice = PersistenceStringComparisonNormalizer.NormalizeOptionalDisplayValue(once);

        Assert.Equal(once, twice);
        Assert.Equal("José Optional", twice);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    public void NormalizeOptionalLookupValue_NullEmptyOrWhitespace_ReturnsNull(string? input)
    {
        string? result = PersistenceStringComparisonNormalizer.NormalizeOptionalLookupValue(input);

        Assert.Null(result);
    }

    [Theory]
    [InlineData("user@example.com", "USER@EXAMPLE.COM")]
    [InlineData(" User@Example.Com ", "USER@EXAMPLE.COM")]
    [InlineData("Jose\u0301@example.com", "JOSÉ@EXAMPLE.COM")]
    public void NormalizeOptionalLookupValue_TrimsNormalizesAndUppercasesInvariant(string input, string expected)
    {
        string? result = PersistenceStringComparisonNormalizer.NormalizeOptionalLookupValue(input);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void NormalizeOptionalLookupValue_UsesInvariantCasing()
    {
        CultureInfo originalCulture = CultureInfo.CurrentCulture;
        CultureInfo originalUiCulture = CultureInfo.CurrentUICulture;

        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("tr-TR");
            CultureInfo.CurrentUICulture = new CultureInfo("tr-TR");

            string? result = PersistenceStringComparisonNormalizer.NormalizeOptionalLookupValue("i");

            Assert.Equal("I", result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public void NormalizeOptionalLookupValue_EncodedValue_IsNotDecoded()
    {
        string input = " O&amp;#39;Connor@example.com ";

        string? result = PersistenceStringComparisonNormalizer.NormalizeOptionalLookupValue(input);

        Assert.Equal("O&AMP;#39;CONNOR@EXAMPLE.COM", result);
        Assert.DoesNotContain("O'CONNOR", result, StringComparison.Ordinal);
    }

    [Fact]
    public void NormalizeOptionalLookupValue_IsIdempotent()
    {
        string input = " Jose\u0301@example.com ";

        string? once = PersistenceStringComparisonNormalizer.NormalizeOptionalLookupValue(input);
        string? twice = PersistenceStringComparisonNormalizer.NormalizeOptionalLookupValue(once);

        Assert.Equal(once, twice);
        Assert.Equal("JOSÉ@EXAMPLE.COM", twice);
    }
}
