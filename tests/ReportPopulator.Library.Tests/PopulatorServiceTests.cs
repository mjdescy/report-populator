using System.Text.Json;
using ClosedXML.Excel;
using ReportPopulator.Library;

namespace ReportPopulator.Library.Tests;

public sealed class PopulatorServiceTests
{
    [Fact]
    public void Run_CopiesDataToCorrectCell()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var sourcePath = Path.Combine(tempDir, "source.xlsx");
            var destPath = Path.Combine(tempDir, "dest.xlsx");
            var configPath = Path.Combine(tempDir, "config.json");

            CreateSourceWorkbook(sourcePath);

            var config = new PopulatorConfig
            {
                Mappings =
                [
                    new PopulatorRecord(
                        SourceFilePath: sourcePath,
                        DestinationFilePath: destPath,
                        DestinationWorksheet: "Report",
                        DestinationCellAddress: "A4"
                    )
                ]
            };

            var service = new PopulatorService();
            service.Run(config);

            Assert.True(File.Exists(destPath));

            using var destWorkbook = new XLWorkbook(destPath);
            var destWorksheet = destWorkbook.Worksheet("Report");

            Assert.Equal("Header1", destWorksheet.Cell("A4").GetString());
            Assert.Equal("Header2", destWorksheet.Cell("B4").GetString());
            Assert.Equal("Header3", destWorksheet.Cell("C4").GetString());
            Assert.Equal("Value1", destWorksheet.Cell("A5").GetString());
            Assert.Equal("Value2", destWorksheet.Cell("B5").GetString());
            Assert.Equal("Value3", destWorksheet.Cell("C5").GetString());
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Run_CreatesDestinationFileIfNotExists()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var sourcePath = Path.Combine(tempDir, "source.xlsx");
            var destPath = Path.Combine(tempDir, "nonexistent.xlsx");

            CreateSourceWorkbook(sourcePath);

            var config = new PopulatorConfig
            {
                Mappings =
                [
                    new PopulatorRecord(
                        SourceFilePath: sourcePath,
                        DestinationFilePath: destPath,
                        DestinationWorksheet: "Sheet1",
                        DestinationCellAddress: "A1"
                    )
                ]
            };

            Assert.False(File.Exists(destPath));

            var service = new PopulatorService();
            service.Run(config);

            Assert.True(File.Exists(destPath));
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Run_HandlesMultipleRecordsToSameDestination()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var source1Path = Path.Combine(tempDir, "source1.xlsx");
            var source2Path = Path.Combine(tempDir, "source2.xlsx");
            var destPath = Path.Combine(tempDir, "dest.xlsx");

            CreateSourceWorkbook(source1Path, new[] { "DataA1", "DataB1" }, new[] { "DataA2", "DataB2" });
            CreateSourceWorkbook(source2Path, new[] { "Foo", "Bar" });

            var config = new PopulatorConfig
            {
                Mappings =
                [
                    new PopulatorRecord(
                        SourceFilePath: source1Path,
                        DestinationFilePath: destPath,
                        DestinationWorksheet: "Report",
                        DestinationCellAddress: "A1"
                    ),
                    new PopulatorRecord(
                        SourceFilePath: source2Path,
                        DestinationFilePath: destPath,
                        DestinationWorksheet: "Report",
                        DestinationCellAddress: "D1"
                    )
                ]
            };

            var service = new PopulatorService();
            service.Run(config);

            Assert.True(File.Exists(destPath));

            using var destWorkbook = new XLWorkbook(destPath);
            var ws = destWorkbook.Worksheet("Report");

            Assert.Equal("DataA1", ws.Cell("A1").GetString());
            Assert.Equal("DataB1", ws.Cell("B1").GetString());
            Assert.Equal("DataA2", ws.Cell("A2").GetString());
            Assert.Equal("DataB2", ws.Cell("B2").GetString());
            Assert.Equal("Foo", ws.Cell("D1").GetString());
            Assert.Equal("Bar", ws.Cell("E1").GetString());
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Run_EmptySourceRange_DoesNotThrow()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var sourcePath = Path.Combine(tempDir, "empty.xlsx");
            var destPath = Path.Combine(tempDir, "dest.xlsx");

            using (var wb = new XLWorkbook())
            {
                wb.AddWorksheet("Sheet1");
                wb.SaveAs(sourcePath);
            }

            var config = new PopulatorConfig
            {
                Mappings =
                [
                    new PopulatorRecord(
                        SourceFilePath: sourcePath,
                        DestinationFilePath: destPath,
                        DestinationWorksheet: "Sheet1",
                        DestinationCellAddress: "A1"
                    )
                ]
            };

            var service = new PopulatorService();

            var exception = Record.Exception(() => service.Run(config));
            Assert.Null(exception);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Run_DifferentDestinationCellOffset()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var sourcePath = Path.Combine(tempDir, "source.xlsx");
            var destPath = Path.Combine(tempDir, "dest.xlsx");

            CreateSourceWorkbook(sourcePath, new[] { "X1", "X2", "X3" }, new[] { "Y1", "Y2", "Y3" });

            var config = new PopulatorConfig
            {
                Mappings =
                [
                    new PopulatorRecord(
                        SourceFilePath: sourcePath,
                        DestinationFilePath: destPath,
                        DestinationWorksheet: "Sheet1",
                        DestinationCellAddress: "C5"
                    )
                ]
            };

            var service = new PopulatorService();
            service.Run(config);

            using var destWorkbook = new XLWorkbook(destPath);
            var ws = destWorkbook.Worksheet("Sheet1");

            Assert.Equal("X1", ws.Cell("C5").GetString());
            Assert.Equal("X2", ws.Cell("D5").GetString());
            Assert.Equal("X3", ws.Cell("E5").GetString());
            Assert.Equal("Y1", ws.Cell("C6").GetString());
            Assert.Equal("Y2", ws.Cell("D6").GetString());
            Assert.Equal("Y3", ws.Cell("E6").GetString());
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void RunFromConfigFile_ResolvesRelativePaths()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var sourcePath = Path.Combine(tempDir, "source.xlsx");
            var destPath = Path.Combine(tempDir, "dest.xlsx");
            var configPath = Path.Combine(tempDir, "config.json");

            CreateSourceWorkbook(sourcePath, new[] { "RelA", "RelB" }, new[] { "RelC", "RelD" });

            var config = new PopulatorConfig
            {
                Mappings =
                [
                    new PopulatorRecord(
                        SourceFilePath: "source.xlsx",
                        DestinationFilePath: "./dest.xlsx",
                        DestinationWorksheet: "Sheet1",
                        DestinationCellAddress: "A1"
                    )
                ]
            };

            var json = JsonSerializer.Serialize(config,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
            File.WriteAllText(configPath, json);

            var service = new PopulatorService();
            service.RunFromConfigFile(configPath);

            using var destWorkbook = new XLWorkbook(destPath);
            var ws = destWorkbook.Worksheet("Sheet1");

            Assert.Equal("RelA", ws.Cell("A1").GetString());
            Assert.Equal("RelB", ws.Cell("B1").GetString());
            Assert.Equal("RelC", ws.Cell("A2").GetString());
            Assert.Equal("RelD", ws.Cell("B2").GetString());
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static void CreateSourceWorkbook(string path, string[]? row1 = null, string[]? row2 = null)
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Sheet1");

        var headers = row1 ?? new[] { "Header1", "Header2", "Header3" };
        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }

        var values = row2 ?? new[] { "Value1", "Value2", "Value3" };
        for (var i = 0; i < values.Length; i++)
        {
            ws.Cell(2, i + 1).Value = values[i];
        }

        wb.SaveAs(path);
    }
}
