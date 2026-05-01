using Serilog;

namespace Template.Web.Extensions;

/// <summary>
/// Extension methods for configuring Serilog for the web application.
/// </summary>
    public static class SerilogExtensions
    {
    /// <summary>
    /// Configures Serilog for the provided <see cref="WebApplicationBuilder"/>.
    /// Reads configuration from <see cref="WebApplicationBuilder.Configuration"/>, reads services,
    /// and enriches logs from the log context.
    /// </summary>
    /// <param name="builder">The web application builder to configure.</param>
    /// <returns>The same <see cref="WebApplicationBuilder"/> instance for chaining.</returns>
        public static WebApplicationBuilder AddTemplateSerilog(this WebApplicationBuilder builder)
        {
            builder.Services.AddSerilog((services, loggerConfiguration) =>
            _ = loggerConfiguration
                    .ReadFrom.Configuration(builder.Configuration)
                    .ReadFrom.Services(services)
                .Enrich.FromLogContext());

            return builder;
        }
    }
}
