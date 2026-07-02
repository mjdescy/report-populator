namespace ReportPopulator.Library;

/// <summary>
/// Represents a single mapping from a source worksheet region to a destination worksheet cell.
/// </summary>
/// <param name="SourceFilePath">Path to the source .xlsx file.</param>
/// <param name="DestinationFilePath">Path to the destination .xlsx file. Created if it does not exist.</param>
/// <param name="DestinationWorksheet">Name of the target worksheet in the destination file. Created if it does not exist.</param>
/// <param name="DestinationCellAddress">Top-left cell where the source range is pasted (e.g. "A4").</param>
public sealed record PopulatorRecord(
    string SourceFilePath,
    string DestinationFilePath,
    string DestinationWorksheet,
    string DestinationCellAddress
);
