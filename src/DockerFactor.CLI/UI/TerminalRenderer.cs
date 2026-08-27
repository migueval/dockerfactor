using Spectre.Console;

namespace DockerFactor.CLI.UI;

public static class TerminalRenderer
{
    public static void RenderHeader()
    {
        AnsiConsole.Write(
            new FigletText("DockerFactor")
                .Color(Color.Aqua));

        AnsiConsole.MarkupLine("[grey]Zero-Inbound-Port VPS Provisioning & Hardened Container CLI[/]");
        AnsiConsole.MarkupLine("[dim]------------------------------------------------------------[/]");
        AnsiConsole.WriteLine();
    }

    public static void RenderSuccessBanner(string title, string details)
    {
        var panel = new Panel(new Markup($"[bold green]{Markup.Escape(title)}[/]\n[white]{Markup.Escape(details)}[/]"))
        {
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Green),
            Padding = new Padding(1, 1, 1, 1)
        };

        AnsiConsole.Write(panel);
    }

    public static void RenderTunnelCard(string publicUrl, string internalService, string provider)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Aqua)
            .AddColumn(new TableColumn("[bold yellow]Property[/]").Centered())
            .AddColumn(new TableColumn("[bold yellow]Value[/]"));

        table.AddRow("[bold white]Status[/]", "[bold green]ACTIVE & ENCRYPTED[/]");
        table.AddRow("[bold white]Public Route[/]", $"[bold cyan]{Markup.Escape(publicUrl)}[/]");
        table.AddRow("[bold white]Target Service[/]", $"[white]{Markup.Escape(internalService)}[/]");
        table.AddRow("[bold white]Provider[/]", $"[grey]{Markup.Escape(provider)}[/]");
        table.AddRow("[bold white]Inbound Ports[/]", "[bold green]0 Exposed Public Ports[/]");

        AnsiConsole.Write(table);
    }

    public static void RenderAuditReport(int score, List<(string Check, bool Passed, string Details)> results)
    {
        var scoreColor = score >= 90 ? "green" : (score >= 70 ? "yellow" : "red");

        AnsiConsole.MarkupLine($"[bold]Zero-Trust Security Score:[/] [{scoreColor} bold]{score}/100[/]");
        AnsiConsole.WriteLine();

        var table = new Table()
            .Border(TableBorder.SimpleHeavy)
            .AddColumn("[bold]Security Control[/]")
            .AddColumn("[bold]Status[/]")
            .AddColumn("[bold]Details[/]");

        foreach (var (check, passed, details) in results)
        {
            var statusStr = passed ? "[green]PASS[/]" : "[red]FAIL[/]";
            table.AddRow(check, statusStr, details);
        }

        AnsiConsole.Write(table);
    }
}
