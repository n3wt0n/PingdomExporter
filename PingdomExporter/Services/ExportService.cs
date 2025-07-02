using System.Text;
using Newtonsoft.Json;
using PingdomExporter.Models;

namespace PingdomExporter.Services
{
    public interface IExportService
    {
        Task<ExportSummary> ExportMonitorConfigurationsAsync();
    }

    public class ExportService : IExportService
    {
        private readonly IPingdomApiService _apiService;
        private readonly ExportConfiguration _config;

        public ExportService(IPingdomApiService apiService, ExportConfiguration config)
        {
            _apiService = apiService ?? throw new ArgumentNullException(nameof(apiService));
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<ExportSummary> ExportMonitorConfigurationsAsync()
        {
            var summary = new ExportSummary
            {
                ExportDate = DateTime.UtcNow
            };

            var startTime = DateTime.UtcNow;

            try
            {
                // Ensure output directory exists
                EnsureOutputDirectoryExists();

                // Export uptime checks
                if (_config.ExportUptimeChecks)
                    summary.UptimeChecksExported = await ExportUptimeChecksAsync(summary);

                // Export transaction checks
                if (_config.ExportTransactionChecks)
                    summary.TransactionChecksExported = await ExportTransactionChecksAsync(summary);

                summary.TotalChecksExported = summary.UptimeChecksExported + summary.TransactionChecksExported;
                summary.Duration = DateTime.UtcNow - startTime;

                // Export summary
                await ExportSummaryAsync(summary);

                Console.WriteLine($"\nExport completed successfully!");
                Console.WriteLine($"Total checks exported: {summary.TotalChecksExported}");
                Console.WriteLine($"Duration: {summary.Duration.TotalSeconds:F2} seconds");
                
                if (summary.Warnings.Any())
                    Console.WriteLine($"Warnings: {summary.Warnings.Count}");

                return summary;
            }
            catch (Exception ex)
            {
                summary.Errors.Add($"Export failed: {ex.Message}");
                summary.Duration = DateTime.UtcNow - startTime;
                
                Console.WriteLine($"Export failed: {ex.Message}");
                
                // Still try to save summary even if export failed
                try
                {
                    await ExportSummaryAsync(summary);
                }
                catch
                {
                    // Ignore summary save errors
                }

                throw;
            }
        }

        private async Task<int> ExportUptimeChecksAsync(ExportSummary summary)
        {
            try
            {
                var checksResponse = await _apiService.GetUptimeChecksAsync();
                var exportedCount = 0;

                if (checksResponse.Checks.Any())
                {
                    // Always export summary list
                    await SaveDataAsync("uptime-checks-summary", checksResponse, summary);
                    exportedCount = checksResponse.Checks.Count;

                    // Export detailed information only if ExportMode is "Full"
                    if (_config.ExportMode.Equals("Full", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Fetching detailed information for uptime checks...");
                        var detailedChecks = new List<PingdomCheck>();

                        foreach (var check in checksResponse.Checks)
                        {
                            try
                            {
                                await ((PingdomApiService)_apiService).DelayForRateLimitAsync();
                                var detailedCheck = await _apiService.GetUptimeCheckDetailsAsync(check.Id);
                                detailedChecks.Add(detailedCheck);

                                if (detailedChecks.Count % 10 == 0)
                                    Console.WriteLine($"Processed {detailedChecks.Count}/{checksResponse.Checks.Count} uptime checks...");
                            }
                            catch (Exception ex)
                            {
                                var warning = $"Failed to fetch details for uptime check {check.Id}: {ex.Message}";
                                summary.Warnings.Add(warning);
                                Console.WriteLine($"Warning: {warning}");
                            }
                        }

                        // Export detailed checks
                        await SaveDataAsync("uptime-checks-detailed", detailedChecks, summary);
                    }
                    else
                    {
                        Console.WriteLine($"Export mode is '{_config.ExportMode}' - skipping detailed uptime check information");
                    }
                }

                return exportedCount;
            }
            catch (Exception ex)
            {
                summary.Errors.Add($"Failed to export uptime checks: {ex.Message}");
                throw;
            }
        }

        private async Task<int> ExportTransactionChecksAsync(ExportSummary summary)
        {
            try
            {
                var checksResponse = await _apiService.GetTransactionChecksAsync();
                var exportedCount = 0;

                if (checksResponse.Checks.Any())
                {
                    // Always export summary list
                    await SaveDataAsync("transaction-checks-summary", checksResponse, summary);
                    exportedCount = checksResponse.Checks.Count;

                    // Export detailed information only if ExportMode is "Full"
                    if (_config.ExportMode.Equals("Full", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Fetching detailed information for transaction checks...");
                        var detailedChecks = new List<TmsCheck>();

                        foreach (var check in checksResponse.Checks)
                        {
                            try
                            {
                                await ((PingdomApiService)_apiService).DelayForRateLimitAsync();
                                var detailedCheck = await _apiService.GetTransactionCheckDetailsAsync(check.Id);
                                detailedChecks.Add(detailedCheck);

                                if (detailedChecks.Count % 5 == 0)
                                    Console.WriteLine($"Processed {detailedChecks.Count}/{checksResponse.Checks.Count} transaction checks...");
                            }
                            catch (Exception ex)
                            {
                                var warning = $"Failed to fetch details for transaction check {check.Id}: {ex.Message}";
                                summary.Warnings.Add(warning);
                                Console.WriteLine($"Warning: {warning}");
                            }
                        }

                        // Export detailed checks
                        await SaveDataAsync("transaction-checks-detailed", detailedChecks, summary);
                    }
                    else
                    {
                        Console.WriteLine($"Export mode is '{_config.ExportMode}' - skipping detailed transaction check information");
                    }
                }

                return exportedCount;
            }
            catch (Exception ex)
            {
                summary.Errors.Add($"Failed to export transaction checks: {ex.Message}");
                throw;
            }
        }

        private async Task SaveDataAsync<T>(string fileName, T data, ExportSummary summary)
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                
                // Save as JSON
                if (_config.OutputFormat == "json" || _config.OutputFormat == "both")
                {
                    var jsonPath = Path.Combine(_config.OutputDirectory, $"{fileName}_{timestamp}.json");
                    var jsonContent = JsonConvert.SerializeObject(data, Formatting.Indented);
                    await File.WriteAllTextAsync(jsonPath, jsonContent, Encoding.UTF8);
                    Console.WriteLine($"Exported: {jsonPath}");
                }

                // Save as CSV (basic implementation for flat data)
                if (_config.OutputFormat == "csv" || _config.OutputFormat == "both")
                {
                    var csvPath = Path.Combine(_config.OutputDirectory, $"{fileName}_{timestamp}.csv");
                    var csvContent = ConvertToCsv(data);
                    await File.WriteAllTextAsync(csvPath, csvContent, Encoding.UTF8);
                    Console.WriteLine($"Exported: {csvPath}");
                }
            }
            catch (Exception ex)
            {
                var error = $"Failed to save {fileName}: {ex.Message}";
                summary.Errors.Add(error);
                Console.WriteLine($"Error: {error}");
            }
        }

        private async Task ExportSummaryAsync(ExportSummary summary)
        {
            try
            {
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
                var summaryPath = Path.Combine(_config.OutputDirectory, $"export-summary_{timestamp}.json");
                var summaryJson = JsonConvert.SerializeObject(summary, Formatting.Indented);
                await File.WriteAllTextAsync(summaryPath, summaryJson, Encoding.UTF8);
                Console.WriteLine($"Export summary saved: {summaryPath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to save export summary: {ex.Message}");
            }
        }

        private void EnsureOutputDirectoryExists()
        {
            if (!Directory.Exists(_config.OutputDirectory))
            {
                Directory.CreateDirectory(_config.OutputDirectory);
                Console.WriteLine($"Created output directory: {_config.OutputDirectory}");
            }
        }

        private string ConvertToCsv<T>(T data)
        {
            // Basic CSV conversion - this is a simplified implementation
            // For complex nested objects, you might want to use a dedicated CSV library
            
            var json = JsonConvert.SerializeObject(data);
            var jsonObject = JsonConvert.DeserializeObject(json);

            var csv = new StringBuilder();

            if (jsonObject is Newtonsoft.Json.Linq.JArray jsonArray && jsonArray.Any())
            {
                // Get headers from first object
                var firstItem = jsonArray.First() as Newtonsoft.Json.Linq.JObject;
                if (firstItem != null)
                {
                    var headers = firstItem.Properties().Select(p => p.Name);
                    csv.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));

                    // Add data rows
                    foreach (var item in jsonArray.OfType<Newtonsoft.Json.Linq.JObject>())
                    {
                        var values = headers.Select(h => 
                        {
                            var value = item[h]?.ToString() ?? "";
                            return $"\"{value.Replace("\"", "\"\"")}\""; // Escape quotes
                        });
                        csv.AppendLine(string.Join(",", values));
                    }
                }
            }
            else if (jsonObject is Newtonsoft.Json.Linq.JObject singleObject)
            {
                // Single object
                var headers = singleObject.Properties().Select(p => p.Name);
                csv.AppendLine(string.Join(",", headers.Select(h => $"\"{h}\"")));
                
                var values = headers.Select(h => 
                {
                    var value = singleObject[h]?.ToString() ?? "";
                    return $"\"{value.Replace("\"", "\"\"")}\"";
                });
                csv.AppendLine(string.Join(",", values));
            }

            return csv.ToString();
        }
    }
}
