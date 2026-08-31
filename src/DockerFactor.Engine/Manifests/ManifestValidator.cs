using System.Text.RegularExpressions;
using DockerFactor.Core.Manifests;
using DockerFactor.Core.Validation;

namespace DockerFactor.Engine.Manifests;

public sealed partial class ManifestValidator
{
    public const string SupportedApiVersion = "dockerfactor.dev/v1alpha1";
    public const string SupportedKind = "Application";

    private static readonly HashSet<string> SupportedRuntimes =
        new(StringComparer.OrdinalIgnoreCase) { "dotnet", "node", "angular", "nestjs", "go", "python", "generic" };

    public IReadOnlyList<ValidationIssue> Validate(ApplicationManifest manifest)
    {
        var issues = new List<ValidationIssue>();

        if (!string.Equals(manifest.ApiVersion, SupportedApiVersion, StringComparison.Ordinal))
            issues.Add(new("DFM001", "apiVersion", $"Expected '{SupportedApiVersion}'."));

        if (!string.Equals(manifest.Kind, SupportedKind, StringComparison.Ordinal))
            issues.Add(new("DFM002", "kind", $"Expected '{SupportedKind}'."));

        if (manifest.Metadata is null || string.IsNullOrWhiteSpace(manifest.Metadata.Name) || !DnsLabelRegex().IsMatch(manifest.Metadata.Name))
            issues.Add(new("DFM003", "metadata.name", "Use 1-63 lowercase letters, numbers or hyphens; start and end with a letter or number."));

        if (manifest.Spec is null || !SupportedRuntimes.Contains(manifest.Spec.Runtime))
            issues.Add(new("DFM004", "spec.runtime", $"Unsupported runtime. Allowed values: {string.Join(", ", SupportedRuntimes.Order())}."));

        if (manifest.Spec is null || manifest.Spec.Port is < 1 or > 65535)
            issues.Add(new("DFM005", "spec.port", "Port must be between 1 and 65535."));

        return issues;
    }

    [GeneratedRegex("^[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?$", RegexOptions.CultureInvariant)]
    private static partial Regex DnsLabelRegex();
}
