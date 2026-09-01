using DockerFactor.Core.Manifests;
using DockerFactor.Core.Validation;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DockerFactor.Engine.Manifests;

public sealed class YamlManifestReader
{
    public const long MaximumManifestBytes = 128 * 1024;

    private readonly IDeserializer _deserializer = new StaticDeserializerBuilder(new DockerFactorYamlContext())
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithDuplicateKeyChecking()
        .WithMaximumRecursion(32)
        .Build();

    private readonly ManifestValidator _validator = new();

    public ManifestValidationResult Read(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            return ManifestValidationResult.Invalid(new ValidationIssue("DFM000", manifestPath, "Manifest file was not found."));

        try
        {
            var file = new FileInfo(manifestPath);
            if (file.Length > MaximumManifestBytes)
                return ManifestValidationResult.Invalid(new ValidationIssue("DFM008", manifestPath, $"Manifest exceeds the {MaximumManifestBytes}-byte limit."));

            var content = File.ReadAllText(manifestPath);
            var unsafeFeature = FindUnsafeYamlFeature(content);
            if (unsafeFeature is not null)
                return ManifestValidationResult.Invalid(unsafeFeature);

            using var input = new StringReader(content);
            var document = _deserializer.Deserialize<YamlApplicationManifest>(input);

            if (document is null)
                return ManifestValidationResult.Invalid(new ValidationIssue("DFM006", "$", "Manifest is empty."));

            var manifest = new ApplicationManifest
            {
                ApiVersion = document.ApiVersion,
                Kind = document.Kind,
                Metadata = document.Metadata is null
                    ? null!
                    : new ManifestMetadata { Name = document.Metadata.Name },
                Spec = document.Spec is null
                    ? null!
                    : new ApplicationSpec
                    {
                        Runtime = document.Spec.Runtime,
                        Port = document.Spec.Port,
                        Build = document.Spec.Build,
                        Command = document.Spec.Command
                    }
            };

            return new(manifest, _validator.Validate(manifest));
        }
        catch (YamlException exception)
        {
            var location = exception.Start.Line > 0
                ? $"line {exception.Start.Line}, column {exception.Start.Column}"
                : "$";
            return ManifestValidationResult.Invalid(new ValidationIssue("DFM006", location, exception.Message));
        }
        catch (IOException exception)
        {
            return ManifestValidationResult.Invalid(new ValidationIssue("DFM007", manifestPath, exception.Message));
        }
        catch (UnauthorizedAccessException exception)
        {
            return ManifestValidationResult.Invalid(new ValidationIssue("DFM007", manifestPath, exception.Message));
        }
    }

    private static ValidationIssue? FindUnsafeYamlFeature(string content)
    {
        var parser = new Parser(new StringReader(content));
        while (parser.MoveNext())
        {
            if (parser.Current is AnchorAlias alias)
                return new("DFM009", $"line {alias.Start.Line}, column {alias.Start.Column}", "YAML aliases are not allowed.");

            if (parser.Current is NodeEvent node)
            {
                if (!node.Anchor.IsEmpty)
                    return new("DFM009", $"line {node.Start.Line}, column {node.Start.Column}", "YAML anchors are not allowed.");
                if (!node.Tag.IsEmpty)
                    return new("DFM010", $"line {node.Start.Line}, column {node.Start.Column}", "Explicit YAML tags are not allowed.");
            }
        }

        return null;
    }
}
