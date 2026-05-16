using Microsoft.Extensions.Options;
using Template.Web.Authentication.Providers.OpenIdConnect;
using Template.Web.Authentication.Providers.Saml2;

namespace Template.Web.Authentication.Options;

/// <summary>
/// Validates template authentication configuration during application startup.
/// </summary>
public sealed class TemplateAuthenticationOptionsValidator : IValidateOptions<TemplateAuthenticationOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, TemplateAuthenticationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        List<string> failures = [];

        ValidateTemplateAuthentication(options, failures);
        ValidateOpenIdConnectProvider(options.Providers.OpenIdConnect, failures);
        ValidateSaml2Provider(options.Providers.Saml2, failures);
        ValidateExternalProvider("Microsoft", options.Providers.Microsoft, failures);
        ValidateExternalProvider("Google", options.Providers.Google, failures);
        ValidateExternalProvider("GitHub", options.Providers.GitHub, failures);

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }

    private static void ValidateTemplateAuthentication(
        TemplateAuthenticationOptions options,
        ICollection<string> failures)
    {
        Require(
            !string.IsNullOrWhiteSpace(options.DefaultScheme),
            "Template:Authentication:DefaultScheme is required.",
            failures);

        Require(
            !string.IsNullOrWhiteSpace(options.DefaultChallengeScheme),
            "Template:Authentication:DefaultChallengeScheme is required.",
            failures);

        Require(
            !string.IsNullOrWhiteSpace(options.DefaultSignInScheme),
            "Template:Authentication:DefaultSignInScheme is required.",
            failures);

        if (!options.Enabled)
        {
            return;
        }

        Require(
            options.Cookie.Enabled,
            "Template:Authentication:Cookie:Enabled must be true when template authentication is enabled.",
            failures);

        Require(
            !string.IsNullOrWhiteSpace(options.Cookie.Scheme),
            "Template:Authentication:Cookie:Scheme is required when template authentication is enabled.",
            failures);

        Require(
            options.Cookie.ExpireMinutes > 0,
            "Template:Authentication:Cookie:ExpireMinutes must be greater than zero when template authentication is enabled.",
            failures);
    }

    private static void ValidateOpenIdConnectProvider(
        TemplateOpenIdConnectAuthenticationOptions options,
        ICollection<string> failures)
    {
        if (!options.Enabled)
        {
            return;
        }

        const string prefix = "Template:Authentication:Providers:OpenIdConnect";

        RequireProviderValue(prefix, nameof(options.Scheme), options.Scheme, failures);
        RequireProviderValue(prefix, nameof(options.DisplayName), options.DisplayName, failures);
        RequireProviderValue(prefix, nameof(options.Authority), options.Authority, failures);
        RequireProviderValue(prefix, nameof(options.ClientId), options.ClientId, failures);
        RequireProviderValue(prefix, nameof(options.CallbackPath), options.CallbackPath, failures);
        RequireProviderValue(prefix, nameof(options.ResponseType), options.ResponseType, failures);

        Require(
            options.Scopes.Any(scope => !string.IsNullOrWhiteSpace(scope)),
            $"{prefix}:Scopes must contain at least one non-empty value when the OpenID Connect provider is enabled.",
            failures);
    }

    private static void ValidateSaml2Provider(
        TemplateSaml2AuthenticationOptions options,
        ICollection<string> failures)
    {
        if (!options.Enabled)
        {
            return;
        }

        const string prefix = "Template:Authentication:Providers:Saml2";

        RequireProviderValue(prefix, nameof(options.Scheme), options.Scheme, failures);
        RequireProviderValue(prefix, nameof(options.DisplayName), options.DisplayName, failures);
        RequireProviderValue(prefix, nameof(options.EntityId), options.EntityId, failures);
        RequireProviderValue(prefix, nameof(options.MetadataUrl), options.MetadataUrl, failures);
        RequireProviderValue(prefix, nameof(options.ModulePath), options.ModulePath, failures);
    }

    private static void ValidateExternalProvider(
        string providerName,
        TemplateExternalAuthenticationProviderOptions options,
        ICollection<string> failures)
    {
        if (!options.Enabled)
        {
            return;
        }

        string prefix = $"Template:Authentication:Providers:{providerName}";

        RequireProviderValue(prefix, nameof(options.Scheme), options.Scheme, failures);
        RequireProviderValue(prefix, nameof(options.DisplayName), options.DisplayName, failures);
        RequireProviderValue(prefix, nameof(options.ClientId), options.ClientId, failures);
        RequireProviderValue(prefix, nameof(options.ClientSecret), options.ClientSecret, failures);
        RequireProviderValue(prefix, nameof(options.CallbackPath), options.CallbackPath, failures);
    }

    private static void RequireProviderValue(
        string prefix,
        string key,
        string? value,
        ICollection<string> failures)
    {
        Require(
            !string.IsNullOrWhiteSpace(value),
            $"{prefix}:{key} is required when the provider is enabled.",
            failures);
    }

    private static void Require(
        bool condition,
        string failureMessage,
        ICollection<string> failures)
    {
        if (!condition)
        {
            failures.Add(failureMessage);
        }
    }
}
