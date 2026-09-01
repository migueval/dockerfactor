using DockerFactor.Core.Inspection;

namespace DockerFactor.Engine.Inspection;

public sealed class ProjectRuntimeDetector
{
    private const int MaximumPackageJsonBytes = 1024 * 1024;

    public ProjectDetection Detect(string projectDirectory)
    {
        if (!Directory.Exists(projectDirectory))
            return new(null, null);

        var dotnetFile = Directory.EnumerateFiles(projectDirectory, "*.csproj", SearchOption.TopDirectoryOnly)
            .Concat(Directory.EnumerateFiles(projectDirectory, "*.slnx", SearchOption.TopDirectoryOnly))
            .FirstOrDefault();
        if (dotnetFile is not null)
            return new("dotnet", Path.GetFileName(dotnetFile));

        var packageJson = Path.Combine(projectDirectory, "package.json");
        if (File.Exists(packageJson))
        {
            var runtime = DetectJavaScriptRuntime(packageJson);
            return new(runtime, "package.json");
        }

        if (File.Exists(Path.Combine(projectDirectory, "go.mod")))
            return new("go", "go.mod");

        if (File.Exists(Path.Combine(projectDirectory, "pyproject.toml")))
            return new("python", "pyproject.toml");

        if (File.Exists(Path.Combine(projectDirectory, "requirements.txt")))
            return new("python", "requirements.txt");

        return new(null, null);
    }

    private static string DetectJavaScriptRuntime(string packageJson)
    {
        var info = new FileInfo(packageJson);
        if (info.Length > MaximumPackageJsonBytes)
            return "node";

        try
        {
            var content = File.ReadAllText(packageJson);
            if (content.Contains("@angular/core", StringComparison.OrdinalIgnoreCase))
                return "angular";
            if (content.Contains("@nestjs/core", StringComparison.OrdinalIgnoreCase))
                return "nestjs";
        }
        catch (IOException)
        {
            return "node";
        }
        catch (UnauthorizedAccessException)
        {
            return "node";
        }

        return "node";
    }
}
