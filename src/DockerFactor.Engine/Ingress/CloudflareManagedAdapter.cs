using System.Diagnostics;
using DockerFactor.Core.Ingress;
using DockerFactor.Core.Models;

namespace DockerFactor.Engine.Ingress;

/// <summary>
/// Managed production Ingress adapter using Cloudflare Tunnel Tokens and custom subdomains.
/// </summary>
public class CloudflareManagedAdapter : IIngressAdapter
{
    private readonly string _tunnelToken;
    private readonly string? _customHostname;

    public CloudflareManagedAdapter(string tunnelToken, string? customHostname = null)
    {
        _tunnelToken = tunnelToken ?? throw new ArgumentNullException(nameof(tunnelToken));
        _customHostname = customHostname;
    }

    public string ProviderName => "Cloudflare Managed Tunnel";

    public Task<IngressRoute> CreateRouteAsync(string internalTargetService, CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cloudflared",
            Arguments = "tunnel run",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        psi.EnvironmentVariables["TUNNEL_TOKEN"] = _tunnelToken;

        var process = new Process { StartInfo = psi };

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start cloudflared with managed token.");
        }

        var publicUrl = _customHostname != null ? $"https://{_customHostname}" : "https://managed.cloudflare.tunnel";

        return Task.FromResult(new IngressRoute(
            PublicUrl: publicUrl,
            InternalTargetService: internalTargetService,
            ProviderName: ProviderName,
            CreatedAtUtc: DateTime.UtcNow,
            ProcessId: process.Id
        ));
    }

    public Task RevokeRouteAsync(IngressRoute route, CancellationToken cancellationToken = default)
    {
        if (route.ProcessId.HasValue)
        {
            try
            {
                var process = Process.GetProcessById(route.ProcessId.Value);
                if (!process.HasExited)
                {
                    process.Kill(entireProcessTree: true);
                }
            }
            catch (Exception)
            {
                // Process already terminated or inaccessible
            }
        }

        return Task.CompletedTask;
    }
}
