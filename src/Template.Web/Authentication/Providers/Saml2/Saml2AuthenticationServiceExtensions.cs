using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Metadata;

namespace Template.Web.Authentication.Providers.Saml2;

/// <summary>
/// Provides extension methods for registering SAML2 authentication provider services.
/// </summary>
public static class Saml2AuthenticationServiceExtensions
{
    /// <summary>
    /// Adds SAML2 authentication using the specified template options to the authentication builder.
    /// </summary>
    /// <remarks>If the SAML2 authentication is not enabled in the provided options, the method returns the
    /// original builder without modification.</remarks>
    /// <param name="builder">The authentication builder to which the SAML2 authentication scheme is added.</param>
    /// <param name="options">The options used to configure the SAML2 authentication scheme. Cannot be null.</param>
    /// <returns>The authentication builder with the SAML2 authentication scheme configured.</returns>
    public static AuthenticationBuilder AddTemplateSaml2Authentication(
        this AuthenticationBuilder builder,
        TemplateSaml2AuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(options);

        if (!options.Enabled)
        {
            return builder;
        }

        builder.AddSaml2(options.Scheme, options.DisplayName, saml2Options =>
        {
            saml2Options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            saml2Options.SPOptions.EntityId = new EntityId(options.EntityId);
            saml2Options.SPOptions.ModulePath = options.ModulePath;

            IdentityProvider identityProvider = new(
                new EntityId(options.MetadataUrl),
                saml2Options.SPOptions)
            {
                MetadataLocation = options.MetadataUrl,
                LoadMetadata = options.LoadMetadata,
                AllowUnsolicitedAuthnResponse = false,
                WantAuthnRequestsSigned = options.RequireSignedAssertions
            };

            saml2Options.IdentityProviders.Add(identityProvider);
        });

        return builder;
    }
}
