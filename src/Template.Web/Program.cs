using Template.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTemplateSecurityHeaders(builder.Configuration);

var app = builder.Build();

app.UseTemplatePipeline();

app.MapGet("/", () => "Hello World!");

app.Run();
