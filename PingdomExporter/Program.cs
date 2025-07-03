using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using PingdomExporter.Models;
using PingdomExporter.Services;
using System.Net.Http.Headers;

namespace PingdomExporter
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                // Parse command line arguments first
                var cliHandler = new CliHandler();
                var cliResult = await cliHandler.ParseAsync(args);
                
                if (cliResult.ShouldExit)
                    return cliResult.ExitCode;

                // Build configuration from multiple sources
                var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables("PINGDOM_")
                    .AddCommandLine(args)
                    .Build();

                // Load base configuration from files/environment
                var exportConfig = configuration.Get<ExportConfiguration>() ?? new ExportConfiguration();
                
                // Override with CLI values (CLI takes highest precedence)
                if (cliResult.Configuration != null)
                    OverrideConfigurationFromCli(exportConfig, cliResult.Configuration);

                // Validate configuration
                if (string.IsNullOrWhiteSpace(exportConfig.ApiToken))
                {
                    Console.WriteLine("Error: API Token is required. Please set it in appsettings.json, via environment variable PINGDOM_ApiToken, or using --api-token parameter");
                    Console.WriteLine("Use --help to see all available options.");
                    return 1;
                }

                if (exportConfig.ApiToken == "YOUR_PINGDOM_API_TOKEN_HERE")
                {
                    Console.WriteLine("Error: Please replace the placeholder API token with your actual Pingdom API token");
                    Console.WriteLine("You can set it in appsettings.json, via PINGDOM_ApiToken environment variable, or using --api-token parameter");
                    return 1;
                }

                // Setup dependency injection
                var services = new ServiceCollection();
                ConfigureServices(services, exportConfig);
                var serviceProvider = services.BuildServiceProvider();

                // Display configuration
                DisplayConfiguration(exportConfig);

                // Confirm before proceeding (unless auto mode)
                if (!exportConfig.AutoMode && !args.Contains("--auto") && !args.Contains("-y"))
                {
                    Console.Write("Do you want to proceed with the export? (y/N): ");
                    var response = Console.ReadLine()?.ToLower();
                    if (response != "y" && response != "yes")
                    {
                        Console.WriteLine("Export cancelled.");
                        return 0;
                    }
                }

                // Execute export
                var exportService = serviceProvider.GetRequiredService<IExportService>();
                var summary = await exportService.ExportMonitorConfigurationsAsync();

                // Display results
                DisplayResults(summary);

                if (summary.Errors.Any())
                    return 1;

                if (exportConfig.VerboseMode)
                    Console.WriteLine("\nExport completed successfully! 🎉");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled error: {ex.Message}");
                if (args.Contains("--verbose") || args.Contains("-v"))
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return 1;
            }
        }

        private static void ConfigureServices(IServiceCollection services, ExportConfiguration config)
        {
            // Register configuration
            services.AddSingleton(config);

            // Register HTTP client with automatic decompression and proper authentication
            services.AddHttpClient<IPingdomApiService, PingdomApiService>(client =>
            {
                client.BaseAddress = new Uri(config.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
                
                // Configure authentication
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", config.ApiToken);
                
                // Request compression and ensure JSON responses
                client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate");
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));
            })
            .ConfigurePrimaryHttpMessageHandler(() =>
            {
                // Ensure automatic decompression is enabled
                return new HttpClientHandler()
                {
                    AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
                };
            });

            // Register services
            services.AddTransient<IExportService, ExportService>();
        }

        private static void OverrideConfigurationFromCli(ExportConfiguration baseConfig, ExportConfiguration cliConfig)
        {
            if (!string.IsNullOrEmpty(cliConfig.ApiToken))
                baseConfig.ApiToken = cliConfig.ApiToken;
            
            if (!string.IsNullOrEmpty(cliConfig.BaseUrl))
                baseConfig.BaseUrl = cliConfig.BaseUrl;
            
            if (!string.IsNullOrEmpty(cliConfig.OutputDirectory))
                baseConfig.OutputDirectory = cliConfig.OutputDirectory;
            
            if (!string.IsNullOrEmpty(cliConfig.OutputFormat))
                baseConfig.OutputFormat = cliConfig.OutputFormat;
            
            if (!string.IsNullOrEmpty(cliConfig.ExportMode))
                baseConfig.ExportMode = cliConfig.ExportMode;
            
            // Only override boolean values if they differ from defaults
            // This allows CLI to override file/env config
            if (cliConfig.ExportUptimeChecks != new ExportConfiguration().ExportUptimeChecks || 
                HasExplicitBooleanFlag(cliConfig, nameof(ExportConfiguration.ExportUptimeChecks)))
                baseConfig.ExportUptimeChecks = cliConfig.ExportUptimeChecks;
            
            if (cliConfig.ExportTransactionChecks != new ExportConfiguration().ExportTransactionChecks || 
                HasExplicitBooleanFlag(cliConfig, nameof(ExportConfiguration.ExportTransactionChecks)))
                baseConfig.ExportTransactionChecks = cliConfig.ExportTransactionChecks;
            
            if (cliConfig.IncludeTags != new ExportConfiguration().IncludeTags || 
                HasExplicitBooleanFlag(cliConfig, nameof(ExportConfiguration.IncludeTags)))
                baseConfig.IncludeTags = cliConfig.IncludeTags;
            
            if (cliConfig.IncludeTeams != new ExportConfiguration().IncludeTeams || 
                HasExplicitBooleanFlag(cliConfig, nameof(ExportConfiguration.IncludeTeams)))
                baseConfig.IncludeTeams = cliConfig.IncludeTeams;
            
            if (cliConfig.IncludeDisabledChecks != new ExportConfiguration().IncludeDisabledChecks || 
                HasExplicitBooleanFlag(cliConfig, nameof(ExportConfiguration.IncludeDisabledChecks)))
                baseConfig.IncludeDisabledChecks = cliConfig.IncludeDisabledChecks;
            
            if (cliConfig.RequestDelayMs != new ExportConfiguration().RequestDelayMs)
                baseConfig.RequestDelayMs = cliConfig.RequestDelayMs;
            
            // CLI-specific flags
            baseConfig.AutoMode = cliConfig.AutoMode;
            baseConfig.VerboseMode = cliConfig.VerboseMode;
        }

        private static bool HasExplicitBooleanFlag(ExportConfiguration config, string propertyName)
        {
            // This is a simplified check - in a real implementation you might want to track
            // which properties were explicitly set via CLI vs using defaults
            return true;
        }

        private static void DisplayConfiguration(ExportConfiguration config)
        {
            Console.WriteLine("Pingdom Monitor Configuration Exporter");
            Console.WriteLine("=====================================");
            Console.WriteLine($"Base URL: {config.BaseUrl}");
            Console.WriteLine($"Output Directory: {config.OutputDirectory}");
            Console.WriteLine($"Export Uptime Checks: {config.ExportUptimeChecks}");
            Console.WriteLine($"Export Transaction Checks: {config.ExportTransactionChecks}");
            Console.WriteLine($"Include Tags: {config.IncludeTags}");
            Console.WriteLine($"Include Teams: {config.IncludeTeams}");
            Console.WriteLine($"Output Format: {config.OutputFormat}");
            Console.WriteLine($"Export Mode: {config.ExportMode}");
            Console.WriteLine($"Include Disabled Checks: {config.IncludeDisabledChecks}");
            Console.WriteLine($"Request Delay: {config.RequestDelayMs}ms");
            
            if (config.VerboseMode)
            {
                Console.WriteLine($"Auto Mode: {config.AutoMode}");
                Console.WriteLine($"Verbose Mode: {config.VerboseMode}");
            }
            Console.WriteLine();
        }

        private static void DisplayResults(ExportSummary summary)
        {
            Console.WriteLine("\n" + new string('=', 50));
            Console.WriteLine("EXPORT SUMMARY");
            Console.WriteLine(new string('=', 50));
            Console.WriteLine($"Export Date: {summary.ExportDate:yyyy-MM-dd HH:mm:ss} UTC");
            Console.WriteLine($"Duration: {summary.Duration.TotalSeconds:F2} seconds");
            Console.WriteLine($"Uptime Checks Exported: {summary.UptimeChecksExported}");
            Console.WriteLine($"Transaction Checks Exported: {summary.TransactionChecksExported}");
            Console.WriteLine($"Total Checks Exported: {summary.TotalChecksExported}");
            
            if (summary.Warnings.Any())
            {
                Console.WriteLine($"\nWarnings ({summary.Warnings.Count}):");
                foreach (var warning in summary.Warnings)
                {
                    Console.WriteLine($"  - {warning}");
                }
            }

            if (summary.Errors.Any())
            {
                Console.WriteLine($"\nErrors ({summary.Errors.Count}):");
                foreach (var error in summary.Errors)
                {
                    Console.WriteLine($"  - {error}");
                }
            }
        }
    }
}
