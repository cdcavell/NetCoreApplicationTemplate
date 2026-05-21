namespace ProjectTemplate.Web.Models;

/// <summary>
/// Represents the baseline login page model.
/// </summary>
public sealed class AccountLoginViewModel
{
    /// <summary>
    /// Gets or sets the local return URL used after a successful authentication flow.
    /// </summary>
    public string ReturnUrl { get; set; } = "/";

    /// <summary>
    /// Gets or sets the external authentication providers available for challenge.
    /// </summary>
    public IReadOnlyList<ExternalAuthenticationProviderViewModel> ExternalProviders { get; set; } = [];
}
