namespace DockerFactor.CLI;

internal sealed record CliOptions(string Command, string Directory, bool Json, bool Strict)
{
    public static bool TryParse(string[] args, out CliOptions? options, out string? error)
    {
        options = null;
        error = null;

        if (args.Length == 0 || args[0] is "--help" or "-h" or "help")
            return true;

        var command = args[0].ToLowerInvariant();
        if (command is not ("inspect" or "validate"))
        {
            error = $"Unknown command: {args[0]}";
            return false;
        }

        var directory = ".";
        var json = false;
        var strict = false;
        var directoryAssigned = false;

        for (var index = 1; index < args.Length; index++)
        {
            var argument = args[index];
            if (argument == "--strict")
            {
                if (command != "validate")
                {
                    error = "--strict is only valid with the validate command.";
                    return false;
                }
                strict = true;
                continue;
            }

            if (argument == "--output")
            {
                if (++index >= args.Length || args[index] is not ("json" or "text"))
                {
                    error = "--output requires either 'json' or 'text'.";
                    return false;
                }
                json = args[index] == "json";
                continue;
            }

            if (argument.StartsWith("-", StringComparison.Ordinal))
            {
                error = $"Unknown option: {argument}";
                return false;
            }

            if (directoryAssigned)
            {
                error = "Only one project directory can be provided.";
                return false;
            }

            directory = argument;
            directoryAssigned = true;
        }

        options = new(command, directory, json, strict);
        return true;
    }
}
