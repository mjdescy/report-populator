using CommandLine;
using ReportPopulator.Library;
using SysConsole = System.Console;

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
            SysConsole.WriteLine("Report population completed successfully.");
            return 0;
        }
        catch (Exception ex)
        {
            SysConsole.Error.WriteLine($"Error: {ex.Message}");
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
        SysConsole.WriteLine($"Sample configuration file written to: {outputPath}");
        return 0;
    }
}

/// <summary>
/// CLI options for the <c>run</c> verb.
/// </summary>
[Verb("run", HelpText = "Run the report population using the specified configuration file.")]
public sealed class RunOptions
{
    /// <summary>
    /// Path to the configuration JSON file.
    /// </summary>
    [Value(0, Required = true, MetaName = "config.json", HelpText = "Path to the configuration file.")]
    public string ConfigFilePath { get; set; } = string.Empty;
}

/// <summary>
/// CLI options for the <c>init</c> verb.
/// </summary>
[Verb("init", HelpText = "Generate a sample configuration file.")]
public sealed class InitOptions
{
    /// <summary>
    /// Optional output file path. Defaults to <c>./sample-config.json</c>.
    /// </summary>
    [Value(0, Required = false, MetaName = "output-path", HelpText = "Optional output file path. Defaults to ./sample-config.json.")]
    public string? OutputPath { get; set; }
}
