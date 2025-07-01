using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using PingdomExporter.Models;
using PingdomExporter.Services;

namespace PingdomExporter
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                // Build configuration
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddEnvironmentVariables("PINGDOM_")
                    .AddCommandLine(args)
                    .Build();

                // Load export configuration
                var exportConfig = new ExportConfiguration();
                configuration.Bind(exportConfig);

                // Validate configuration
                if (string.IsNullOrWhiteSpace(exportConfig.ApiToken))
                {
                    Console.WriteLine("Error: API Token is required. Please set it in appsettings.json or via environment variable PINGDOM_ApiToken");
                    return 1;
                }

                if (exportConfig.ApiToken == "YOUR_PINGDOM_API_TOKEN_HERE")
                {
                    Console.WriteLine("Error: Please replace the placeholder API token in appsettings.json with your actual Pingdom API token");
                    return 1;
                }

                // Setup dependency injection
                var services = new ServiceCollection();
                ConfigureServices(services, exportConfig);
                var serviceProvider = services.BuildServiceProvider();

                // Display configuration
                Console.WriteLine("Pingdom Monitor Configuration Exporter");
                Console.WriteLine("=====================================");
                Console.WriteLine($"Base URL: {exportConfig.BaseUrl}");
                Console.WriteLine($"Output Directory: {exportConfig.OutputDirectory}");
                Console.WriteLine($"Export Uptime Checks: {exportConfig.ExportUptimeChecks}");
                Console.WriteLine($"Export Transaction Checks: {exportConfig.ExportTransactionChecks}");
                Console.WriteLine($"Output Format: {exportConfig.OutputFormat}");
                Console.WriteLine($"Request Delay: {exportConfig.RequestDelayMs}ms");
                Console.WriteLine();

                // Confirm before proceeding
                if (!args.Contains("--auto") && !args.Contains("-y"))
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
                    return 1;
                }

                Console.WriteLine("\nExport completed successfully! 🎉");
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unhandled error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return 1;
            }
        }

        private static void ConfigureServices(IServiceCollection services, ExportConfiguration config)
        {
            // Register configuration
            services.AddSingleton(config);

            // Register HTTP client
            services.AddHttpClient<IPingdomApiService, PingdomApiService>(client =>
            {
                client.BaseAddress = new Uri(config.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // Register services
            services.AddTransient<IExportService, ExportService>();
        }
    }
}
