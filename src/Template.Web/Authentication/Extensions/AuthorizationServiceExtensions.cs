namespace Template.Web.Authentication.Extensions;

/// <summary>
/// Provides extension methods for registering template authorization services.
/// </summary>
public static class AuthorizationServiceExtensions
{
    /// <summary>
    /// Adds template authorization services and baseline policies.
    /// </summary>
    /// <param name="services">The service collection to add authorization services to.</param>
    /// <param name="configuration">The application configuration source.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddTemplateAuthorization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services.AddAuthorizationBuilder()
            .AddPolicy(TemplateAuthorizationPolicyNames.AuthenticatedUser, policy => policy.RequireAuthenticatedUser());

        return services;
    }
}

/// <summary>
/// Provides template authorization policy names.
/// </summary>
public static class TemplateAuthorizationPolicyNames
{
    /// <summary>
    /// Policy requiring the current user to be authenticated.
    /// </summary>
    public const string AuthenticatedUser = "Template.AuthenticatedUser";
}
