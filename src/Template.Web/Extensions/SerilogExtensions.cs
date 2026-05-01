using Serilog;

namespace Template.Web.Extensions
{
    public static class SerilogExtensions
    {
        public static WebApplicationBuilder AddTemplateSerilog(this WebApplicationBuilder builder)
        {
            builder.Services.AddSerilog((services, loggerConfiguration) =>
            {
                loggerConfiguration
                    .ReadFrom.Configuration(builder.Configuration)
                    .ReadFrom.Services(services)
                    .Enrich.FromLogContext();
            });

            return builder;
        }
    }
}
