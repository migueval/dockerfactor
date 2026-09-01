using System.Text;
using System.Text.Json;
using DockerFactor.Core.Inspection;

namespace DockerFactor.CLI;

internal static class JsonInspectionWriter
{
    public static void Write(TextWriter output, ProjectInspection inspection, bool effectiveValid)
    {
        using var stream = new MemoryStream();
        using (var json = new Utf8JsonWriter(stream, new() { Indented = true }))
        {
            json.WriteStartObject();
            json.WriteBoolean("valid", effectiveValid);
            json.WriteString("projectDirectory", inspection.ProjectDirectory);
            json.WriteString("manifestPath", inspection.ManifestPath);

            json.WritePropertyName("detection");
            json.WriteStartObject();
            if (inspection.Detection.Runtime is null) json.WriteNull("runtime");
            else json.WriteString("runtime", inspection.Detection.Runtime);
            if (inspection.Detection.Evidence is null) json.WriteNull("evidence");
            else json.WriteString("evidence", inspection.Detection.Evidence);
            json.WriteEndObject();

            json.WritePropertyName("manifest");
            var manifest = inspection.Validation.Manifest;
            if (manifest is null)
            {
                json.WriteNullValue();
            }
            else
            {
                json.WriteStartObject();
                json.WriteString("apiVersion", manifest.ApiVersion);
                json.WriteString("kind", manifest.Kind);
                json.WriteString("name", manifest.Metadata?.Name);
                json.WriteString("runtime", manifest.Spec?.Runtime);
                if (manifest.Spec is null) json.WriteNull("port");
                else json.WriteNumber("port", manifest.Spec.Port);
                json.WriteEndObject();
            }

            json.WritePropertyName("issues");
            json.WriteStartArray();
            foreach (var issue in inspection.Validation.Issues)
            {
                json.WriteStartObject();
                json.WriteString("code", issue.Code);
                json.WriteString("severity", issue.Severity.ToString().ToLowerInvariant());
                json.WriteString("path", issue.Path);
                json.WriteString("message", issue.Message);
                json.WriteEndObject();
            }
            json.WriteEndArray();
            json.WriteEndObject();
        }

        output.WriteLine(Encoding.UTF8.GetString(stream.ToArray()));
    }
}
