using DockerFactor.Core.Validation;

namespace DockerFactor.Core.Inspection;

public sealed record ProjectInspection(
    string ProjectDirectory,
    string ManifestPath,
    ManifestValidationResult Validation);
