using DockerFactor.Core.Manifests;

namespace DockerFactor.Core.Validation;

public sealed record ManifestValidationResult(
    ApplicationManifest? Manifest,
    IReadOnlyList<ValidationIssue> Issues)
{
    public bool IsValid => Manifest is not null && Issues.All(issue => issue.Severity != ValidationSeverity.Error);

    public static ManifestValidationResult Invalid(params ValidationIssue[] issues) =>
        new(null, issues);
}
