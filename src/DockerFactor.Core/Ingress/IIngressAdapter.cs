using DockerFactor.Core.Models;

namespace DockerFactor.Core.Ingress;

/// <summary>
/// Abstract interface for ingress tunnel adapters (Cloudflare Quick Tunnels, Cloudflare Managed, Tailscale, etc.).
/// Decouples public HTTPS endpoint binding from underlying network providers.
/// </summary>
public interface IIngressAdapter
{
    /// <summary>
    /// Gets the human-readable provider name (e.g. "Cloudflare QuickTunnel", "Cloudflare Managed").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Binds an internal target service (e.g. "http://localhost:8080") to a public HTTPS endpoint.
    /// </summary>
    /// <param name="internalTargetService">Local service URL or container endpoint.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>An IngressRoute containing the public HTTPS URL and metadata.</returns>
    Task<IngressRoute> CreateRouteAsync(string internalTargetService, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes an active public route and shuts down associated tunnel processes.
    /// </summary>
    /// <param name="route">Active route to revoke.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task RevokeRouteAsync(IngressRoute route, CancellationToken cancellationToken = default);
}
