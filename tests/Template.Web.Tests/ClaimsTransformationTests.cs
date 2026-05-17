using System.Security.Claims;
using Template.Web.Authentication.Claims;
using Template.Web.Authentication.Options;

namespace Template.Web.Tests;

/// <summary>
/// Provides unit tests for template claims transformation and normalization behavior.
/// </summary>
public sealed class ClaimsTransformationTests
{
    /// <summary>
    /// Verifies that OpenID Connect-style claims are normalized into template-owned claim types.
    /// </summary>
    [Fact]
    public async Task ClaimsTransformation_NormalizesOidcStyleClaims()
    {
        TemplateClaimsTransformation transformation = CreateTransformation();

        ClaimsPrincipal principal = CreatePrincipal(
            authenticationType: "OpenIdConnect",
            claims:
            [
                new Claim("sub", "user-123"),
                new Claim("name", "Test User"),
                new Claim("email", "test.user@example.test"),
                new Claim("roles", "Administrator"),
                new Claim("groups", "Developers"),
                new Claim("scope", "template.read")
            ]);

        ClaimsPrincipal transformed = await transformation.TransformAsync(principal);

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Subject &&
            claim.Value == "user-123");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Name &&
            claim.Value == "Test User");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Email &&
            claim.Value == "test.user@example.test");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Role &&
            claim.Value == "Administrator");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Group &&
            claim.Value == "Developers");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Permission &&
            claim.Value == "template.read");
    }

    /// <summary>
    /// Verifies that SAML-style claims are normalized into template-owned claim types.
    /// </summary>
    [Fact]
    public async Task ClaimsTransformation_NormalizesSamlStyleClaims()
    {
        TemplateClaimsTransformation transformation = CreateTransformation();

        ClaimsPrincipal principal = CreatePrincipal(
            authenticationType: "Saml2",
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, "saml-user-123"),
                new Claim(ClaimTypes.Name, "Saml Test User"),
                new Claim(ClaimTypes.Email, "saml.user@example.test"),
                new Claim(ClaimTypes.Role, "Manager"),
                new Claim("memberOf", "Operations"),
                new Claim("permission", "template.write")
            ]);

        ClaimsPrincipal transformed = await transformation.TransformAsync(principal);

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Subject &&
            claim.Value == "saml-user-123");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Name &&
            claim.Value == "Saml Test User");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Email &&
            claim.Value == "saml.user@example.test");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Role &&
            claim.Value == "Manager");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Group &&
            claim.Value == "Operations");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Permission &&
            claim.Value == "template.write");
    }

    /// <summary>
    /// Verifies that original provider claims are preserved by default.
    /// </summary>
    [Fact]
    public async Task ClaimsTransformation_PreservesOriginalClaimsByDefault()
    {
        TemplateClaimsTransformation transformation = CreateTransformation();

        ClaimsPrincipal principal = CreatePrincipal(
            authenticationType: "OpenIdConnect",
            claims:
            [
                new Claim("sub", "user-123")
            ]);

        ClaimsPrincipal transformed = await transformation.TransformAsync(principal);

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == "sub" &&
            claim.Value == "user-123");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Subject &&
            claim.Value == "user-123");
    }

    /// <summary>
    /// Verifies that original provider claims are removed when configured explicitly.
    /// </summary>
    [Fact]
    public async Task ClaimsTransformation_RemovesOriginalClaims_WhenConfigured()
    {
        TemplateClaimsTransformation transformation = CreateTransformation(new TemplateClaimsTransformationOptions
        {
            Enabled = true,
            RemoveOriginalClaims = true
        });

        ClaimsPrincipal principal = CreatePrincipal(
            authenticationType: "OpenIdConnect",
            claims:
            [
                new Claim("sub", "user-123")
            ]);

        ClaimsPrincipal transformed = await transformation.TransformAsync(principal);

        Assert.DoesNotContain(transformed.Claims, claim =>
            claim.Type == "sub" &&
            claim.Value == "user-123");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Subject &&
            claim.Value == "user-123");
    }

    /// <summary>
    /// Verifies that claims are not normalized when claims transformation is disabled.
    /// </summary>
    [Fact]
    public async Task ClaimsTransformation_DoesNotAddNormalizedClaims_WhenDisabled()
    {
        TemplateClaimsTransformation transformation = CreateTransformation(new TemplateClaimsTransformationOptions
        {
            Enabled = false
        });

        ClaimsPrincipal principal = CreatePrincipal(
            authenticationType: "OpenIdConnect",
            claims:
            [
                new Claim("sub", "user-123")
            ]);

        ClaimsPrincipal transformed = await transformation.TransformAsync(principal);

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == "sub" &&
            claim.Value == "user-123");

        Assert.DoesNotContain(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Subject &&
            claim.Value == "user-123");
    }

    /// <summary>
    /// Verifies that provider-specific mappings override the default mappings for the matching authentication type.
    /// </summary>
    [Fact]
    public async Task ClaimsTransformation_UsesProviderSpecificMappings_WhenConfigured()
    {
        TemplateClaimsTransformation transformation = CreateTransformation(new TemplateClaimsTransformationOptions
        {
            Enabled = true,
            ProviderMappings =
            {
                ["GitHub"] = new TemplateClaimMappingOptions
                {
                    Subject = [ "github_id" ],
                    Name = [ "github_name" ],
                    Email = [ "github_email" ],
                    Role = [ "github_role" ],
                    Group = [ "github_group" ],
                    Permission = [ "github_permission" ]
                }
            }
        });

        ClaimsPrincipal principal = CreatePrincipal(
            authenticationType: "GitHub",
            claims:
            [
                new Claim("github_id", "github-user-123"),
                new Claim("github_name", "GitHub Test User"),
                new Claim("github_email", "github.user@example.test"),
                new Claim("github_role", "Maintainer"),
                new Claim("github_group", "Template Team"),
                new Claim("github_permission", "repository.read")
            ]);

        ClaimsPrincipal transformed = await transformation.TransformAsync(principal);

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Subject &&
            claim.Value == "github-user-123");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Name &&
            claim.Value == "GitHub Test User");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Email &&
            claim.Value == "github.user@example.test");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Role &&
            claim.Value == "Maintainer");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Group &&
            claim.Value == "Template Team");

        Assert.Contains(transformed.Claims, claim =>
            claim.Type == TemplateClaimTypes.Permission &&
            claim.Value == "repository.read");
    }

    /// <summary>
    /// Verifies that duplicate normalized claims are not added when transformation runs more than once.
    /// </summary>
    [Fact]
    public async Task ClaimsTransformation_DoesNotDuplicateNormalizedClaims()
    {
        TemplateClaimsTransformation transformation = CreateTransformation();

        ClaimsPrincipal principal = CreatePrincipal(
            authenticationType: "OpenIdConnect",
            claims:
            [
                new Claim("sub", "user-123")
            ]);

        ClaimsPrincipal transformedOnce = await transformation.TransformAsync(principal);
        ClaimsPrincipal transformedTwice = await transformation.TransformAsync(transformedOnce);

        int normalizedSubjectClaimCount = transformedTwice.Claims.Count(claim =>
            claim.Type == TemplateClaimTypes.Subject &&
            claim.Value == "user-123");

        Assert.Equal(1, normalizedSubjectClaimCount);
    }

    private static TemplateClaimsTransformation CreateTransformation(
        TemplateClaimsTransformationOptions? claimsTransformationOptions = null)
    {
        TemplateAuthenticationOptions authenticationOptions = new()
        {
            ClaimsTransformation = claimsTransformationOptions ?? new TemplateClaimsTransformationOptions()
        };

        return new TemplateClaimsTransformation(Microsoft.Extensions.Options.Options.Create(authenticationOptions));
    }

    private static ClaimsPrincipal CreatePrincipal(
        string authenticationType,
        IEnumerable<Claim> claims)
    {
        ClaimsIdentity identity = new(claims, authenticationType);

        return new ClaimsPrincipal(identity);
    }
}
