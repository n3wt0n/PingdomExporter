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

            // Export Options (negative flags for features enabled by default)
            var noUptimeOption = new Option<bool>(
                aliases: new[] { "--no-uptime" },
                description: "Skip exporting uptime checks")
            {
                IsRequired = false
            };

            var noTransactionOption = new Option<bool>(
                aliases: new[] { "--no-transaction" },
                description: "Skip exporting transaction checks")
            {
                IsRequired = false
            };

            var noTagsOption = new Option<bool>(
                aliases: new[] { "--no-tags" },
                description: "Skip tag information")
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
            rootCommand.AddOption(noUptimeOption);
            rootCommand.AddOption(noTransactionOption);
            rootCommand.AddOption(noTagsOption);
            rootCommand.AddOption(noTeamsOption);
            rootCommand.AddOption(includeDisabledOption);
            rootCommand.AddOption(delayOption);
            rootCommand.AddOption(autoOption);
            rootCommand.AddOption(verboseOption);

            return rootCommand;
        }

        private ExportConfiguration ExtractConfiguration(ParseResult parseResult)
        {
            // Create a configuration object with empty values - only set what was explicitly provided via CLI
            var config = new ExportConfiguration
            {
                ApiToken = string.Empty,
                BaseUrl = string.Empty,
                OutputDirectory = string.Empty,
                OutputFormat = string.Empty,
                ExportMode = string.Empty,
                // Keep boolean defaults as they are since they have special handling
                RequestDelayMs = 1000 // This will be overridden only if --delay is provided
            };

            // Extract values from command line - only set if explicitly provided
            var apiTokenOption = _rootCommand.Options.OfType<Option<string>>().First(o => o.HasAlias("--api-token"));
            if (parseResult.FindResultFor(apiTokenOption) != null)
                config.ApiToken = parseResult.GetValueForOption(apiTokenOption) ?? string.Empty;

            var baseUrlOption = _rootCommand.Options.OfType<Option<string>>().First(o => o.HasAlias("--base-url"));
            if (parseResult.FindResultFor(baseUrlOption) != null)
                config.BaseUrl = parseResult.GetValueForOption(baseUrlOption) ?? string.Empty;

            var outputDirOption = _rootCommand.Options.OfType<Option<string>>().First(o => o.HasAlias("--output-dir"));
            if (parseResult.FindResultFor(outputDirOption) != null)
                config.OutputDirectory = parseResult.GetValueForOption(outputDirOption) ?? string.Empty;

            var formatOption = _rootCommand.Options.OfType<Option<string>>().First(o => o.HasAlias("--format"));
            if (parseResult.FindResultFor(formatOption) != null)
                config.OutputFormat = parseResult.GetValueForOption(formatOption) ?? string.Empty;

            var exportModeOption = _rootCommand.Options.OfType<Option<string>>().First(o => o.HasAlias("--export-mode"));
            if (parseResult.FindResultFor(exportModeOption) != null)
                config.ExportMode = parseResult.GetValueForOption(exportModeOption) ?? string.Empty;

            // Handle boolean options (negative flags only for features enabled by default)
            var noUptime = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--no-uptime")));
            if (noUptime) config.ExportUptimeChecks = false;

            var noTransaction = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--no-transaction")));
            if (noTransaction) config.ExportTransactionChecks = false;

            var noTags = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--no-tags")));
            if (noTags) config.IncludeTags = false;

            var noTeams = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--no-teams")));
            if (noTeams) config.IncludeTeams = false;

            var includeDisabled = parseResult.GetValueForOption(_rootCommand.Options.OfType<Option<bool>>().First(o => o.HasAlias("--include-disabled")));
            if (includeDisabled) config.IncludeDisabledChecks = true;

            var delayOption = _rootCommand.Options.OfType<Option<int?>>().First(o => o.HasAlias("--delay"));
            if (parseResult.FindResultFor(delayOption) != null && parseResult.GetValueForOption(delayOption) is int delay)
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
