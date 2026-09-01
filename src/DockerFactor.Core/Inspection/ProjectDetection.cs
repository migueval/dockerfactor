namespace DockerFactor.Core.Inspection;

public sealed record ProjectDetection(string? Runtime, string? Evidence)
{
    public bool WasDetected => Runtime is not null;
}
