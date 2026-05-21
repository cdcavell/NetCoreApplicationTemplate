using System.Diagnostics;

namespace ProjectTemplate.Web.ErrorHandling;

/// <summary>
/// Provides extension methods for configuring standardized problem details and exception handling in ASP.NET Core
/// applications.
/// </summary>
/// <remarks>These extensions integrate custom problem details formatting and exception handling into the
/// application's dependency injection and middleware pipelines. They help ensure consistent error responses and
/// traceability across environments.</remarks>
internal static class ProblemDetailsExtensions
{
    /// <summary>
    /// Adds standardized Problem Details error responses and a custom exception handler to the application's service
    /// collection.
    /// </summary>
    /// <remarks>This method configures Problem Details middleware to include trace and request identifiers in
    /// error responses and customizes error details for non-development environments. It also registers a custom
    /// exception handler for consistent error formatting.</remarks>
    /// <param name="services">The service collection to which Problem Details and the exception handler are added. Cannot be null.</param>
    /// <param name="webHostEnvironment">The current web hosting environment. Used to determine error detail behavior based on environment. Cannot be
    /// null.</param>
    /// <returns>The same service collection instance, enabling method chaining.</returns>
    public static IServiceCollection AddApplicationProblemDetails(
        this IServiceCollection services,
        IWebHostEnvironment webHostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(webHostEnvironment);

        services.AddExceptionHandler<ProblemDetailsExceptionHandler>();

        services.AddProblemDetails(options => options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance ??= context.HttpContext.Request.Path;

                context.ProblemDetails.Extensions["traceId"] =
                    Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

                context.ProblemDetails.Extensions["requestId"] =
                    context.HttpContext.TraceIdentifier;

                if (!webHostEnvironment.IsDevelopment() &&
                    context.ProblemDetails.Status >= StatusCodes.Status500InternalServerError)
                {
                    context.ProblemDetails.Detail ??=
                        "An unexpected error occurred. Contact support with the request ID.";
                }
            });

        return services;
    }

    /// <summary>
    /// Configures standardized error handling and problem details middleware for the application based on the current
    /// environment.
    /// </summary>
    /// <remarks>In the development environment, this method enables the developer exception page. In other
    /// environments, it configures a generic exception handler and enforces HTTP Strict Transport Security (HSTS). It
    /// also sets up status code pages to return problem details responses when appropriate, or redirects to a custom
    /// error page otherwise.</remarks>
    /// <param name="app">The <see cref="WebApplication"/> instance to configure. Cannot be null.</param>
    /// <returns>The configured <see cref="WebApplication"/> instance.</returns>
    public static WebApplication UseProblemDetails(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error/500");
            app.UseHsts();
        }

        app.UseWhen(
            ProblemDetailsRequestClassifier.ShouldWriteProblemDetails,
            branch => branch.UseStatusCodePages());

        app.UseWhen(
            context => !ProblemDetailsRequestClassifier.ShouldWriteProblemDetails(context),
            branch => branch.UseStatusCodePagesWithReExecute("/Home/Error/{0}"));

        return app;
    }
}
