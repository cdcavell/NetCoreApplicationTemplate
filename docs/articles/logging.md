# Logging

This application uses Serilog for structured application logging.

Serilog is configured as the primary logging provider so that application events, startup events, errors, and HTTP request activity are written using a consistent structured format.

## Bootstrap Logging
The application logs a bootstrap message when the web application begins provider configuration:
```csharp
Log.Information("Bootstrapping ProjectTemplate.Web application");
```

## Startup Logging
The application logs a startup message when the web application begins initialization:
```csharp
Log.Information("Starting ProjectTemplate.Web application");
```

## Pipeline Logging 
The application logs a startup message when the web application begins configuring the middleware pipeline:
```csharp
Log.Information("Configuring pipeline for ProjectTemplate.Web application");
```

## Runtime Logging
The application logs a startup message when the web application begins running:
```csharp
Log.Information("Running ProjectTemplate.Web application");
```

## Ongoing Application Logging
While the application is running, structured logs are written for normal application activity, warnings, errors, and HTTP request processing. These logs help provide visibility into the current behavior of the application without requiring a debugger to be attached.

Runtime logging may include:

- Application lifecycle events
- Controller or endpoint activity
- HTTP request completion details
- Warnings from expected but noteworthy conditions
- Exceptions and unexpected failures
- Framework or infrastructure messages based on configured log levels

The default logging configuration is intended to capture useful operational information while avoiding sensitive data such as passwords, authentication tokens, cookies, request bodies, and response bodies.

Additional logging can be added throughout the application by injecting ILogger<T> into services, controllers, middleware, or other application components.

## Bootstrap Exception Logging
The application logs any exceptions that occur during the bootstrapping process or while configuring the middleware pipeline:
```csharp
Log.Fatal(ex, "ProjectTemplate.Web application terminated unexpectedly");
```
## Structured Request Logging

The application includes structured HTTP request logging through Serilog.

Request logging records a single completion event for each normal request and includes:

- HTTP method
- Request path
- Response status code
- Elapsed request duration
- Request ID
- Correlation ID
- Request scheme and host
- Remote IP address, when enabled
- Authenticated user name, when enabled

Request logging is configured through:

```csharp
builder.Services.AddApplicationRequestLogging(builder.Configuration);
```
And applied through the standard application pipeline:
```csharp
app.UseApplicationRequestLogging();
```
Configuration is controlled through `appsettings.json`:
```json
"ProjectTemplate": {
  "RequestLogging": {
    "Enabled": true,
    "CorrelationHeaderName": "X-Correlation-ID",
    "IncludeQueryString": false,
    "IncludeUserName": true,
    "IncludeRemoteIpAddress": true,
    "ExcludedPathPrefixes": [
      "/health",
      "/metrics",
      "/favicon.ico",
      "/css",
      "/js",
      "/lib",
      "/_framework"
    ]
  }
}
```
Query string logging is disabled by default because query strings may contain sensitive values. Applications should avoid logging request bodies, response bodies, cookies, authorization headers, access tokens, refresh tokens, or authentication payloads unless a specific, reviewed diagnostic need exists.
