using CommandLine;
using ReportPopulator.Library;

namespace ReportPopulator.Console;

public static class Program
{
    public static int Main(string[] args)
    {
        return Parser.Default.ParseArguments<RunOptions, InitOptions>(args)
            .MapResult(
                (RunOptions opts) => RunCommand(opts),
                (InitOptions opts) => InitCommand(opts),
                _ => 1);
    }

    private static int RunCommand(RunOptions options)
    {
        var service = new PopulatorService();

        try
        {
            service.RunFromConfigFile(options.ConfigFilePath);
            System.Console.WriteLine("Report population completed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            System.Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static int InitCommand(InitOptions options)
    {
        var service = new PopulatorService();
        var outputPath = string.IsNullOrWhiteSpace(options.OutputPath)
            ? Path.Combine(Directory.GetCurrentDirectory(), "sample-config.json")
            : options.OutputPath;

        service.GenerateSampleConfig(outputPath);
        System.Console.WriteLine($"Sample configuration file written to: {outputPath}");
        return 0;
    }
}

[Verb("run", HelpText = "Run the report population using the specified configuration file.")]
public sealed class RunOptions
{
    [Value(0, Required = true, MetaName = "config.json", HelpText = "Path to the configuration file.")]
    public string ConfigFilePath { get; set; } = string.Empty;
}

[Verb("init", HelpText = "Generate a sample configuration file.")]
public sealed class InitOptions
{
    [Value(0, Required = false, MetaName = "output-path", HelpText = "Optional output file path. Defaults to ./sample-config.json.")]
    public string? OutputPath { get; set; }
}
