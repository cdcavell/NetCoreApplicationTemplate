# Error Handling

The application includes centralized error handling for both unhandled exceptions and HTTP status code responses.

Error handling is configured through the application pipeline using:

```csharp
app.UseApplicationErrorHandling();
```
The error handling behavior is environment-aware:

- In Development, the application uses the developer exception page.
- In non-development environments, unhandled exceptions are routed to `/Home/Error/500`.
- HTTP status code responses are re-executed through `/Home/Error/{statusCode}`.
- Error responses are user-safe and do not expose exception details.
- Error events are logged using source-generated `LoggerMessage` methods.
- The request ID displayed on the error page matches the request ID written to the application logs.

## Status Code Pages

Status code pages are handled centrally using:
```csharp
app.UseStatusCodePagesWithReExecute("/Home/Error/{0}");
```
This allows common HTTP responses such as `404 Not Found`, `401 Unauthorized`, `403 Forbidden`, and `429 Too Many Requests` to use the shared error page strategy.

## Unhandled Exceptions

In non-development environments, unhandled exceptions are routed through:
```csharp
app.UseExceptionHandler("/Home/Error/500");
```
This allows unhandled exceptions to be logged and displayed using the shared error page strategy without exposing sensitive exception details to end users.

## Request ID Correlation

The error page displays the same request ID that is written to the log entry.

Example browser output:
```text
Request ID: 0HNL9ADUFCPUT:00000009
```
Example log output:
```text
Status code page routed to error page. StatusCode: 404; OriginalPath: /invalid; RemoteIpAddress: ::1; RequestId: 0HNL9ADUFCPUT:00000009
```
This makes it easier to match a user-facing error page with the corresponding application log entry.

## Logging

Error handling logs include:
- Status code routed to the error page.
- Original request path.
- Remote IP address when available.
- Request ID.
- Exception details for unhandled exceptions.

Log event IDs are centralized in ApplicationLogEventIds to keep application logging consistent.

## Centralized Problem Details Error Handling

The application uses centralized error handling to provide consistent responses for both browser and API-style requests.

Browser requests are routed to the standard application error page, such as `/Home/Error/{statusCode}`. API, AJAX, and JSON-oriented requests receive a Problem Details response using the ASP.NET Core `IProblemDetailsService`.

Problem Details responses include safe metadata such as:

- HTTP status code
- Error title
- Request path
- Trace ID
- Request ID

Detailed exception information is only exposed in the Development environment. Production responses avoid leaking stack traces, exception messages, connection details, or internal implementation information.

Unhandled exceptions are logged centrally and converted into safe Problem Details responses when the request expects JSON.
