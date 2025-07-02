using System.CommandLine;
using System.CommandLine.Parsing;
using PingdomExporter.Models;

namespace PingdomExporter.Services
{
    public class CliHandler
    {
        private readonly RootCommand _rootCommand;

        public CliHandler()
        {
            _rootCommand = CreateRootCommand();
        }

        public async Task<CliResult> ParseAsync(string[] args)
        {
            var parseResult = _rootCommand.Parse(args);
            
            // Check for help or version
            if (args.Contains("--help") || args.Contains("-h") || args.Contains("--version"))
            {
                await _rootCommand.InvokeAsync(args);
                return new CliResult { ShouldExit = true, ExitCode = 0 };
            }

            // Check for parse errors
            if (parseResult.Errors.Count > 0)
            {
                foreach (var error in parseResult.Errors)
                {
                    Console.WriteLine($"Error: {error.Message}");
                }
                Console.WriteLine();
                Console.WriteLine("Use --help to see available options.");
                return new CliResult { ShouldExit = true, ExitCode = 1 };
            }

            // Extract configuration from parsed arguments
            var config = ExtractConfiguration(parseResult);
            return new CliResult { Configuration = config, ShouldExit = false };
        }

        private RootCommand CreateRootCommand()
        {
            var rootCommand = new RootCommand("Pingdom Monitor Configuration Exporter")
            {
                Description = "Export monitor configurations from Pingdom API for backup, migration, or analysis purposes."
            };

            // API Configuration
            var apiTokenOption = new Option<string>(
                aliases: new[] { "--api-token", "-t" },
                description: "Pingdom API token (Bearer token)")
            {
                IsRequired = false
            };

            var baseUrlOption = new Option<string>(
                aliases: new[] { "--base-url", "-u" },
                description: "Pingdom API base URL")
            {
                IsRequired = false
            };

            // Output Configuration
            var outputDirOption = new Option<string>(
                aliases: new[] { "--output-dir", "-o" },
                description: "Output directory for exported files")
            {
                IsRequired = false
            };

            var outputFormatOption = new Option<string>(
                aliases: new[] { "--format", "-f" },
                description: "Output format: json, csv, or both")
            {
                IsRequired = false
            };

            var exportModeOption = new Option<string>(
                aliases: new[] { "--export-mode", "-m" },
                description: "Export mode: Full (includes details), Summary (summary only), or UptimeRobot (UptimeRobot import format)")
            {
                IsRequired = false
            };

            // Export Options
            var exportUptimeOption = new Option<bool?>(
                aliases: new[] { "--uptime" },
                description: "Export uptime checks (true/false)")
            {
                IsRequired = false
            };

            var noUptimeOption = new Option<bool>(
                aliases: new[] { "--no-uptime" },
                description: "Skip exporting uptime checks")
            {
                IsRequired = false
            };

            var exportTransactionOption = new Option<bool?>(
                aliases: new[] { "--transaction" },
                description: "Export transaction checks (true/false)")
            {
                IsRequired = false
            };

            var noTransactionOption = new Option<bool>(
                aliases: new[] { "--no-transaction" },
                description: "Skip exporting transaction checks")
            {
                IsRequired = false
            };

            var includeTagsOption = new Option<bool?>(
                aliases: new[] { "--include-tags" },
                description: "Include tag information (true/false)")
            {
                IsRequired = false
            };

            var noTagsOption = new Option<bool>(
                aliases: new[] { "--no-tags" },
                description: "Skip tag information")
            {
                IsRequired = false
            };

            var includeTeamsOption = new Option<bool?>(
                aliases: new[] { "--include-teams" },
                description: "Include team assignments (true/false)")
            {
                IsRequired = false
            };

            var noTeamsOption = new Option<bool>(
                aliases: new[] { "--no-teams" },
                description: "Skip team assignments")
            {
                IsRequired = false
            };

            var includeDisabledOption = new Option<bool>(
                aliases: new[] { "--include-disabled" },
                description: "Include disabled/paused checks")
            {
                IsRequired = false
            };

            // Performance Options
            var delayOption = new Option<int?>(
                aliases: new[] { "--delay", "-d" },
                description: "Delay between API requests in milliseconds (for rate limiting)")
            {
                IsRequired = false
            };

            // Execution Options
            var autoOption = new Option<bool>(
                aliases: new[] { "--auto", "-y" },
                description: "Run automatically without prompts")
            {
                IsRequired = false
            };

            var verboseOption = new Option<bool>(
                aliases: new[] { "--verbose", "-v" },
                description: "Enable verbose output")
            {
                IsRequired = false
            };

            // Add all options to the root command
            rootCommand.AddOption(apiTokenOption);
            rootCommand.AddOption(baseUrlOption);
            rootCommand.AddOption(outputDirOption);
            rootCommand.AddOption(outputFormatOption);
            rootCommand.AddOption(exportModeOption);
            rootCommand.AddOption(exportUptimeOption);
            rootCommand.AddOption(noUptimeOption);
            rootCommand.AddOption(exportTransactionOption);
            rootCommand.AddOption(noTransactionOption);
            rootCommand.AddOption(includeTagsOption);
            rootCommand.AddOption(noTagsOption);
            rootCommand.AddOption(includeTeamsOption);
            rootCommand.AddOption(noTeamsOption);
            rootCommand.AddOption(includeDisabledOption);
            rootCommand.AddOption(delayOption);
            rootCommand.AddOption(autoOption);
            rootCommand.AddOption(verboseOption);

            return rootCommand;
        }

        private ExportConfiguration ExtractConfiguration(ParseResult parseResult)
        {
            var config = new ExportConfiguration();

            // Extract values from command line
            if (parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<string>>().First(o => o.HasAlias("--api-token"))) is string apiToken)
                config.ApiToken = apiToken;

            if (parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<string>>().First(o => o.HasAlias("--base-url"))) is string baseUrl)
                config.BaseUrl = baseUrl;

            if (parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<string>>().First(o => o.HasAlias("--output-dir"))) is string outputDir)
                config.OutputDirectory = outputDir;

            if (parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<string>>().First(o => o.HasAlias("--format"))) is string format)
                config.OutputFormat = format;

            if (parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<string>>().First(o => o.HasAlias("--export-mode"))) is string exportMode)
                config.ExportMode = exportMode;

            // Handle boolean options with negation support
            var exportUptime = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool?>>().First(o => o.HasAlias("--uptime")));
            var noUptime = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--no-uptime")));
            if (exportUptime.HasValue) config.ExportUptimeChecks = exportUptime.Value;
            else if (noUptime) config.ExportUptimeChecks = false;

            var exportTransaction = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool?>>().First(o => o.HasAlias("--transaction")));
            var noTransaction = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--no-transaction")));
            if (exportTransaction.HasValue) config.ExportTransactionChecks = exportTransaction.Value;
            else if (noTransaction) config.ExportTransactionChecks = false;

            var includeTags = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool?>>().First(o => o.HasAlias("--include-tags")));
            var noTags = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--no-tags")));
            if (includeTags.HasValue) config.IncludeTags = includeTags.Value;
            else if (noTags) config.IncludeTags = false;

            var includeTeams = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool?>>().First(o => o.HasAlias("--include-teams")));
            var noTeams = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--no-teams")));
            if (includeTeams.HasValue) config.IncludeTeams = includeTeams.Value;
            else if (noTeams) config.IncludeTeams = false;

            var includeDisabled = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--include-disabled")));
            if (includeDisabled) config.IncludeDisabledChecks = true;

            if (parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<int?>>().First(o => o.HasAlias("--delay"))) is int delay)
                config.RequestDelayMs = delay;

            // Special handling for auto and verbose flags
            config.AutoMode = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--auto")));
            config.VerboseMode = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--verbose")));

            return config;
        }
    }

    public class CliResult
    {
        public ExportConfiguration? Configuration { get; set; }
        public bool ShouldExit { get; set; }
        public int ExitCode { get; set; }
    }
}
