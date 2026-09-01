# Safe Manifest Initialization

`docker-factor init` creates the first `dockerfactor.yaml` proposal from read-only project detection. It never generates Dockerfiles, executes project commands or contacts external services.

## Preview

Always preview the proposal when evaluating a new project:

```bash
docker-factor init ./my-app --dry-run
docker-factor init ./my-app --dry-run --output json
```

Dry-run mode returns exit code `0` and never writes a file.

## Create

If `dockerfactor.yaml` does not exist:

```bash
docker-factor init ./my-app
```

The file is created atomically with UTF-8 encoding and no byte-order mark.

## Existing manifests

DockerFactor refuses to replace an existing manifest:

```text
Refusing to overwrite existing manifest ... Use --force to replace it explicitly.
```

The command returns exit code `3` and preserves the file byte-for-byte. Replacement requires explicit authorization:

```bash
docker-factor init ./my-app --force
```

`--dry-run` and `--force` cannot be combined. Invalid option combinations return exit code `64`; filesystem write failures return `74`.

## Generated defaults

| Runtime | Port | Build | Command |
| :--- | ---: | :--- | :--- |
| .NET | 8080 | `dotnet publish <project> -c Release` | `dotnet <project>.dll` when a `.csproj` is detected |
| Node.js | 3000 | `npm run build --if-present` | `npm start` |
| Angular | 4200 | `npm run build` | `npm start` |
| NestJS | 3000 | `npm run build` | `npm start` |
| Go | 8080 | `go build -o app .` | `./app` |
| Python | 8000 | Not inferred | `python app.py` |
| Generic | 8080 | Not inferred | Not inferred |

Generated values are proposals, not guarantees about an application's behavior. Review the manifest before using it in later deployment stages.
