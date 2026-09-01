# DockerFactor Application Manifest

The `dockerfactor.yaml` file is DockerFactor's versioned, declarative application contract. The current API version is `dockerfactor.dev/v1alpha1` and its machine-readable JSON Schema is available at [`schemas/dockerfactor.v1alpha1.schema.json`](../schemas/dockerfactor.v1alpha1.schema.json).

## Example

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

## Fields

| Path | Required | Description |
| :--- | :---: | :--- |
| `apiVersion` | Yes | Must be `dockerfactor.dev/v1alpha1`. |
| `kind` | Yes | Must be `Application`. |
| `metadata.name` | Yes | DNS-label-compatible application name, 1–63 characters. |
| `spec.runtime` | Yes | One of `angular`, `dotnet`, `generic`, `go`, `nestjs`, `node`, or `python`. |
| `spec.port` | Yes | Application TCP port from 1 through 65535. |
| `spec.build` | No | Explicit application build command. |
| `spec.command` | No | Explicit application start command. |

Unknown fields, duplicate keys, explicit YAML tags, anchors and aliases are rejected. Manifests are limited to 128 KiB and a maximum deserialization depth of 32.

## Validation severities

- `error` makes the manifest invalid.
- `warning` reports a suspicious project/manifest mismatch. It fails only with `validate --strict`.
- `info` is an optional recommendation and never fails validation.

## Stable diagnostic codes

| Code | Severity | Meaning |
| :--- | :--- | :--- |
| `DFM000` | Error | Manifest file not found. |
| `DFM001` | Error | Unsupported API version. |
| `DFM002` | Error | Unsupported resource kind. |
| `DFM003` | Error | Invalid application name. |
| `DFM004` | Error | Unsupported runtime. |
| `DFM005` | Error | Port outside the supported range. |
| `DFM006` | Error | Malformed YAML, empty document or unknown field. |
| `DFM007` | Error | Manifest could not be read. |
| `DFM008` | Error | Manifest exceeds 128 KiB. |
| `DFM009` | Error | YAML anchor or alias is present. |
| `DFM010` | Error | Explicit YAML tag is present. |
| `DFM101` | Warning | Declared runtime differs from detected project runtime. |
| `DFM201` | Info | No explicit build command. |
| `DFM202` | Info | No explicit start command. |

## CLI usage

```bash
docker-factor inspect ./my-app
docker-factor inspect ./my-app --output json
docker-factor validate ./my-app
docker-factor validate ./my-app --strict --output json
```

Exit codes are `0` for success, `2` for validation failure and `64` for invalid CLI usage. JSON output is written to stdout and is intended for CI and editor integrations.
