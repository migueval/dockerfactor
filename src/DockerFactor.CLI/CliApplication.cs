using DockerFactor.Core.Inspection;
using DockerFactor.Core.Validation;
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
        writer.WriteLine();
        writer.WriteLine("Both commands are read-only. Strict mode treats warnings as validation failures.");
    }
}
