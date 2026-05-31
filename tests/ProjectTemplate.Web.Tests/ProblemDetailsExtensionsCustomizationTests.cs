using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectTemplate.Web.ErrorHandling;
using ProjectTemplate.Web.Options;

namespace ProjectTemplate.Web.Tests;

public sealed class ProblemDetailsExtensionsCustomizationTests
{
    [Fact]
    public void AddApplicationProblemDetails_ProductionServerError_AddsGenericDetailAndTraceExtensions()
    {
        ServiceCollection services = CreateServices(
            Environments.Production,
            correlationHeaderName: "X-Correlation-ID");

        using ServiceProvider provider = services.BuildServiceProvider();

        ProblemDetailsOptions options = provider
            .GetRequiredService<IOptions<ProblemDetailsOptions>>()
            .Value;

        DefaultHttpContext httpContext = new()
        {
            RequestServices = provider,
            TraceIdentifier = "trace-250"
        };

        httpContext.Request.Path = "/current-path";
        httpContext.Request.Headers["X-Correlation-ID"] = $"  {new string('c', 140)}  ";

        httpContext.Features.Set<IExceptionHandlerPathFeature>(
            new TestExceptionHandlerPathFeature
            {
                Path = "/original-exception-path",
                Error = new InvalidOperationException("Test exception")
            });

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError
        };

        options.CustomizeProblemDetails!(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });

        Assert.Equal("/original-exception-path", problemDetails.Instance);
        Assert.Equal("An unexpected error occurred. Contact support with the request ID.", problemDetails.Detail);
        Assert.Equal("trace-250", problemDetails.Extensions["traceId"]);
        Assert.Equal("trace-250", problemDetails.Extensions["requestId"]);

        string correlationId = Assert.IsType<string>(problemDetails.Extensions["correlationId"]);
        Assert.Equal(128, correlationId.Length);
        Assert.All(correlationId, character => Assert.Equal('c', character));
    }

    [Fact]
    public void AddApplicationProblemDetails_ProductionClientError_UsesStatusCodeOriginalPathWithoutGenericDetail()
    {
        ServiceCollection services = CreateServices(
            Environments.Production,
            correlationHeaderName: "X-Correlation-ID");

        using ServiceProvider provider = services.BuildServiceProvider();

        ProblemDetailsOptions options = provider
            .GetRequiredService<IOptions<ProblemDetailsOptions>>()
            .Value;

        DefaultHttpContext httpContext = new()
        {
            RequestServices = provider,
            TraceIdentifier = "trace-404"
        };

        httpContext.Request.Path = "/current-not-found-path";

        httpContext.Features.Set<IStatusCodeReExecuteFeature>(
            new TestStatusCodeReExecuteFeature
            {
                OriginalPath = "/original-status-code-path",
                OriginalPathBase = string.Empty,
                OriginalQueryString = "?id=404"
            });

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status404NotFound
        };

        options.CustomizeProblemDetails!(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });

        Assert.Equal("/original-status-code-path", problemDetails.Instance);
        Assert.Null(problemDetails.Detail);
        Assert.Equal("trace-404", problemDetails.Extensions["traceId"]);
        Assert.Equal("trace-404", problemDetails.Extensions["requestId"]);
        Assert.Equal("trace-404", problemDetails.Extensions["correlationId"]);
    }

    [Fact]
    public void AddApplicationProblemDetails_DevelopmentServerError_DoesNotAddGenericDetail()
    {
        ServiceCollection services = CreateServices(
            Environments.Development,
            correlationHeaderName: "X-Correlation-ID");

        using ServiceProvider provider = services.BuildServiceProvider();

        ProblemDetailsOptions options = provider
            .GetRequiredService<IOptions<ProblemDetailsOptions>>()
            .Value;

        DefaultHttpContext httpContext = new()
        {
            RequestServices = provider,
            TraceIdentifier = "trace-dev"
        };

        httpContext.Request.Path = "/development-path";

        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError
        };

        options.CustomizeProblemDetails!(
            new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problemDetails
            });

        Assert.Equal("/development-path", problemDetails.Instance);
        Assert.Null(problemDetails.Detail);
        Assert.Equal("trace-dev", problemDetails.Extensions["traceId"]);
        Assert.Equal("trace-dev", problemDetails.Extensions["requestId"]);
        Assert.Equal("trace-dev", problemDetails.Extensions["correlationId"]);
    }

    [Fact]
    public void AddApplicationProblemDetails_WithCurrentActivity_AddsActivityTraceAndSpan()
    {
        ServiceCollection services = CreateServices(
            Environments.Production,
            correlationHeaderName: "X-Correlation-ID");

        using ServiceProvider provider = services.BuildServiceProvider();

        ProblemDetailsOptions options = provider
            .GetRequiredService<IOptions<ProblemDetailsOptions>>()
            .Value;

        DefaultHttpContext httpContext = new()
        {
            RequestServices = provider,
            TraceIdentifier = "trace-activity"
        };

        httpContext.Request.Path = "/activity-path";

        using Activity activity = new("ProblemDetailsTestActivity");
        activity.Start();

        try
        {
            ProblemDetails problemDetails = new()
            {
                Status = StatusCodes.Status400BadRequest
            };

            options.CustomizeProblemDetails!(
                new ProblemDetailsContext
                {
                    HttpContext = httpContext,
                    ProblemDetails = problemDetails
                });

            Assert.Equal(activity.TraceId.ToString(), problemDetails.Extensions["traceId"]);
            Assert.Equal(activity.SpanId.ToString(), problemDetails.Extensions["spanId"]);
            Assert.Equal("trace-activity", problemDetails.Extensions["requestId"]);
        }
        finally
        {
            activity.Stop();
        }
    }

    private static ServiceCollection CreateServices(
        string environmentName,
        string correlationHeaderName)
    {
        ServiceCollection services = new();

        _ = services.Configure<ApplicationRequestLoggingOptions>(
            options => options.CorrelationHeaderName = correlationHeaderName);

        _ = services.AddApplicationProblemDetails(
            new TestWebHostEnvironment
            {
                EnvironmentName = environmentName,
                ApplicationName = "ProjectTemplate.Web.Tests"
            });

        return services;
    }

    private sealed class TestExceptionHandlerPathFeature : IExceptionHandlerPathFeature
    {
        public Exception Error { get; set; } = new InvalidOperationException();

        public string Path { get; set; } = string.Empty;

        public Endpoint? Endpoint { get; set; }

        public RouteValueDictionary? RouteValues { get; set; }
    }

    private sealed class TestStatusCodeReExecuteFeature : IStatusCodeReExecuteFeature
    {
        public string OriginalPathBase { get; set; } = string.Empty;

        public string OriginalPath { get; set; } = string.Empty;

        public string? OriginalQueryString { get; set; }
    }

    private sealed class TestWebHostEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = string.Empty;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();

        public string WebRootPath { get; set; } = string.Empty;

        public string EnvironmentName { get; set; } = Environments.Production;

        public string ContentRootPath { get; set; } = string.Empty;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
