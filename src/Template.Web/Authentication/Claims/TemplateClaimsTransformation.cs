using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Template.Web.Authentication.Options;

namespace Template.Web.Authentication.Claims;

/// <summary>
/// Normalizes provider-specific claims into template-owned claim names.
/// </summary>
/// <param name="authenticationOptionsAccessor">The template authentication options accessor.</param>
public sealed class TemplateClaimsTransformation(
    IOptions<TemplateAuthenticationOptions> authenticationOptionsAccessor) : IClaimsTransformation
{
    private readonly IOptions<TemplateAuthenticationOptions> _authenticationOptionsAccessor =
        authenticationOptionsAccessor ?? throw new ArgumentNullException(nameof(authenticationOptionsAccessor));

    /// <inheritdoc />
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        TemplateClaimsTransformationOptions options =
            _authenticationOptionsAccessor.Value.ClaimsTransformation;

        if (!options.Enabled)
        {
            return Task.FromResult(principal);
        }

        foreach (ClaimsIdentity identity in principal.Identities.OfType<ClaimsIdentity>())
        {
            TemplateClaimMappingOptions mappings = ResolveMappings(options, identity.AuthenticationType);

            NormalizeClaim(identity, TemplateClaimTypes.Subject, mappings.Subject, options.RemoveOriginalClaims);
            NormalizeClaim(identity, TemplateClaimTypes.Name, mappings.Name, options.RemoveOriginalClaims);
            NormalizeClaim(identity, TemplateClaimTypes.Email, mappings.Email, options.RemoveOriginalClaims);
            NormalizeClaim(identity, TemplateClaimTypes.Role, mappings.Role, options.RemoveOriginalClaims);
            NormalizeClaim(identity, TemplateClaimTypes.Group, mappings.Group, options.RemoveOriginalClaims);
            NormalizeClaim(identity, TemplateClaimTypes.Permission, mappings.Permission, options.RemoveOriginalClaims);
        }

        return Task.FromResult(principal);
    }

    private static TemplateClaimMappingOptions ResolveMappings(
        TemplateClaimsTransformationOptions options,
        string? authenticationType)
    {
        return !string.IsNullOrWhiteSpace(authenticationType)
            && options.ProviderMappings.TryGetValue(authenticationType, out TemplateClaimMappingOptions? providerMappings)
            ? providerMappings
            : options.DefaultMappings;
    }

    private static void NormalizeClaim(
        ClaimsIdentity identity,
        string normalizedClaimType,
        IEnumerable<string> sourceClaimTypes,
        bool removeOriginalClaims)
    {
        var sourceTypes = sourceClaimTypes
            .Where(claimType => !string.IsNullOrWhiteSpace(claimType))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (sourceTypes.Count == 0)
        {
            return;
        }

        var sourceClaims = identity.Claims
            .Where(claim => sourceTypes.Contains(claim.Type))
            .ToList();

        if (sourceClaims.Count == 0)
        {
            return;
        }

        foreach (Claim sourceClaim in sourceClaims)
        {
            if (!HasClaim(identity, normalizedClaimType, sourceClaim.Value))
            {
                identity.AddClaim(new Claim(
                    normalizedClaimType,
                    sourceClaim.Value,
                    sourceClaim.ValueType,
                    sourceClaim.Issuer,
                    sourceClaim.OriginalIssuer));
            }
        }

        if (!removeOriginalClaims)
        {
            return;
        }

        foreach (Claim sourceClaim in sourceClaims)
        {
            if (!string.Equals(sourceClaim.Type, normalizedClaimType, StringComparison.OrdinalIgnoreCase))
            {
                identity.RemoveClaim(sourceClaim);
            }
        }
    }

    private static bool HasClaim(
        ClaimsIdentity identity,
        string claimType,
        string claimValue)
    {
        return identity.Claims.Any(claim =>
            string.Equals(claim.Type, claimType, StringComparison.OrdinalIgnoreCase)
            && string.Equals(claim.Value, claimValue, StringComparison.Ordinal));
    }
}
