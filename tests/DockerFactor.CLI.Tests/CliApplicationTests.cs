using System.Text.Json;
using DockerFactor.CLI;

namespace DockerFactor.CLI.Tests;

public sealed class CliApplicationTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"dockerfactor cli tests {Guid.NewGuid():N}");

    public CliApplicationTests() => Directory.CreateDirectory(_directory);

    [Fact]
    public void Validate_json_emits_machine_readable_output()
    {
        WriteManifest("dotnet");
        File.WriteAllText(Path.Combine(_directory, "App.csproj"), "<Project />");
        using var output = new StringWriter();
        using var error = new StringWriter();

        var exitCode = CliApplication.Run(["validate", _directory, "--output", "json"], output, error);
        using var json = JsonDocument.Parse(output.ToString());

        Assert.Equal(0, exitCode);
        Assert.True(json.RootElement.GetProperty("valid").GetBoolean());
        Assert.Equal("dotnet", json.RootElement.GetProperty("detection").GetProperty("runtime").GetString());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Fact]
    public void Strict_mode_fails_when_detected_runtime_differs()
    {
        WriteManifest("go");
        File.WriteAllText(Path.Combine(_directory, "App.csproj"), "<Project />");

        var normalExit = CliApplication.Run(["validate", _directory], new StringWriter(), new StringWriter());
        var strictExit = CliApplication.Run(["validate", _directory, "--strict"], new StringWriter(), new StringWriter());

        Assert.Equal(0, normalExit);
        Assert.Equal(2, strictExit);
    }

    [Fact]
    public void Unknown_command_returns_usage_exit_code()
    {
        var exitCode = CliApplication.Run(["unknown"], new StringWriter(), new StringWriter());

        Assert.Equal(64, exitCode);
    }

    private void WriteManifest(string runtime) => File.WriteAllText(Path.Combine(_directory, "dockerfactor.yaml"), $$"""
        apiVersion: dockerfactor.dev/v1alpha1
        kind: Application
        metadata:
          name: cli-test
        spec:
          runtime: {{runtime}}
          port: 8080
          build: build
          command: start
        """);

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}
