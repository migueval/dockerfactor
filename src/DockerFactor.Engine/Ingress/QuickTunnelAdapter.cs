using System.Diagnostics;
using System.Text.RegularExpressions;
using DockerFactor.Core.Ingress;
using DockerFactor.Core.Models;

namespace DockerFactor.Engine.Ingress;

/// <summary>
/// Free domain-less Ingress adapter using Cloudflare Quick Tunnels (trycloudflare.com).
/// </summary>
public class QuickTunnelAdapter : IIngressAdapter
{
    private static readonly Regex TryCloudflareRegex = new(
        @"https://[a-zA-Z0-9-]+\.trycloudflare\.com",
        RegexOptions.Compiled | RegexOptions.IgnoreCase
    );

    public string ProviderName => "Cloudflare QuickTunnel (trycloudflare.com)";

    public async Task<IngressRoute> CreateRouteAsync(string internalTargetService, CancellationToken cancellationToken = default)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "cloudflared",
            Arguments = $"tunnel --url {internalTargetService}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        var process = new Process { StartInfo = psi };
        var tcs = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        DataReceivedEventHandler handler = (sender, e) =>
        {
            if (string.IsNullOrWhiteSpace(e.Data)) return;

            var match = TryCloudflareRegex.Match(e.Data);
            if (match.Success)
            {
                tcs.TrySetResult(match.Value);
            }
        };

        process.OutputDataReceived += handler;
        process.ErrorDataReceived += handler;

        if (!process.Start())
        {
            throw new InvalidOperationException("Failed to start cloudflared binary. Ensure 'cloudflared' is installed and available on PATH.");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        using (cts.Token.Register(() => tcs.TrySetException(new TimeoutException("Timed out waiting for trycloudflare.com tunnel URL."))))
        {
            var publicUrl = await tcs.Task;

            return new IngressRoute(
                PublicUrl: publicUrl,
                InternalTargetService: internalTargetService,
                ProviderName: ProviderName,
                CreatedAtUtc: DateTime.UtcNow,
                ProcessId: process.Id
            );
        }
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
