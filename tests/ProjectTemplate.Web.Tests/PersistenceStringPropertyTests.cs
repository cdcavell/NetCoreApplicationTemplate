using FsCheck;
using FsCheck.Fluent;
using ProjectTemplate.Infrastructure.Data;

namespace ProjectTemplate.Web.Tests;

/// <summary>
/// Contains property-based tests for persistence string normalization behavior.
/// </summary>
public sealed class PersistenceStringPropertyTests
{
    /// <summary>
    /// Verifies that required display-value normalization is idempotent for generated strings.
    /// </summary>
    [Fact]
    public void NormalizeRequiredDisplayValue_IsIdempotentForGeneratedStrings()
    {
        Prop.ForAll<string>(value =>
        {
            string input = value ?? string.Empty;
            string once = PersistenceStringComparisonNormalizer.NormalizeRequiredDisplayValue(input);
            string twice = PersistenceStringComparisonNormalizer.NormalizeRequiredDisplayValue(once);

            return string.Equals(once, twice, StringComparison.Ordinal);
        }).QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Verifies that optional display-value normalization is idempotent for generated strings.
    /// </summary>
    [Fact]
    public void NormalizeOptionalDisplayValue_IsIdempotentForGeneratedStrings()
    {
        Prop.ForAll<string>(value =>
        {
            string? once = PersistenceStringComparisonNormalizer.NormalizeOptionalDisplayValue(value);
            string? twice = PersistenceStringComparisonNormalizer.NormalizeOptionalDisplayValue(once);

            return string.Equals(once, twice, StringComparison.Ordinal);
        }).QuickCheckThrowOnFailure();
    }

    /// <summary>
    /// Verifies that persistence canonicalization stabilizes after the first completed pass.
    /// </summary>
    [Fact]
    public void Canonicalize_IsIdempotentForGeneratedStrings()
    {
        Prop.ForAll<string>(value =>
        {
            string input = value ?? string.Empty;
            string once = PersistenceStringCanonicalizer.Canonicalize(input);
            string twice = PersistenceStringCanonicalizer.Canonicalize(once);

            return string.Equals(once, twice, StringComparison.Ordinal);
        }).QuickCheckThrowOnFailure();
    }
}
