using System.Text;

namespace ProjectTemplate.Infrastructure.Data;

internal static class PersistenceStringComparisonNormalizer
{
    internal static string NormalizeRequiredDisplayValue(string value)
    {
        return value.Trim().Normalize(NormalizationForm.FormC);
    }

    internal static string NormalizeRequiredLookupValue(string value)
    {
        return NormalizeRequiredDisplayValue(value).ToUpperInvariant();
    }

    internal static string? NormalizeOptionalDisplayValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim().Normalize(NormalizationForm.FormC);
    }

    internal static string? NormalizeOptionalLookupValue(string? value)
    {
        string? normalizedValue = NormalizeOptionalDisplayValue(value);

        return normalizedValue?.ToUpperInvariant();
    }
}
