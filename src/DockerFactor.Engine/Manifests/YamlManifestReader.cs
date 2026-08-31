using DockerFactor.Core.Manifests;
using DockerFactor.Core.Validation;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DockerFactor.Engine.Manifests;

public sealed class YamlManifestReader
{
    private readonly IDeserializer _deserializer = new DeserializerBuilder()
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
            var manifest = _deserializer.Deserialize<ApplicationManifest>(input);

            if (manifest is null)
                return ManifestValidationResult.Invalid(new ValidationIssue("DFM006", "$", "Manifest is empty."));

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
