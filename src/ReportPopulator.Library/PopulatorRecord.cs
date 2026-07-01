namespace ReportPopulator.Library;

public sealed record PopulatorRecord(
    string SourceFilePath,
    string DestinationFilePath,
    string DestinationWorksheet,
    string DestinationCellAddress
);
