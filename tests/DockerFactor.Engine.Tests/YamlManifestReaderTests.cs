using DockerFactor.Engine.Manifests;

namespace DockerFactor.Engine.Tests;

public sealed class YamlManifestReaderTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"dockerfactor-tests-{Guid.NewGuid():N}");

    public YamlManifestReaderTests() => Directory.CreateDirectory(_directory);

    [Fact]
    public void Reads_and_validates_a_supported_manifest()
    {
        var path = WriteManifest("""
            apiVersion: dockerfactor.dev/v1alpha1
            kind: Application
            metadata:
              name: hello-api
            spec:
              runtime: dotnet
              port: 8080
            """);

        var result = new YamlManifestReader().Read(path);

        Assert.True(result.IsValid);
        Assert.Equal("hello-api", result.Manifest!.Metadata.Name);
    }

    [Fact]
    public void Rejects_unknown_fields_instead_of_silently_ignoring_them()
    {
        var path = WriteManifest("""
            apiVersion: dockerfactor.dev/v1alpha1
            kind: Application
            metadata:
              name: hello-api
            spec:
              runtime: dotnet
              port: 8080
              privileged: true
            """);

        var result = new YamlManifestReader().Read(path);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DFM006");
    }

    [Fact]
    public void Reports_a_missing_manifest()
    {
        var result = new YamlManifestReader().Read(Path.Combine(_directory, "missing.yaml"));

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DFM000");
    }

    [Fact]
    public void Null_sections_are_validation_errors_instead_of_crashes()
    {
        var path = WriteManifest("""
            apiVersion: dockerfactor.dev/v1alpha1
            kind: Application
            metadata: null
            spec: null
            """);

        var result = new YamlManifestReader().Read(path);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DFM003");
        Assert.Contains(result.Issues, issue => issue.Code == "DFM004");
        Assert.Contains(result.Issues, issue => issue.Code == "DFM005");
    }

    [Fact]
    public void Rejects_yaml_anchors_and_aliases()
    {
        var path = WriteManifest("""
            apiVersion: dockerfactor.dev/v1alpha1
            kind: Application
            metadata: &metadata
              name: hello-api
            spec:
              runtime: dotnet
              port: 8080
            copy: *metadata
            """);

        var result = new YamlManifestReader().Read(path);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DFM009");
    }

    [Fact]
    public void Rejects_explicit_yaml_tags()
    {
        var path = WriteManifest("""
            apiVersion: dockerfactor.dev/v1alpha1
            kind: Application
            metadata:
              name: !!str hello-api
            spec:
              runtime: dotnet
              port: 8080
            """);

        var result = new YamlManifestReader().Read(path);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DFM010");
    }

    [Fact]
    public void Rejects_oversized_manifests()
    {
        var path = WriteManifest(new string('x', checked((int)YamlManifestReader.MaximumManifestBytes + 1)));

        var result = new YamlManifestReader().Read(path);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "DFM008");
    }

    private string WriteManifest(string content)
    {
        var path = Path.Combine(_directory, "dockerfactor.yaml");
        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}
