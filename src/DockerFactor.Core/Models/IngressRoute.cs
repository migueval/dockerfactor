namespace DockerFactor.Core.Models;

/// <summary>
/// Represents a public HTTPS ingress route bound to an internal container service endpoint.
/// </summary>
public record IngressRoute(
    string PublicUrl,
    string InternalTargetService,
    string ProviderName,
    DateTime CreatedAtUtc,
    int? ProcessId = null
);
