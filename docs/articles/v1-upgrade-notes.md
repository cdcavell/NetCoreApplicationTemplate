
# v1.0 Upgrade Notes

This article records upgrade guidance for the first stable `v1.0.0` release of the .NET Core Application Template.

## 0.x to 1.0.0

The `v1.0.0` release establishes the first stable public surface for the template. Pre-1.0 releases are preview releases and may contain release-readiness, packaging, documentation, and workflow changes that are finalized by the stable release.

Before upgrading or generating production applications from `v1.0.0`, review:

- [Public Surface v1.0](public-surface-v1.md)
- [ADR-0003: Record Release Surface and Distribution Strategy](../adr/0003-record-release-surface-and-distribution-strategy.md)
- [Template Packaging](template-packaging.md)
- [Release Checklist](../../RELEASE.md)
- [Changelog](../../CHANGELOG.md)

## Consumer Review Checklist

For applications generated from a pre-1.0 version, compare the generated application against the v1.0 public surface:

```text
[ ] Package ID and template short name are correct.
[ ] Generated solution and project structure match expected names and layout.
[ ] ProjectTemplate configuration sections and option keys are still valid.
[ ] Health endpoints are available at /health, /health/ready, and /health/live.
[ ] Error handling routes are still available.
[ ] Middleware ordering has not been locally modified in a way that breaks documented behavior.
[ ] Container port and probe paths match deployment expectations.
[ ] Any local customizations are documented before adopting v1.0.0 as a baseline.
```

## After v1.0.0

After `v1.0.0`, future changes should be classified using [Public Surface v1.0](public-surface-v1.md).

Breaking changes should be reserved for a future major release and should include explicit migration notes.
