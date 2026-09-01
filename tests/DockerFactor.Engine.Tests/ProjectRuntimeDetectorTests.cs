using DockerFactor.Engine.Inspection;

namespace DockerFactor.Engine.Tests;

public sealed class ProjectRuntimeDetectorTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"dockerfactor-detection-tests-{Guid.NewGuid():N}");

    public ProjectRuntimeDetectorTests() => Directory.CreateDirectory(_directory);

    [Theory]
    [InlineData("App.csproj", "", "dotnet")]
    [InlineData("go.mod", "module example.com/app", "go")]
    [InlineData("requirements.txt", "flask", "python")]
    [InlineData("package.json", "{\"dependencies\":{\"@angular/core\":\"1.0.0\"}}", "angular")]
    public void Detects_runtime_from_project_evidence(string fileName, string content, string expectedRuntime)
    {
        File.WriteAllText(Path.Combine(_directory, fileName), content);

        var detection = new ProjectRuntimeDetector().Detect(_directory);

        Assert.Equal(expectedRuntime, detection.Runtime);
        Assert.Equal(fileName, detection.Evidence);
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}
