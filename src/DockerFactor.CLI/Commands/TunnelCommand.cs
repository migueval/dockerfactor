using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using DockerFactor.CLI.UI;
using DockerFactor.Engine.Ingress;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DockerFactor.CLI.Commands;

public class TunnelCommandSettings : CommandSettings
{
    [CommandArgument(0, "[TARGET]")]
    [Description("Target service URL or container endpoint (e.g. http://localhost:8080)")]
    public string Target { get; init; } = "http://localhost:8080";
}

public class TunnelCommand : AsyncCommand<TunnelCommandSettings>
{
    public override async Task<int> ExecuteAsync([NotNull] CommandContext context, [NotNull] TunnelCommandSettings settings)
    {
        TerminalRenderer.RenderHeader();

        AnsiConsole.MarkupLine($"[bold blue]Initiating QuickTunnel toward target:[/] [cyan]{settings.Target}[/]");

        var adapter = new QuickTunnelAdapter();

        try
        {
            var route = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Earth)
                .StartAsync("Deploying Cloudflare QuickTunnel (trycloudflare.com)...", async _ =>
                {
                    return await adapter.CreateRouteAsync(settings.Target);
                });

            AnsiConsole.WriteLine();
            TerminalRenderer.RenderTunnelCard(route.PublicUrl, route.InternalTargetService, route.ProviderName);

            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold yellow]Tunnel active. Press [red]Ctrl + C[/] or Enter to stop the tunnel.[/]");

            Console.ReadLine();

            AnsiConsole.MarkupLine("[grey]Revoking tunnel and terminating background processes...[/]");
            await adapter.RevokeRouteAsync(route);

            AnsiConsole.MarkupLine("[bold green]Tunnel revoked cleanly.[/]");
            return 0;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[bold red]Error deploying tunnel:[/] {ex.Message}");
            return 1;
        }
    }
}
