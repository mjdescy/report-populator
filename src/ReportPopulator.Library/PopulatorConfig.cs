namespace ReportPopulator.Library;

/// <summary>
/// Contains a collection of report population mappings.
/// </summary>
public sealed class PopulatorConfig
{
    /// <summary>
    /// The list of source-to-destination mappings to process.
    /// </summary>
    public List<PopulatorRecord> Mappings { get; init; } = [];
}
