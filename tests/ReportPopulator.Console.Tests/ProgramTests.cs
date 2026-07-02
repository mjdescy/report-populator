using System.Text.Json;
using ReportPopulator.Library;

namespace ReportPopulator.Console.Tests;

public sealed class ProgramTests
{
    [Fact]
    public void Init_NoPath_CreatesSampleConfigInCurrentDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        var originalCwd = Environment.CurrentDirectory;

        try
        {
            Environment.CurrentDirectory = tempDir;

            var exitCode = Program.Main(["init"]);
            Assert.Equal(0, exitCode);

            var expectedPath = Path.Combine(tempDir, "sample-config.json");
            Assert.True(File.Exists(expectedPath));
        }
        finally
        {
            Environment.CurrentDirectory = originalCwd;
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Init_WithExplicitPath_CreatesFileAtPath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var outputPath = Path.Combine(tempDir, "my-config.json");
            var exitCode = Program.Main(["init", outputPath]);

            Assert.Equal(0, exitCode);
            Assert.True(File.Exists(outputPath));

            var json = File.ReadAllText(outputPath);
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var mappings = JsonSerializer.Deserialize<List<PopulatorRecord>>(json, options);

            Assert.NotNull(mappings);
            Assert.NotEmpty(mappings);
        }
        finally
        {
            CleanupDefaultSampleConfig();
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Init_CreatesValidJson()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var outputPath = Path.Combine(tempDir, "config.json");
            var exitCode = Program.Main(["init", outputPath]);

            Assert.Equal(0, exitCode);

            var json = File.ReadAllText(outputPath);
            Assert.Contains("\"sourceFilePath\"", json);
            Assert.Contains("\"destinationFilePath\"", json);
            Assert.Contains("\"destinationWorksheet\"", json);
            Assert.Contains("\"destinationCellAddress\"", json);
        }
        finally
        {
            CleanupDefaultSampleConfig();
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void Run_MissingConfigPath_ReturnsError()
    {
        var exitCode = Program.Main(["run"]);
        Assert.NotEqual(0, exitCode);
    }

    [Fact]
    public void Run_NoArgs_ReturnsError()
    {
        var exitCode = Program.Main([]);
        Assert.NotEqual(0, exitCode);
    }

    [Fact]
    public void Run_EndToEnd_PopulatesDestination()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var sourcePath = Path.Combine(tempDir, "source.xlsx");
            var destPath = Path.Combine(tempDir, "dest.xlsx");
            var configPath = Path.Combine(tempDir, "config.json");

            using (var wb = new ClosedXML.Excel.XLWorkbook())
            {
                var sheet = wb.AddWorksheet("Data");
                sheet.Cell("A1").Value = "Name";
                sheet.Cell("B1").Value = "Age";
                sheet.Cell("A2").Value = "Alice";
                sheet.Cell("B2").Value = 30;
                wb.SaveAs(sourcePath);
            }

            var config = new PopulatorConfig
            {
                Mappings =
                [
                    new PopulatorRecord(
                        SourceFilePath: sourcePath,
                        DestinationFilePath: destPath,
                        DestinationWorksheet: "Report",
                        DestinationCellAddress: "B3"
                    )
                ]
            };
            var json = JsonSerializer.Serialize(config.Mappings,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true });
            File.WriteAllText(configPath, json);

            var exitCode = Program.Main(["run", configPath]);
            Assert.Equal(0, exitCode);

            Assert.True(File.Exists(destPath));

            using var destWb = new ClosedXML.Excel.XLWorkbook(destPath);
            var ws = destWb.Worksheet("Report");
            Assert.Equal("Name", ws.Cell("B3").GetString());
            Assert.Equal("Age", ws.Cell("C3").GetString());
            Assert.Equal("Alice", ws.Cell("B4").GetString());
            Assert.Equal(30.0, ws.Cell("C4").Value.GetNumber());
        }
        finally
        {
            CleanupDefaultSampleConfig();
            Directory.Delete(tempDir, recursive: true);
        }
    }

    private static void CleanupDefaultSampleConfig()
    {
        var defaultPath = Path.Combine(Directory.GetCurrentDirectory(), "sample-config.json");
        if (File.Exists(defaultPath))
        {
            File.Delete(defaultPath);
        }
    }
}
