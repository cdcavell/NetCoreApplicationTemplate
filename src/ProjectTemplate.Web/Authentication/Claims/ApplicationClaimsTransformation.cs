using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using ProjectTemplate.Web.Authentication.Options;

namespace ProjectTemplate.Web.Authentication.Claims;

/// <summary>
/// Normalizes provider-specific claims into application-owned claim names.
/// </summary>
/// <param name="authenticationOptionsAccessor">The application authentication options accessor.</param>
public sealed class ApplicationClaimsTransformation(
    IOptions<ApplicationAuthenticationOptions> authenticationOptionsAccessor) : IClaimsTransformation
{
    private readonly IOptions<ApplicationAuthenticationOptions> _authenticationOptionsAccessor =
        authenticationOptionsAccessor ?? throw new ArgumentNullException(nameof(authenticationOptionsAccessor));

    /// <inheritdoc />
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);

        ApplicationClaimsTransformationOptions options =
            _authenticationOptionsAccessor.Value.ClaimsTransformation;

        if (!options.Enabled)
        {
            return Task.FromResult(principal);
        }

        foreach (ClaimsIdentity identity in principal.Identities.OfType<ClaimsIdentity>())
        {
            ApplicationClaimMappingOptions mappings = ResolveMappings(options, identity.AuthenticationType);

            NormalizeClaim(identity, ApplicationClaimTypes.Subject, mappings.Subject, options.RemoveOriginalClaims);
            NormalizeClaim(identity, ApplicationClaimTypes.Name, mappings.Name, options.RemoveOriginalClaims);
            NormalizeClaim(identity, ApplicationClaimTypes.Email, mappings.Email, options.RemoveOriginalClaims);
            NormalizeClaim(identity, ApplicationClaimTypes.Role, mappings.Role, options.RemoveOriginalClaims);
            NormalizeClaim(identity, ApplicationClaimTypes.Group, mappings.Group, options.RemoveOriginalClaims);
            NormalizeClaim(identity, ApplicationClaimTypes.Permission, mappings.Permission, options.RemoveOriginalClaims);
        }

        return Task.FromResult(principal);
    }

    private static ApplicationClaimMappingOptions ResolveMappings(
        ApplicationClaimsTransformationOptions options,
        string? authenticationType)
    {
        return !string.IsNullOrWhiteSpace(authenticationType)
            && options.ProviderMappings.TryGetValue(authenticationType, out ApplicationClaimMappingOptions? providerMappings)
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
