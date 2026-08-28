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
        if (!Uri.TryCreate(internalTargetService, UriKind.Absolute, out var validatedUri) ||
            (validatedUri.Scheme != Uri.UriSchemeHttp && validatedUri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException("Target service must be a valid HTTP or HTTPS URL.", nameof(internalTargetService));
        }

        var fileName = "cloudflared";
        var arguments = $"tunnel --url {validatedUri.AbsoluteUri}";

        if (OperatingSystem.IsWindows() && !IsCommandAvailable("cloudflared"))
        {
            fileName = "wsl";
            var targetDistro = GetTargetWslDistro();
            arguments = string.IsNullOrEmpty(targetDistro)
                ? $"cloudflared tunnel --url {validatedUri.AbsoluteUri}"
                : $"-d {targetDistro} cloudflared tunnel --url {validatedUri.AbsoluteUri}";
        }

        var psi = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
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

        try
        {
            if (!process.Start())
            {
                throw new InvalidOperationException("Failed to start cloudflared process.");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                "Could not execute 'cloudflared'. Ensure cloudflared is installed on PATH or inside WSL (run 'winget install Cloudflare.cloudflared' or install via WSL script).",
                ex
            );
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
                InternalTargetService: validatedUri.AbsoluteUri,
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
                if (!process.HasExited && (process.ProcessName.Contains("cloudflared", StringComparison.OrdinalIgnoreCase) || process.ProcessName.Contains("wsl", StringComparison.OrdinalIgnoreCase)))
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

    private static string? GetTargetWslDistro()
    {
        if (!OperatingSystem.IsWindows()) return null;

        try
        {
            using var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "wsl",
                Arguments = "--list --quiet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (proc == null) return null;

            var output = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            var distros = output
                .Replace("\0", "")
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(d => d.Trim())
                .Where(d => !string.IsNullOrWhiteSpace(d) &&
                            !d.StartsWith("docker-desktop", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return distros.FirstOrDefault();
        }
        catch
        {
            return null;
        }
    }

    private static bool IsCommandAvailable(string command)
    {
        try
        {
            using var proc = Process.Start(new ProcessStartInfo
            {
                FileName = OperatingSystem.IsWindows() ? "where" : "which",
                Arguments = command,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            proc?.WaitForExit();
            return proc?.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }
}
