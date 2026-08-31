namespace DockerFactor.Core.Manifests;

public sealed class ApplicationManifest
{
    public string ApiVersion { get; init; } = string.Empty;
    public string Kind { get; init; } = string.Empty;
    public ManifestMetadata Metadata { get; init; } = new();
    public ApplicationSpec Spec { get; init; } = new();
}

public sealed class ManifestMetadata
{
    public string Name { get; init; } = string.Empty;
}

public sealed class ApplicationSpec
{
    public string Runtime { get; init; } = string.Empty;
    public int Port { get; init; }
    public string? Build { get; init; }
    public string? Command { get; init; }
}
