# API Versioning

The application includes a foundation for controller-based API versioning.

API versioning helps keep public API routes stable while allowing future versions to evolve without breaking existing clients.

## Default Version

The default API version is configured under `ProjectTemplate:ApiVersioning`.

```json
{
  "ProjectTemplate": {
    "ApiVersioning": {
      "DefaultMajorVersion": 1,
      "DefaultMinorVersion": 0,
      "AssumeDefaultVersionWhenUnspecified": true,
      "ReportApiVersions": true,
      "EnableUrlSegmentVersioning": true,
      "EnableHeaderVersioning": true,
      "HeaderName": "X-API-Version"
    }
  }
}
```
The application starts with API version `1.0`.

## Supported Versioning Strategies

The application supports both URL segment and request header versioning.

URL segment example:
```http
GET /api/v1/application-information
```
Header versioning example:
```http
GET /api/application-information
X-API-Version: 1.0
```
URL segment versioning is usually easier to discover and debug. Header versioning can be useful for clients that prefer stable resource URLs.

## Controller Convention

Versioned API controllers should declare the supported API version and include a versioned route.
```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/example")]
public sealed class ExampleController : ControllerBase
{
}
```
If an endpoint should also support header-based versioning without the version in the URL, add an unversioned route as well.
```charp
[Route("api/example")]
```

## Response Headers

When `ReportApiVersions` is enabled, responses include API version headers that help clients discover supported and deprecated versions.

## Production Guidance

For production APIs:
- Prefer a consistent versioning strategy across the application.
- Avoid mixing URL and header versioning on the same endpoint unless clients need both.
- Keep old API versions available long enough for client migration.
- Document deprecated versions before removing them.
- Add tests for each supported API version.
