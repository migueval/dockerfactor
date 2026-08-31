using DockerFactor.Core.Manifests;
using DockerFactor.Core.Validation;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DockerFactor.Engine.Manifests;

public sealed class YamlManifestReader
{
    private readonly IDeserializer _deserializer = new StaticDeserializerBuilder(new DockerFactorYamlContext())
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .WithDuplicateKeyChecking()
        .Build();

    private readonly ManifestValidator _validator = new();

    public ManifestValidationResult Read(string manifestPath)
    {
        if (!File.Exists(manifestPath))
            return ManifestValidationResult.Invalid(new ValidationIssue("DFM000", manifestPath, "Manifest file was not found."));

        try
        {
            using var input = File.OpenText(manifestPath);
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
}
