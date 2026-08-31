namespace DockerFactor.Engine.Manifests;

internal sealed class YamlApplicationManifest
{
    public string ApiVersion { get; set; } = string.Empty;
    public string Kind { get; set; } = string.Empty;
    public YamlManifestMetadata? Metadata { get; set; }
    public YamlApplicationSpec? Spec { get; set; }
}

internal sealed class YamlManifestMetadata
{
    public string Name { get; set; } = string.Empty;
}

internal sealed class YamlApplicationSpec
{
    public string Runtime { get; set; } = string.Empty;
    public int Port { get; set; }
    public string? Build { get; set; }
    public string? Command { get; set; }
}
