using DockerFactor.Core.Manifests;
using DockerFactor.Engine.Manifests;

namespace DockerFactor.Engine.Tests;

public sealed class ManifestValidatorTests
{
    [Fact]
    public void Valid_manifest_has_no_issues()
    {
        var issues = new ManifestValidator().Validate(ValidManifest());

        Assert.Empty(issues);
    }

    [Fact]
    public void Invalid_manifest_reports_stable_codes_and_paths()
    {
        var manifest = new ApplicationManifest
        {
            ApiVersion = "v1",
            Kind = "Service",
            Metadata = new() { Name = "Invalid Name" },
            Spec = new() { Runtime = "cobol", Port = 70000 }
        };

        var issues = new ManifestValidator().Validate(manifest);

        Assert.Equal(["DFM001", "DFM002", "DFM003", "DFM004", "DFM005"], issues.Select(issue => issue.Code));
    }

    private static ApplicationManifest ValidManifest() => new()
    {
        ApiVersion = ManifestValidator.SupportedApiVersion,
        Kind = ManifestValidator.SupportedKind,
        Metadata = new() { Name = "hello-api" },
        Spec = new() { Runtime = "dotnet", Port = 8080 }
    };
}
