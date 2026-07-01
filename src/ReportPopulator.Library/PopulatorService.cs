using System.Text.Json;
using ClosedXML.Excel;

namespace ReportPopulator.Library;

public sealed class PopulatorService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public PopulatorConfig LoadConfig(string configFilePath)
    {
        var json = File.ReadAllText(configFilePath);
        var config = JsonSerializer.Deserialize<PopulatorConfig>(json, JsonOptions);
        return config ?? new PopulatorConfig();
    }

    public void GenerateSampleConfig(string outputFilePath)
    {
        var config = new PopulatorConfig
        {
            Mappings =
            [
                new PopulatorRecord(
                    SourceFilePath: "path/to/source.xlsx",
                    DestinationFilePath: "path/to/destination.xlsx",
                    DestinationWorksheet: "Sheet1",
                    DestinationCellAddress: "A4"
                ),
                new PopulatorRecord(
                    SourceFilePath: "path/to/another-source.xlsx",
                    DestinationFilePath: "path/to/destination.xlsx",
                    DestinationWorksheet: "Sheet2",
                    DestinationCellAddress: "B2"
                )
            ]
        };

        var directory = Path.GetDirectoryName(outputFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(config, JsonOptions);
        File.WriteAllText(outputFilePath, json);
    }

    public void Run(PopulatorConfig config)
    {
        foreach (var mapping in config.Mappings)
        {
            ProcessMapping(mapping);
        }
    }

    private static void ProcessMapping(PopulatorRecord mapping)
    {
        using var sourceWorkbook = new XLWorkbook(mapping.SourceFilePath);
        var sourceWorksheet = sourceWorkbook.Worksheet(1);
        var sourceRange = sourceWorksheet.RangeUsed();

        using var destinationWorkbook = File.Exists(mapping.DestinationFilePath)
            ? new XLWorkbook(mapping.DestinationFilePath)
            : new XLWorkbook();

        IXLWorksheet destinationWorksheet;
        if (destinationWorkbook.TryGetWorksheet(mapping.DestinationWorksheet, out var existingWorksheet))
        {
            destinationWorksheet = existingWorksheet;
        }
        else
        {
            destinationWorksheet = destinationWorkbook.AddWorksheet(mapping.DestinationWorksheet);
        }

        if (sourceRange is not null)
        {
            var destinationCell = destinationWorksheet.Cell(mapping.DestinationCellAddress);
            CopyRange(sourceRange, destinationCell);
        }

        destinationWorkbook.SaveAs(mapping.DestinationFilePath);
    }

    private static void CopyRange(IXLRange sourceRange, IXLCell destinationCell)
    {
        var destinationWorksheet = destinationCell.Worksheet;
        var startRow = destinationCell.Address.RowNumber;
        var startColumn = destinationCell.Address.ColumnNumber;

        foreach (var row in sourceRange.RowsUsed())
        {
            foreach (var cell in row.CellsUsed())
            {
                var destRow = startRow + (row.RowNumber() - sourceRange.FirstRow().RowNumber());
                var destCol = startColumn + (cell.Address.ColumnNumber - sourceRange.FirstColumn().ColumnNumber());
                var targetCell = destinationWorksheet.Cell(destRow, destCol);
                targetCell.Value = cell.Value;
            }
        }
    }
}
