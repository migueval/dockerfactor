using DockerFactor.CLI.Commands;
using Spectre.Console.Cli;

var app = new CommandApp();

app.Configure(config =>
{
    config.SetApplicationName("docker-factor");

    config.AddCommand<ConnectCommand>("connect")
        .WithDescription("Connect and pair local CLI with target VPS using pairing token");

    config.AddCommand<TunnelCommand>("tunnel")
        .WithDescription("Deploy instant Cloudflare QuickTunnel (trycloudflare.com) toward local target");

    config.AddCommand<AuditCommand>("audit")
        .WithDescription("Scan and evaluate host & container Zero-Trust security posture");
});

return await app.RunAsync(args);
