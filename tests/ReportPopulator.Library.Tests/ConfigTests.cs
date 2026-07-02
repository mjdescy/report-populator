using System.Text.Json;
using ReportPopulator.Library;

namespace ReportPopulator.Library.Tests;

public sealed class ConfigTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    [Fact]
    public void LoadConfig_ParsesMappingsCorrectly()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var configPath = Path.Combine(tempDir, "config.json");
            var json = """
            [
              {
                "sourceFilePath": "source.xlsx",
                "destinationFilePath": "dest.xlsx",
                "destinationWorksheet": "Sheet1",
                "destinationCellAddress": "A4"
              },
              {
                "sourceFilePath": "source2.xlsx",
                "destinationFilePath": "dest2.xlsx",
                "destinationWorksheet": "Data",
                "destinationCellAddress": "B2"
              }
            ]
            """;
            File.WriteAllText(configPath, json);

            var service = new PopulatorService();
            var config = service.LoadConfig(configPath);

            Assert.NotNull(config);
            Assert.Equal(2, config.Mappings.Count);

            Assert.Equal("source.xlsx", config.Mappings[0].SourceFilePath);
            Assert.Equal("dest.xlsx", config.Mappings[0].DestinationFilePath);
            Assert.Equal("Sheet1", config.Mappings[0].DestinationWorksheet);
            Assert.Equal("A4", config.Mappings[0].DestinationCellAddress);

            Assert.Equal("source2.xlsx", config.Mappings[1].SourceFilePath);
            Assert.Equal("dest2.xlsx", config.Mappings[1].DestinationFilePath);
            Assert.Equal("Data", config.Mappings[1].DestinationWorksheet);
            Assert.Equal("B2", config.Mappings[1].DestinationCellAddress);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void LoadConfig_EmptyMappings_ReturnsEmptyList()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var configPath = Path.Combine(tempDir, "config.json");
            var json = """[]""";
            File.WriteAllText(configPath, json);

            var service = new PopulatorService();
            var config = service.LoadConfig(configPath);

            Assert.NotNull(config);
            Assert.Empty(config.Mappings);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void GenerateSampleConfig_CreatesValidFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var outputPath = Path.Combine(tempDir, "sample.json");
            var service = new PopulatorService();
            service.GenerateSampleConfig(outputPath);

            Assert.True(File.Exists(outputPath));

            var json = File.ReadAllText(outputPath);
            var mappings = JsonSerializer.Deserialize<List<PopulatorRecord>>(json, JsonOptions);

            Assert.NotNull(mappings);
            Assert.NotEmpty(mappings);
            Assert.Equal("path/to/source.xlsx", mappings[0].SourceFilePath);
        }
        finally
        {
            Directory.Delete(tempDir, recursive: true);
        }
    }

    [Fact]
    public void GenerateSampleConfig_CreatesIntermediateDirectories()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

        try
        {
            var outputPath = Path.Combine(tempDir, "sub", "nested", "sample.json");
            var service = new PopulatorService();
            service.GenerateSampleConfig(outputPath);

            Assert.True(File.Exists(outputPath));
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }
}
