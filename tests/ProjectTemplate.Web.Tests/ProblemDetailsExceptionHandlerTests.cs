using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using ProjectTemplate.Web.ErrorHandling;

namespace ProjectTemplate.Web.Tests;

/// <summary>
/// Provides contract tests for centralized Problem Details exception handling.
/// </summary>
public sealed class ProblemDetailsExceptionHandlerTests
{
    public static TheoryData<ProblemDetailsExceptionCase, int, string> ExceptionMappings =>
        new()
        {
            { ProblemDetailsExceptionCase.BadHttpRequest, StatusCodes.Status400BadRequest, "Bad Request" },
            { ProblemDetailsExceptionCase.Argument, StatusCodes.Status500InternalServerError, "Internal Server Error" },
            { ProblemDetailsExceptionCase.UnauthorizedAccess, StatusCodes.Status403Forbidden, "Forbidden" },
            { ProblemDetailsExceptionCase.Timeout, StatusCodes.Status503ServiceUnavailable, "Service Unavailable" },
            { ProblemDetailsExceptionCase.Unknown, StatusCodes.Status500InternalServerError, "Internal Server Error" }
        };

    /// <summary>
    /// Verifies that supported exception types are mapped to the expected status code and title.
    /// </summary>
    [Theory]
    [MemberData(nameof(ExceptionMappings))]
    public async Task TryHandleAsync_ExceptionMappings_WriteExpectedProblemDetails(
        ProblemDetailsExceptionCase exceptionCase,
        int expectedStatusCode,
        string expectedTitle)
    {
        Exception exception = CreateException(exceptionCase);

        CapturingProblemDetailsService problemDetailsService = new();
        ProblemDetailsExceptionHandler handler = CreateHandler(
            problemDetailsService,
            Environments.Production);

        DefaultHttpContext httpContext = CreateProblemDetailsHttpContext();

        bool handled = await handler.TryHandleAsync(
            httpContext,
            exception,
            TestContext.Current.CancellationToken);

        Assert.True(handled);
        Assert.NotNull(problemDetailsService.Context);
        Assert.Equal(expectedStatusCode, httpContext.Response.StatusCode);

        ProblemDetails problemDetails = problemDetailsService.Context.ProblemDetails;

        Assert.Equal(expectedStatusCode, problemDetails.Status);
        Assert.Equal(expectedTitle, problemDetails.Title);
        Assert.Equal($"https://httpstatuses.com/{expectedStatusCode}", problemDetails.Type);
        Assert.Equal("/api/test/problem-details", problemDetails.Instance);
        Assert.True(problemDetails.Extensions.ContainsKey("traceId"));
        Assert.True(problemDetails.Extensions.ContainsKey("requestId"));
    }

    /// <summary>
    /// Verifies that unknown production exceptions use a generic server-error detail.
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_UnknownProductionException_DoesNotExposeRawExceptionDetail()
    {
        const string SensitiveMessage = "Sensitive database password leak marker.";

        CapturingProblemDetailsService problemDetailsService = new();
        ProblemDetailsExceptionHandler handler = CreateHandler(
            problemDetailsService,
            Environments.Production);

        DefaultHttpContext httpContext = CreateProblemDetailsHttpContext();

        bool handled = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException(SensitiveMessage),
            TestContext.Current.CancellationToken);

        Assert.True(handled);
        Assert.NotNull(problemDetailsService.Context);

        ProblemDetails problemDetails = problemDetailsService.Context.ProblemDetails;

        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("Internal Server Error", problemDetails.Title);
        Assert.Equal(
            "An unexpected error occurred. Contact support with the request ID.",
            problemDetails.Detail);
        Assert.DoesNotContain(SensitiveMessage, problemDetails.Detail, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that production request-level client-error responses do not expose raw exception messages.
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_ProductionBadHttpRequestException_DoesNotExposeRawExceptionDetail()
    {
        const string SensitiveMessage = "Sensitive request parser leak marker.";

        CapturingProblemDetailsService problemDetailsService = new();
        ProblemDetailsExceptionHandler handler = CreateHandler(
            problemDetailsService,
            Environments.Production);

        DefaultHttpContext httpContext = CreateProblemDetailsHttpContext();

        bool handled = await handler.TryHandleAsync(
            httpContext,
            new BadHttpRequestException(SensitiveMessage, StatusCodes.Status400BadRequest),
            TestContext.Current.CancellationToken);

        Assert.True(handled);
        Assert.NotNull(problemDetailsService.Context);

        ProblemDetails problemDetails = problemDetailsService.Context.ProblemDetails;

        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
        Assert.Equal("Bad Request", problemDetails.Title);
        Assert.DoesNotContain(
            SensitiveMessage,
            problemDetails.Detail ?? string.Empty,
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that production ArgumentException responses are treated as server errors and hide raw details.
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_ProductionArgumentException_IsServerErrorAndDoesNotExposeRawExceptionDetail()
    {
        const string SensitiveMessage = "Sensitive internal argument failure marker.";

        CapturingProblemDetailsService problemDetailsService = new();
        ProblemDetailsExceptionHandler handler = CreateHandler(
            problemDetailsService,
            Environments.Production);

        DefaultHttpContext httpContext = CreateProblemDetailsHttpContext();

        bool handled = await handler.TryHandleAsync(
            httpContext,
            new ArgumentException(SensitiveMessage),
            TestContext.Current.CancellationToken);

        Assert.True(handled);
        Assert.NotNull(problemDetailsService.Context);

        ProblemDetails problemDetails = problemDetailsService.Context.ProblemDetails;

        Assert.Equal(StatusCodes.Status500InternalServerError, problemDetails.Status);
        Assert.Equal("Internal Server Error", problemDetails.Title);
        Assert.Equal(
            "An unexpected error occurred. Contact support with the request ID.",
            problemDetails.Detail);
        Assert.DoesNotContain(SensitiveMessage, problemDetails.Detail, StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that development responses include the exception message for diagnostics.
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_DevelopmentEnvironment_IncludesExceptionMessage()
    {
        const string DevelopmentMessage = "Development diagnostic detail.";

        CapturingProblemDetailsService problemDetailsService = new();
        ProblemDetailsExceptionHandler handler = CreateHandler(
            problemDetailsService,
            Environments.Development);

        DefaultHttpContext httpContext = CreateProblemDetailsHttpContext();

        bool handled = await handler.TryHandleAsync(
            httpContext,
            new ArgumentException(DevelopmentMessage),
            TestContext.Current.CancellationToken);

        Assert.True(handled);
        Assert.NotNull(problemDetailsService.Context);
        Assert.Equal(DevelopmentMessage, problemDetailsService.Context.ProblemDetails.Detail);
    }

    /// <summary>
    /// Verifies that non-Problem-Details requests are ignored by the handler.
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_NonProblemDetailsRequest_ReturnsFalse()
    {
        CapturingProblemDetailsService problemDetailsService = new();
        ProblemDetailsExceptionHandler handler = CreateHandler(
            problemDetailsService,
            Environments.Production);

        DefaultHttpContext httpContext = new();
        httpContext.Request.Path = "/home/error";

        bool handled = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("Non API error."),
            TestContext.Current.CancellationToken);

        Assert.False(handled);
        Assert.Null(problemDetailsService.Context);
    }

    /// <summary>
    /// Verifies that the handler does not attempt to write after the response has started.
    /// </summary>
    [Fact]
    public async Task TryHandleAsync_ResponseAlreadyStarted_ReturnsFalse()
    {
        CapturingProblemDetailsService problemDetailsService = new();
        ProblemDetailsExceptionHandler handler = CreateHandler(
            problemDetailsService,
            Environments.Production);

        DefaultHttpContext httpContext = CreateStartedResponseProblemDetailsHttpContext();

        bool handled = await handler.TryHandleAsync(
            httpContext,
            new InvalidOperationException("Started response error."),
            TestContext.Current.CancellationToken);

        Assert.False(handled);
        Assert.Null(problemDetailsService.Context);
    }

    private static ProblemDetailsExceptionHandler CreateHandler(
        CapturingProblemDetailsService problemDetailsService,
        string environmentName)
    {
        return new ProblemDetailsExceptionHandler(
            problemDetailsService,
            new TestWebHostEnvironment(environmentName),
            NullLogger<ProblemDetailsExceptionHandler>.Instance);
    }

    private static DefaultHttpContext CreateProblemDetailsHttpContext()
    {
        DefaultHttpContext httpContext = new();

        httpContext.Request.Method = HttpMethods.Get;
        httpContext.Request.Path = "/api/test/problem-details";
        httpContext.TraceIdentifier = "test-request-id";

        return httpContext;
    }

    private sealed class CapturingProblemDetailsService : IProblemDetailsService
    {
        public ProblemDetailsContext? Context { get; private set; }

        public ValueTask WriteAsync(ProblemDetailsContext context)
        {
            Context = context;

            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryWriteAsync(ProblemDetailsContext context)
        {
            Context = context;

            return ValueTask.FromResult(true);
        }
    }
    private sealed class TestWebHostEnvironment(string environmentName) : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "ProjectTemplate.Web.Tests";

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public string EnvironmentName { get; set; } = environmentName;

        public string WebRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
    }

    private static Exception CreateException(ProblemDetailsExceptionCase exceptionCase)
    {
        return exceptionCase switch
        {
            ProblemDetailsExceptionCase.BadHttpRequest => new BadHttpRequestException(
                "Bad HTTP request contract test detail.",
                StatusCodes.Status400BadRequest),

            ProblemDetailsExceptionCase.Argument => new ArgumentException(
                "Invalid argument contract test detail."),

            ProblemDetailsExceptionCase.UnauthorizedAccess => new UnauthorizedAccessException(
                "Forbidden contract test detail."),

            ProblemDetailsExceptionCase.Timeout => new TimeoutException(
                "Timeout contract test detail."),

            ProblemDetailsExceptionCase.Unknown => new InvalidOperationException(
                "Internal failure contract test detail."),

            _ => throw new ArgumentOutOfRangeException(nameof(exceptionCase), exceptionCase, null)
        };
    }

    public enum ProblemDetailsExceptionCase
    {
        BadHttpRequest,
        Argument,
        UnauthorizedAccess,
        Timeout,
        Unknown
    }

    private static DefaultHttpContext CreateStartedResponseProblemDetailsHttpContext()
    {
        FeatureCollection features = new();

        features.Set<IHttpRequestFeature>(new HttpRequestFeature
        {
            Method = HttpMethods.Get,
            Path = "/api/test/problem-details"
        });

        features.Set<IHttpResponseFeature>(new StartedResponseFeature());

        DefaultHttpContext httpContext = new(features)
        {
            TraceIdentifier = "test-request-id"
        };

        return httpContext;
    }

    private sealed class StartedResponseFeature : HttpResponseFeature
    {
        public override bool HasStarted => true;
    }
}
