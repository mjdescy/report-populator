using System.Text.Json;
using ClosedXML.Excel;

namespace ReportPopulator.Library;

/// <summary>
/// Reads configuration, processes report population mappings, and generates sample config files.
/// </summary>
public sealed class PopulatorService
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    /// <summary>
    /// Loads a population configuration from a JSON file.
    /// </summary>
    /// <param name="configFilePath">Path to the configuration JSON file.</param>
    /// <returns>A <see cref="PopulatorConfig"/> containing the parsed mappings.</returns>
    public PopulatorConfig LoadConfig(string configFilePath)
    {
        var json = File.ReadAllText(configFilePath);
        var mappings = JsonSerializer.Deserialize<List<PopulatorRecord>>(json, _jsonOptions);
        return new PopulatorConfig { Mappings = mappings ?? [] };
    }

    /// <summary>
    /// Writes a sample configuration file with example mappings.
    /// </summary>
    /// <param name="outputFilePath">Path where the sample config file will be written. Intermediate directories are created if needed.</param>
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

        var json = JsonSerializer.Serialize(config.Mappings, _jsonOptions);
        File.WriteAllText(outputFilePath, json);
    }

    /// <summary>
    /// Executes all mappings in the configuration, copying source ranges to destination workbooks.
    /// </summary>
    /// <param name="config">The population configuration to execute.</param>
    /// <param name="baseDirectory">
    /// Optional base directory for resolving relative paths. When <c>null</c>, paths are used as-is.
    /// </param>
    public void Run(PopulatorConfig config, string? baseDirectory = null)
    {
        foreach (var mapping in config.Mappings)
        {
            ProcessMapping(mapping, baseDirectory);
        }
    }

    /// <summary>
    /// Loads a configuration file and runs the population, resolving relative source and destination
    /// paths against the config file's directory.
    /// </summary>
    /// <param name="configFilePath">Path to the configuration JSON file.</param>
    public void RunFromConfigFile(string configFilePath)
    {
        var config = LoadConfig(configFilePath);
        var baseDirectory = Path.GetDirectoryName(Path.GetFullPath(configFilePath));
        Run(config, baseDirectory);
    }

    private static void ProcessMapping(PopulatorRecord mapping, string? baseDirectory)
    {
        var sourcePath = ResolvePath(mapping.SourceFilePath, baseDirectory);
        var destinationPath = ResolvePath(mapping.DestinationFilePath, baseDirectory);

        using var sourceWorkbook = new XLWorkbook(sourcePath);
        var sourceWorksheet = sourceWorkbook.Worksheet(1);
        var sourceRange = sourceWorksheet.RangeUsed();

        using var destinationWorkbook = File.Exists(destinationPath)
            ? new XLWorkbook(destinationPath)
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

        destinationWorkbook.SaveAs(destinationPath);
    }

    private static string ResolvePath(string path, string? baseDirectory)
    {
        if (baseDirectory is null || Path.IsPathRooted(path))
            return path;

        return Path.GetFullPath(Path.Combine(baseDirectory, path));
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
