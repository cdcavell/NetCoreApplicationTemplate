using Template.Web.Constants;

namespace Template.Web.Extensions;

/// <summary>
/// Provides extension methods to register template authorization policies.
/// </summary>
public static class AuthorizationServiceExtensions
{
    /// <summary>
    /// Adds the template authorization policy baseline.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The same service collection instance for chaining.</returns>
    public static IServiceCollection AddTemplateAuthorization(this IServiceCollection services)
    {
        services.AddAuthorizationBuilder()
            .AddPolicy(TemplateAuthorizationPolicyNames.AuthenticatedUser, policy => policy.RequireAuthenticatedUser());

        return services;
    }
}
