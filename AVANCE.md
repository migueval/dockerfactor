# DockerFactor Development Progress

**Updated:** August 31, 2026
**Status:** Second functional increment completed

## Summary

DockerFactor has been intentionally rebuilt from a clean foundation. The previous exploratory prototype was removed so future capabilities can be implemented around explicit contracts, testable behavior and evidence-based security claims.

The first two increments establish the application manifest and read-only inspection/validation workflows. They do not deploy containers, modify the host, open tunnels or change firewall rules.

## Delivered

### Clean project boundaries

- All projects target .NET 10, and the CLI is configured for Native AOT publication.
- `DockerFactor.Core` contains domain contracts and abstractions.
- `DockerFactor.Engine` contains YAML parsing, validation and inspection behavior.
- `DockerFactor.CLI` exposes the user-facing command-line entry point.
- Separate Core and Engine test projects verify contracts and implementation behavior.

### Versioned application manifest

The first supported contract uses:

```yaml
apiVersion: dockerfactor.dev/v1alpha1
kind: Application
metadata:
  name: hello-api
spec:
  runtime: dotnet
  port: 8080
  build: dotnet publish -c Release
  command: dotnet HelloApi.dll
```

Supported runtime identifiers are `dotnet`, `node`, `angular`, `nestjs`, `go`, `python` and `generic`.

### Strict validation

The parser rejects malformed YAML, duplicate keys and unknown fields. Semantic validation covers:

- supported API version and resource kind;
- DNS-label-compatible application names;
- supported runtime identifiers;
- TCP port range from 1 through 65535;
- missing files and inaccessible manifests;
- null manifest sections without crashing.

Validation failures expose stable codes from `DFM000` through `DFM007` so they can later be consumed reliably by CI systems and editor integrations.

### Read-only CLI workflow

The implemented command is:

```bash
docker-factor inspect [PROJECT_DIRECTORY]
docker-factor validate [PROJECT_DIRECTORY] [--strict] [--output text|json]
```

During development it can be executed with:

```bash
dotnet run --project src/DockerFactor.CLI -- inspect examples/hello-api
```

Exit codes form part of the CLI contract:

- `0`: help or successful validation;
- `2`: invalid or missing manifest;
- `64`: invalid command usage.

### CI and editor contract

- Deterministic JSON output is available through `--output json`.
- Strict mode promotes runtime-detection warnings to validation failures.
- A public Draft 2020-12 JSON Schema documents the v1alpha1 contract.
- Runtime detection supports .NET, Node, Angular, NestJS, Go and Python projects without modifying them.
- Defensive parsing rejects oversized manifests, excessive recursion, anchors, aliases and explicit YAML tags.

## Verification

The repository currently builds with zero compiler warnings and zero errors on .NET 10. The automated suite contains 19 passing tests across the Core, Engine and CLI layers. A real `win-x64` Native AOT executable was published successfully and used to validate `examples/hello-api/dockerfactor.yaml` through JSON output; the native process returned exit code `0`.

## Explicitly Not Implemented Yet

- Dockerfile or Compose generation;
- local or remote deployments;
- Cloudflare tunnel management;
- VPS bootstrap and firewall changes;
- cryptographic pairing or mTLS identity;
- host and container security audits;
- state reconciliation, rollout or rollback;
- CI/CD workflow generation.

These remain roadmap items and must not be inferred from the target architecture documents as current product behavior.

## Next Increment

The next recommended increment is a safe initialization workflow that detects the project, previews a proposed manifest and never overwrites user files without explicit approval.
