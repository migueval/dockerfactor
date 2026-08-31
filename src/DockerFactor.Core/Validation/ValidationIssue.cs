namespace DockerFactor.Core.Validation;

public sealed record ValidationIssue(string Code, string Path, string Message);
