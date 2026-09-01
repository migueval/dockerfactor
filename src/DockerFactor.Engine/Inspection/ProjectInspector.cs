using DockerFactor.Core.Abstractions;
using DockerFactor.Core.Inspection;
using DockerFactor.Engine.Manifests;

namespace DockerFactor.Engine.Inspection;

public sealed class ProjectInspector : IProjectInspector
{
    private readonly YamlManifestReader _reader = new();
    private readonly ProjectRuntimeDetector _detector = new();

    public ProjectInspection Inspect(string projectDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectDirectory);

        var fullPath = Path.GetFullPath(projectDirectory);
        var manifestPath = Path.Combine(fullPath, "dockerfactor.yaml");
        var detection = _detector.Detect(fullPath);
        var validation = _reader.Read(manifestPath);

        if (validation.Manifest is not null)
        {
            var issues = validation.Issues.ToList();
            var manifest = validation.Manifest;

            if (detection.Runtime is not null &&
                !string.Equals(detection.Runtime, manifest.Spec?.Runtime, StringComparison.OrdinalIgnoreCase))
            {
                issues.Add(new(
                    "DFM101",
                    "spec.runtime",
                    $"Declared runtime '{manifest.Spec?.Runtime}' differs from detected runtime '{detection.Runtime}' ({detection.Evidence}).",
                    Core.Validation.ValidationSeverity.Warning));
            }

            if (string.IsNullOrWhiteSpace(manifest.Spec?.Build))
                issues.Add(new("DFM201", "spec.build", "Consider declaring an explicit build command.", Core.Validation.ValidationSeverity.Info));

            if (string.IsNullOrWhiteSpace(manifest.Spec?.Command))
                issues.Add(new("DFM202", "spec.command", "Consider declaring an explicit start command.", Core.Validation.ValidationSeverity.Info));

            validation = new(manifest, issues);
        }

        return new(fullPath, manifestPath, detection, validation);
    }
}
