using System.Text.Json.Serialization;

namespace ReportPopulator.Library;

public sealed class PopulatorConfig
{
    [JsonPropertyName("mappings")]
    public List<PopulatorRecord> Mappings { get; init; } = [];
}
