## Forwarded Headers and Proxy Support

The application includes optional forwarded headers support for deployments behind reverse proxies,
load balancers, ingress controllers, and hosted infrastructure.

Forwarded headers allow the application to correctly resolve the original client IP address,
request scheme, and host when traffic is forwarded through another server before reaching Kestrel.

Configuration is controlled through `appsettings.json`:

```json
"ProjectTemplate": {
  "ForwardedHeaders": {
    "Enabled": true,
    "Headers": [
      "XForwardedFor",
      "XForwardedProto"
    ],
    "ForwardLimit": 1,
    "RequireHeaderSymmetry": false,
    "ClearKnownNetworksAndProxies": false,
    "KnownProxies": [],
    "KnownNetworks": [],
    "AllowedHosts": []
  }
}
```
By default, the application processes:

- `X-Forwarded-For`
- `X-Forwarded-Proto`

Production deployments should explicitly configure trusted proxy IP addresses or trusted proxy
networks using `KnownProxies` or `KnownNetworks`.

`XForwardedHost` is intentionally not enabled by default. If enabled, configure `AllowedHosts`
to reduce the risk of host header spoofing.
