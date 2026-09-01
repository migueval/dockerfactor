using DockerFactor.Core.Inspection;
using DockerFactor.Core.Manifests;

namespace DockerFactor.Core.Initialization;

public sealed record ManifestProposal(
    string ProjectDirectory,
    string ManifestPath,
    ProjectDetection Detection,
    ApplicationManifest Manifest,
    string Content);
