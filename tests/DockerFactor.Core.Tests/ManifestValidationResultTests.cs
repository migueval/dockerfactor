using DockerFactor.Core.Manifests;
using DockerFactor.Core.Validation;

namespace DockerFactor.Core.Tests;

public sealed class ManifestValidationResultTests
{
    [Fact]
    public void IsValid_requires_a_manifest_without_issues()
    {
        var result = new ManifestValidationResult(new ApplicationManifest(), []);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Invalid_has_no_manifest_and_contains_the_issue()
    {
        var result = ManifestValidationResult.Invalid(new ValidationIssue("DFM000", "file", "missing"));

        Assert.False(result.IsValid);
        Assert.Null(result.Manifest);
        Assert.Single(result.Issues);
    }
}
