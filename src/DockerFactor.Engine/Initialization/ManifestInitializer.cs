using System.Text;
using DockerFactor.Core.Initialization;
using DockerFactor.Core.Manifests;
using DockerFactor.Engine.Inspection;

namespace DockerFactor.Engine.Initialization;

public sealed class ManifestInitializer
{
    private readonly ProjectRuntimeDetector _detector = new();

    public ManifestProposal Propose(string projectDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectDirectory);

        var fullPath = Path.GetFullPath(projectDirectory);
        if (!Directory.Exists(fullPath))
            throw new DirectoryNotFoundException($"Project directory was not found: {fullPath}");

        var detection = _detector.Detect(fullPath);
        var runtime = detection.Runtime ?? "generic";
        var name = ToDnsLabel(new DirectoryInfo(fullPath).Name);
        var (port, build, command) = Defaults(runtime, detection.Evidence);
        var manifest = new ApplicationManifest
        {
            ApiVersion = Manifests.ManifestValidator.SupportedApiVersion,
            Kind = Manifests.ManifestValidator.SupportedKind,
            Metadata = new() { Name = name },
            Spec = new() { Runtime = runtime, Port = port, Build = build, Command = command }
        };

        return new(fullPath, Path.Combine(fullPath, "dockerfactor.yaml"), detection, manifest, Render(manifest));
    }

    public ManifestWriteStatus Write(ManifestProposal proposal, bool force)
    {
        if (File.Exists(proposal.ManifestPath) && !force)
            return ManifestWriteStatus.Refused;

        if (force)
        {
            var existed = File.Exists(proposal.ManifestPath);
            File.WriteAllText(proposal.ManifestPath, proposal.Content, new UTF8Encoding(false));
            return existed ? ManifestWriteStatus.Overwritten : ManifestWriteStatus.Created;
        }

        using var stream = new FileStream(proposal.ManifestPath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
        using var writer = new StreamWriter(stream, new UTF8Encoding(false));
        writer.Write(proposal.Content);
        return ManifestWriteStatus.Created;
    }

    private static (int Port, string? Build, string? Command) Defaults(string runtime, string? evidence) => runtime switch
    {
        "dotnet" => DotNetDefaults(evidence),
        "node" => (3000, "npm run build --if-present", "npm start"),
        "angular" => (4200, "npm run build", "npm start"),
        "nestjs" => (3000, "npm run build", "npm start"),
        "go" => (8080, "go build -o app .", "./app"),
        "python" => (8000, null, "python app.py"),
        _ => (8080, null, null)
    };

    private static (int Port, string? Build, string? Command) DotNetDefaults(string? evidence)
    {
        var projectFile = evidence ?? "App.csproj";
        var command = projectFile.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase)
            ? $"dotnet {Path.GetFileNameWithoutExtension(projectFile)}.dll"
            : null;
        return (8080, $"dotnet publish {QuoteCommandArgument(projectFile)} -c Release", command);
    }

    private static string Render(ApplicationManifest manifest)
    {
        var lines = new List<string>
        {
            $"apiVersion: {manifest.ApiVersion}",
            $"kind: {manifest.Kind}",
            "metadata:",
            $"  name: {manifest.Metadata.Name}",
            "spec:",
            $"  runtime: {manifest.Spec.Runtime}",
            $"  port: {manifest.Spec.Port}"
        };

        if (manifest.Spec.Build is not null)
            lines.Add($"  build: {QuoteYaml(manifest.Spec.Build)}");
        if (manifest.Spec.Command is not null)
            lines.Add($"  command: {QuoteYaml(manifest.Spec.Command)}");

        return string.Join('\n', lines) + '\n';
    }

    private static string ToDnsLabel(string value)
    {
        var result = new StringBuilder();
        var pendingHyphen = false;

        foreach (var character in value.ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                if (pendingHyphen && result.Length > 0)
                    result.Append('-');
                result.Append(character);
                pendingHyphen = false;
            }
            else
            {
                pendingHyphen = result.Length > 0;
            }

        }

        if (result.Length == 0)
            return "app";

        var normalized = result.ToString().TrimEnd('-');
        return normalized.Length <= 63 ? normalized : normalized[..63].TrimEnd('-');
    }

    private static string QuoteYaml(string value) => $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string QuoteCommandArgument(string value) => value.Contains(' ') ? $"\"{value}\"" : value;
}
