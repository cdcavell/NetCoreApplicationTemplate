using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

namespace Template.Web.Extensions;

/// <summary>
/// Provides extension methods to register rate limiting services for the application.
/// </summary>
    public static class RateLimitingServiceExtensions
    {
    /// <summary>
    /// Adds the application's predefined rate limiting policies to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the rate limiting services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance so calls can be chained.</returns>
        public static IServiceCollection AddTemplateRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    await context.HttpContext.Response.WriteAsJsonAsync(new
                    {
                        error = "Too many requests.",
                        statusCode = StatusCodes.Status429TooManyRequests
                    }, cancellationToken);
                };

                options.AddFixedWindowLimiter("fixed", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 60;
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                options.AddConcurrencyLimiter("concurrency", limiterOptions =>
                {
                    limiterOptions.PermitLimit = 10;
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });
            });

            return services;
        }
    }
}
