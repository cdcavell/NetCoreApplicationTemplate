## OpenTelemetry

The application includes baseline OpenTelemetry support for tracing and metrics.

OpenTelemetry is registered through:

```csharp
builder.Services.AddApplicationOpenTelemetry(builder.Configuration, builder.Environment);
```
Configuration is controlled through `appsettings.json`:
```json
"ProjectTemplate": {
  "OpenTelemetry": {
    "Enabled": true,
    "ServiceName": "ProjectTemplate.Web",
    "ServiceVersion": "0.1.3",
    "EnableTracing": true,
    "EnableMetrics": true,
    "EnableAspNetCoreInstrumentation": true,
    "EnableHttpClientInstrumentation": true,
    "Otlp": {
      "Enabled": false,
      "Endpoint": "",
      "Protocol": "Grpc"
    }
  }
}
```
By default, the application collects local tracing and metrics instrumentation but does not export telemetry to an external collector. To enable OTLP export, configure an OTLP endpoint and set `ProjectTemplate:OpenTelemetry:Otlp:Enabled` to `true`.

Common local OTLP collector endpoints:
```text
http://localhost:4317
http://localhost:4318
```
The OTLP exporter can also be configured through standard OpenTelemetry environment variables such as `OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_EXPORTER_OTLP_PROTOCOL`.
