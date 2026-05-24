# Docker Development Workflow

The application includes optional Docker support for local development and application portability.

Docker support is intended to provide a repeatable local runtime environment. It does not replace normal `dotnet restore`, `dotnet build`, or `dotnet test` workflows.

## Prerequisites

Install Docker Desktop or another Docker-compatible container runtime.

## Build the Image

From the repository root:

```powershell
docker build -t projecttemplate-web:dev .
```

## Run with Docker
```powershell
docker run --rm -p 8080:8080 projecttemplate-web:dev
```
The application is available at:
```
http://localhost:8080
```

### Run with Docker Compose
```powershell
docker compose up --build
```
Stop the container:
```powershell
docker compose down
```
Remove local Docker volumes:
```powershell
docker compose down -v
```

## Local SQLite Data
The compose file stores the local SQLite database under a named Docker volume mounted to:
```
/app/data
```
The compose file overrides the default SQLite connection string:
```
ConnectionStrings__ApplicationDatabase=Data Source=/app/data/application-dev.db
```
This keeps generated container data out of the repository working tree.

## Logs

The compose file mounts application logs to a named Docker volume mounted to:
```
/app/logs
``` 
View container logs:
```powershell
docker compose logs -f projecttemplate.web
```

## Forwarded Headers
The application already supports forwarded headers through the ProjectTemplate:ForwardedHeaders configuration section.

For local Docker development, the compose file enables forwarded headers and clears the default known proxy/network restrictions so forwarded headers can be tested from local container or proxy scenarios.

This setting is for local development only:
```
ProjectTemplate__ForwardedHeaders__ClearKnownNetworksAndProxies=true
```
Production deployments should prefer explicit trusted proxy settings:
```json
"ProjectTemplate": {
  "ForwardedHeaders": {
    "Enabled": true,
    "ForwardLimit": 1,
    "KnownProxies": [
      "10.0.0.10"
    ],
    "KnownNetworks": [
      "10.0.0.0/24"
    ],
    "AllowedHosts": [
      "example.com"
    ]
  }
}
```

## Forwarded Header Smoke Test
Start the container:
```powershell
docker compose up --build
```
Send a request with forwarded headers:
```powershell
curl -I http://localhost:8080/ `
  -H "X-Forwarded-For: 203.0.113.10" `
  -H "X-Forwarded-Proto: https"
```
Then review the application logs:
```powershell
docker compose logs projecttemplate.web
```
The request should complete successfully, and forwarded header handling should not break normal local routing.

## Container Image Strategy

The application uses Microsoft .NET container images with a multi-stage Dockerfile. The SDK image is used for restore, build, and publish steps. The ASP.NET runtime image is used for the final application image.

The local development image tag is:

```text
projecttemplate-web:dev
```

This tag is intended for local development and testing. It is not a published production image contract.

Applications generated from the template may adapt the Dockerfile for production deployment. Production consumers may pin image digests, use organization-approved base images, or apply additional hardening based on their deployment requirements.

After the `v1.0.0` release, changes to documented image names, tag conventions, exposed ports, or runtime image strategy should be reviewed as release-surface changes.

See [ADR-0003: Record Release Surface and Distribution Strategy](../adr/0003-record-release-surface-and-distribution-strategy.md) for the release-surface decision.

## Notes
Do not commit generated SQLite databases, local logs, container volumes, secrets, or environment-specific credentials.
