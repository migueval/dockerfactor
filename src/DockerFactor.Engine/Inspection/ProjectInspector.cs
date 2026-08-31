using DockerFactor.Core.Abstractions;
using DockerFactor.Core.Inspection;
using DockerFactor.Engine.Manifests;

namespace DockerFactor.Engine.Inspection;

public sealed class ProjectInspector : IProjectInspector
{
    private readonly YamlManifestReader _reader = new();

    public ProjectInspection Inspect(string projectDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectDirectory);

        var fullPath = Path.GetFullPath(projectDirectory);
        var manifestPath = Path.Combine(fullPath, "dockerfactor.yaml");
        return new(fullPath, manifestPath, _reader.Read(manifestPath));
    }
}
