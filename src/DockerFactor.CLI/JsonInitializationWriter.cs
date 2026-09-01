using System.Text;
using System.Text.Json;
using DockerFactor.Core.Initialization;

namespace DockerFactor.CLI;

internal static class JsonInitializationWriter
{
    public static void Write(TextWriter output, ManifestProposal proposal, string status)
    {
        using var stream = new MemoryStream();
        using (var json = new Utf8JsonWriter(stream, new() { Indented = true }))
        {
            json.WriteStartObject();
            json.WriteString("status", status);
            json.WriteString("projectDirectory", proposal.ProjectDirectory);
            json.WriteString("manifestPath", proposal.ManifestPath);
            json.WriteString("detectedRuntime", proposal.Detection.Runtime);
            json.WriteString("evidence", proposal.Detection.Evidence);
            json.WritePropertyName("manifest");
            json.WriteStartObject();
            json.WriteString("apiVersion", proposal.Manifest.ApiVersion);
            json.WriteString("kind", proposal.Manifest.Kind);
            json.WriteString("name", proposal.Manifest.Metadata.Name);
            json.WriteString("runtime", proposal.Manifest.Spec.Runtime);
            json.WriteNumber("port", proposal.Manifest.Spec.Port);
            json.WriteString("build", proposal.Manifest.Spec.Build);
            json.WriteString("command", proposal.Manifest.Spec.Command);
            json.WriteEndObject();
            json.WriteString("yaml", proposal.Content);
            json.WriteEndObject();
        }

        output.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
    }
}
