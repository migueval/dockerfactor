using DockerFactor.Core.Inspection;
using DockerFactor.Core.Initialization;
using DockerFactor.Core.Validation;
using DockerFactor.Engine.Initialization;
using DockerFactor.Engine.Inspection;

namespace DockerFactor.CLI;

public static class CliApplication
{
    public static int Run(string[] args, TextWriter output, TextWriter error)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(error);

        if (!CliOptions.TryParse(args, out var options, out var parseError))
        {
            error.WriteLine(parseError);
            PrintHelp(error);
            return 64;
        }

        if (options is null)
        {
            PrintHelp(output);
            return 0;
        }

        if (options.Command == "init")
            return RunInit(options, output, error);

        var inspection = new ProjectInspector().Inspect(options.Directory);
        var hasErrors = inspection.Validation.Issues.Any(issue => issue.Severity == ValidationSeverity.Error) ||
                        inspection.Validation.Manifest is null;
        var hasWarnings = inspection.Validation.Issues.Any(issue => issue.Severity == ValidationSeverity.Warning);
        var effectiveValid = !hasErrors && !(options.Strict && hasWarnings);

        if (options.Json)
            JsonInspectionWriter.Write(output, inspection, effectiveValid);
        else if (options.Command == "inspect")
            WriteInspection(output, error, inspection, effectiveValid);
        else
            WriteValidation(output, error, inspection, effectiveValid, options.Strict);

        return effectiveValid ? 0 : 2;
    }

    private static int RunInit(CliOptions options, TextWriter output, TextWriter error)
    {
        try
        {
            var initializer = new ManifestInitializer();
            var proposal = initializer.Propose(options.Directory);

            if (options.DryRun)
            {
                if (options.Json) JsonInitializationWriter.Write(output, proposal, "preview");
                else WriteInitText(output, proposal, "Preview only; no file was written.", includeYaml: true);
                return 0;
            }

            var status = initializer.Write(proposal, options.Force);
            if (status == ManifestWriteStatus.Refused)
            {
                if (options.Json) JsonInitializationWriter.Write(output, proposal, "refused");
                else error.WriteLine($"Refusing to overwrite existing manifest: {proposal.ManifestPath}. Use --force to replace it explicitly.");
                return 3;
            }

            var statusText = status == ManifestWriteStatus.Created ? "created" : "overwritten";
            if (options.Json) JsonInitializationWriter.Write(output, proposal, statusText);
            else WriteInitText(output, proposal, $"Manifest {statusText}: {proposal.ManifestPath}", includeYaml: false);
            return 0;
        }
        catch (DirectoryNotFoundException exception)
        {
            error.WriteLine(exception.Message);
            return 2;
        }
        catch (IOException exception)
        {
            error.WriteLine($"Could not write manifest: {exception.Message}");
            return 74;
        }
        catch (UnauthorizedAccessException exception)
        {
            error.WriteLine($"Could not write manifest: {exception.Message}");
            return 74;
        }
    }

    private static void WriteInitText(TextWriter output, ManifestProposal proposal, string message, bool includeYaml)
    {
        output.WriteLine("DockerFactor manifest initialization");
        output.WriteLine($"Project:  {proposal.ProjectDirectory}");
        output.WriteLine($"Detected: {proposal.Detection.Runtime ?? "unknown"}" +
                         (proposal.Detection.Evidence is null ? string.Empty : $" ({proposal.Detection.Evidence})"));
        output.WriteLine($"Runtime:  {proposal.Manifest.Spec.Runtime}");
        output.WriteLine($"Port:     {proposal.Manifest.Spec.Port}");
        output.WriteLine(message);
        if (includeYaml)
        {
            output.WriteLine();
            output.Write(proposal.Content);
        }
    }

    private static void WriteInspection(TextWriter output, TextWriter error, ProjectInspection inspection, bool valid)
    {
        output.WriteLine("DockerFactor project inspection");
        output.WriteLine($"Project:  {inspection.ProjectDirectory}");
        output.WriteLine($"Manifest: {inspection.ManifestPath}");
        output.WriteLine($"Detected: {inspection.Detection.Runtime ?? "unknown"}" +
                         (inspection.Detection.Evidence is null ? string.Empty : $" ({inspection.Detection.Evidence})"));
        WriteIssues(error, inspection.Validation.Issues);

        if (!valid)
        {
            error.WriteLine("Result: invalid");
            return;
        }

        var manifest = inspection.Validation.Manifest!;
        output.WriteLine("Result:   valid");
        output.WriteLine($"App:      {manifest.Metadata.Name}");
        output.WriteLine($"Runtime:  {manifest.Spec.Runtime}");
        output.WriteLine($"Port:     {manifest.Spec.Port}");
    }

    private static void WriteValidation(TextWriter output, TextWriter error, ProjectInspection inspection, bool valid, bool strict)
    {
        WriteIssues(error, inspection.Validation.Issues);
        output.WriteLine(valid
            ? "Manifest validation succeeded."
            : strict
                ? "Manifest validation failed in strict mode."
                : "Manifest validation failed.");
    }

    private static void WriteIssues(TextWriter writer, IReadOnlyList<ValidationIssue> issues)
    {
        foreach (var issue in issues)
            writer.WriteLine($"{issue.Code} {issue.Severity.ToString().ToLowerInvariant()} {issue.Path}: {issue.Message}");
    }

    private static void PrintHelp(TextWriter writer)
    {
        writer.WriteLine("DockerFactor — .NET 10 Native AOT deployment CLI");
        writer.WriteLine();
        writer.WriteLine("Usage:");
        writer.WriteLine("  docker-factor inspect [PROJECT_DIRECTORY] [--output text|json]");
        writer.WriteLine("  docker-factor validate [PROJECT_DIRECTORY] [--strict] [--output text|json]");
        writer.WriteLine("  docker-factor init [PROJECT_DIRECTORY] [--dry-run|--force] [--output text|json]");
        writer.WriteLine();
        writer.WriteLine("Inspect and validate are read-only. Init never overwrites an existing manifest unless --force is provided.");
    }
}
