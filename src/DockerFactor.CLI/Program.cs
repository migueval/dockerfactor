using DockerFactor.Engine.Inspection;

return Run(args);

static int Run(string[] args)
{
    if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
    {
        PrintHelp();
        return 0;
    }

    if (!string.Equals(args[0], "inspect", StringComparison.OrdinalIgnoreCase))
    {
        Console.Error.WriteLine($"Unknown command: {args[0]}");
        PrintHelp();
        return 64;
    }

    var directory = args.Length > 1 ? args[1] : ".";
    var inspection = new ProjectInspector().Inspect(directory);

    Console.WriteLine("DockerFactor project inspection");
    Console.WriteLine($"Project:  {inspection.ProjectDirectory}");
    Console.WriteLine($"Manifest: {inspection.ManifestPath}");

    if (!inspection.Validation.IsValid)
    {
        Console.Error.WriteLine("Result: invalid");
        foreach (var issue in inspection.Validation.Issues)
            Console.Error.WriteLine($"  {issue.Code} {issue.Path}: {issue.Message}");
        return 2;
    }

    var manifest = inspection.Validation.Manifest!;
    Console.WriteLine("Result:   valid");
    Console.WriteLine($"App:      {manifest.Metadata.Name}");
    Console.WriteLine($"Runtime:  {manifest.Spec.Runtime}");
    Console.WriteLine($"Port:     {manifest.Spec.Port}");
    return 0;
}

static void PrintHelp()
{
    Console.WriteLine("DockerFactor — early-stage hardened deployment CLI");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  docker-factor inspect [PROJECT_DIRECTORY]");
    Console.WriteLine();
    Console.WriteLine("The inspect command is read-only and validates dockerfactor.yaml.");
}
