# DockerFactor .NET 10 Example API

This project intentionally starts without `dockerfactor.yaml` so the complete safe initialization workflow can be tested.

Run the following commands from the repository root.

## 1. Preview the proposal

```powershell
dotnet run --project src\DockerFactor.CLI -- init examples\dotnet-api --dry-run
```

Expected detection:

```text
Detected: dotnet (DockerFactor.ExampleApi.csproj)
Runtime:  dotnet
Port:     8080
```

## 2. Preview as JSON

```powershell
dotnet run --project src\DockerFactor.CLI -- init examples\dotnet-api --dry-run --output json
```

## 3. Create the manifest

```powershell
dotnet run --project src\DockerFactor.CLI -- init examples\dotnet-api
```

This creates `examples\dotnet-api\dockerfactor.yaml`.

## 4. Inspect and validate

```powershell
dotnet run --project src\DockerFactor.CLI -- inspect examples\dotnet-api
dotnet run --project src\DockerFactor.CLI -- validate examples\dotnet-api --strict
dotnet run --project src\DockerFactor.CLI -- validate examples\dotnet-api --output json
```

All three commands should return exit code `0`.

## 5. Verify overwrite protection

Run init again:

```powershell
dotnet run --project src\DockerFactor.CLI -- init examples\dotnet-api
```

DockerFactor should preserve the existing manifest and return exit code `3`. Use `--force` only when you intentionally want to regenerate it.

## 6. Run the API

```powershell
dotnet run --project examples\dotnet-api
```

Open:

- `http://127.0.0.1:8080/`
- `http://127.0.0.1:8080/health`

Stop the API with `Ctrl+C`.

## Reset the initialization exercise

Delete only the generated `examples\dotnet-api\dockerfactor.yaml` file, then repeat the workflow from step 1. The generated manifest is intentionally ignored and not committed with this example.
