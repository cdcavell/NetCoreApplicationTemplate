using Serilog;
using Template.Web.Extensions;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Debug(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [Bootstrap] {Message:lj}{NewLine}{Exception}")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [Bootstrap] {Message:lj}{NewLine}{Exception}")
    .CreateBootstrapLogger();

try
{
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

    builder.AddTemplateSerilog();
    Log.Information("Bootstrapping Template.Web application");

    builder.Services.AddTemplateSecurityHeaders(builder.Configuration);
    builder.Services.AddTemplateRateLimiting();

    Log.Information("Starting Template.Web application");
    WebApplication app = builder.Build();

    Log.Information("Configuring pipline for Template.Web application");
    app.UseTemplatePipeline();

    app.MapGet("/", () => "Hello World!");

    Log.Information("Running Template.Web application");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Template.Web application terminated unexpectedly");
    Environment.ExitCode = 1;
}
finally
{
    Log.CloseAndFlush();
}








