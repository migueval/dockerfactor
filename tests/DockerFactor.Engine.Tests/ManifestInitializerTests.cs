using DockerFactor.Core.Initialization;
using DockerFactor.Engine.Initialization;

namespace DockerFactor.Engine.Tests;

public sealed class ManifestInitializerTests : IDisposable
{
    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"DockerFactor Init Tests {Guid.NewGuid():N}");

    public ManifestInitializerTests() => Directory.CreateDirectory(_directory);

    [Fact]
    public void Proposes_deterministic_dotnet_defaults_and_safe_name()
    {
        File.WriteAllText(Path.Combine(_directory, "My App.csproj"), "<Project />");

        var proposal = new ManifestInitializer().Propose(_directory);

        Assert.Equal("dotnet", proposal.Manifest.Spec.Runtime);
        Assert.Equal(8080, proposal.Manifest.Spec.Port);
        Assert.Equal("dotnet publish \"My App.csproj\" -c Release", proposal.Manifest.Spec.Build);
        Assert.Contains("build: \"dotnet publish \\\"My App.csproj\\\" -c Release\"", proposal.Content);
        Assert.Matches("^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$", proposal.Manifest.Metadata.Name);
        Assert.False(File.Exists(proposal.ManifestPath));
    }

    [Fact]
    public void Write_creates_then_refuses_to_overwrite_without_force()
    {
        var initializer = new ManifestInitializer();
        var proposal = initializer.Propose(_directory);

        var created = initializer.Write(proposal, force: false);
        File.WriteAllText(proposal.ManifestPath, "user content");
        var refused = initializer.Write(proposal, force: false);

        Assert.Equal(ManifestWriteStatus.Created, created);
        Assert.Equal(ManifestWriteStatus.Refused, refused);
        Assert.Equal("user content", File.ReadAllText(proposal.ManifestPath));
    }

    [Fact]
    public void Force_explicitly_overwrites_existing_manifest()
    {
        var initializer = new ManifestInitializer();
        var proposal = initializer.Propose(_directory);
        File.WriteAllText(proposal.ManifestPath, "old");

        var status = initializer.Write(proposal, force: true);

        Assert.Equal(ManifestWriteStatus.Overwritten, status);
        Assert.Equal(proposal.Content, File.ReadAllText(proposal.ManifestPath));
    }

    public void Dispose()
    {
        if (Directory.Exists(_directory))
            Directory.Delete(_directory, recursive: true);
    }
}
