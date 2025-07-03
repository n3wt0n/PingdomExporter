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
                    // Filter checks based on IncludeDisabledChecks setting
                    var filteredChecks = FilterChecksByStatus(checksResponse.Checks);
                    var filteredResponse = new PingdomChecksResponse
                    {
                        Checks = filteredChecks,
                        Counts = checksResponse.Counts
                    };

                    // Always export summary list
                    await SaveDataAsync("uptime-checks-summary", filteredResponse, summary);
                    exportedCount = filteredChecks.Count;

                    if (_config.VerboseMode && checksResponse.Checks.Count != filteredChecks.Count)
                    {
                        Console.WriteLine($"Filtered out {checksResponse.Checks.Count - filteredChecks.Count} disabled/paused checks");
                    }

                    // Handle different export modes
                    if (_config.ExportMode.Equals("Full", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Fetching detailed information for uptime checks...");
                        var detailedChecks = new List<PingdomCheck>();

                        foreach (var check in filteredChecks)
                        {
                            try
                            {
                                await ((PingdomApiService)_apiService).DelayForRateLimitAsync();
                                var detailedCheck = await _apiService.GetUptimeCheckDetailsAsync(check.Id);
                                detailedChecks.Add(detailedCheck);

                                if (detailedChecks.Count % 10 == 0)
                                    Console.WriteLine($"Processed {detailedChecks.Count}/{filteredChecks.Count} uptime checks...");
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
                    else if (_config.ExportMode.Equals("UptimeRobot", StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine("Converting checks to UptimeRobot import format...");
                        Console.WriteLine("Fetching detailed information for UptimeRobot conversion...");
                        
                        var detailedChecks = new List<PingdomCheck>();
                        foreach (var check in filteredChecks)
                        {
                            try
                            {
                                await ((PingdomApiService)_apiService).DelayForRateLimitAsync();
                                var detailedCheck = await _apiService.GetUptimeCheckDetailsAsync(check.Id);
                                detailedChecks.Add(detailedCheck);

                                if (detailedChecks.Count % 10 == 0)
                                    Console.WriteLine($"Processed {detailedChecks.Count}/{filteredChecks.Count} checks for UptimeRobot conversion...");
                            }
                            catch (Exception ex)
                            {
                                var warning = $"Failed to fetch details for uptime check {check.Id}: {ex.Message}";
                                summary.Warnings.Add(warning);
                                Console.WriteLine($"Warning: {warning}");
                                // Use summary data as fallback
                                detailedChecks.Add(check);
                            }
                        }
                        
                        var uptimeRobotMonitors = ConvertToUptimeRobotFormat(detailedChecks);
                        await SaveUptimeRobotImportFileAsync(uptimeRobotMonitors, summary);
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

        private List<UptimeRobotMonitor> ConvertToUptimeRobotFormat(List<PingdomCheck> checks)
        {
            var monitors = new List<UptimeRobotMonitor>();

            foreach (var check in checks)
            {
                try
                {
                    var monitor = new UptimeRobotMonitor
                    {
                        FriendlyName = check.Name,
                        Interval = Math.Max(60, check.Resolution * 60) // Convert minutes to seconds, min 60s
                    };

                    // For UptimeRobot conversion, we'll use the Type as string for basic mapping
                    var typeString = check.Type?.ToString() ?? "http";
                    
                    // Map Pingdom check types to UptimeRobot types
                    if (typeString.Contains("http", StringComparison.OrdinalIgnoreCase))
                    {
                        monitor.Type = "HTTP";
                        monitor.UrlIp = GetHttpUrlFromCheck(check);
                    }
                    else if (typeString.Contains("ping", StringComparison.OrdinalIgnoreCase))
                    {
                        monitor.Type = "Ping";
                        monitor.UrlIp = check.Hostname ?? string.Empty;
                    }
                    else if (typeString.Contains("tcp", StringComparison.OrdinalIgnoreCase))
                    {
                        monitor.Type = "Port";
                        monitor.UrlIp = check.Hostname ?? string.Empty;
                        monitor.Port = GetTcpPortFromCheck(check);
                    }
                    else
                    {
                        // Default to HTTP for unknown types
                        monitor.Type = "HTTP";
                        monitor.UrlIp = GetHttpUrlFromCheck(check);
                    }

                    monitors.Add(monitor);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to convert check {check.Id} to UptimeRobot format: {ex.Message}");
                }
            }

            return monitors;
        }

        private string GetHttpUrlFromCheck(PingdomCheck check)
        {
            // Try to extract full URL from detailed check data
            if (check.Type is Newtonsoft.Json.Linq.JObject typeObject)
            {
                // This is detailed check data - extract URL from type object
                var httpDetails = typeObject["http"];
                if (httpDetails != null)
                {
                    var url = httpDetails["url"]?.ToString();
                    if (!string.IsNullOrEmpty(url))
                    {
                        // Construct full URL by combining hostname and URL path
                        var hostname = check.Hostname ?? string.Empty;
                        
                        // If URL is already complete, return it
                        if (url.StartsWith("http://") || url.StartsWith("https://"))
                        {
                            return url;
                        }
                        
                        // If URL is a path, combine with hostname
                        if (url.StartsWith("/"))
                        {
                            // Determine protocol - check if encryption is enabled
                            var encryption = httpDetails["encryption"]?.ToObject<bool>() ?? true;
                            var protocol = encryption ? "https" : "http";
                            
                            // Handle port if specified
                            var port = httpDetails["port"]?.ToObject<int>();
                            var portString = "";
                            if (port.HasValue && port.Value != (encryption ? 443 : 80))
                            {
                                portString = $":{port.Value}";
                            }
                            
                            return $"{protocol}://{hostname}{portString}{url}";
                        }
                        
                        // If URL doesn't start with /, assume it's a relative path
                        var defaultProtocol = httpDetails["encryption"]?.ToObject<bool>() == false ? "http" : "https";
                        return $"{defaultProtocol}://{hostname}/{url.TrimStart('/')}";
                    }
                }
            }
            
            // Fallback to hostname-based URL construction
            var fallbackHostname = check.Hostname ?? string.Empty;
            
            if (fallbackHostname.StartsWith("http://") || fallbackHostname.StartsWith("https://"))
            {
                return fallbackHostname;
            }
            
            // Default to HTTPS for security
            return $"https://{fallbackHostname}";
        }

        private string GetTcpPortFromCheck(PingdomCheck check)
        {
            // Try to extract port from detailed check data
            if (check.Type is Newtonsoft.Json.Linq.JObject typeObject)
            {
                // This is detailed check data - extract port from type object
                var tcpDetails = typeObject["tcp"];
                if (tcpDetails != null)
                {
                    var port = tcpDetails["port"]?.ToObject<int>();
                    if (port.HasValue)
                    {
                        return port.Value.ToString();
                    }
                }
            }
            
            // Fallback - return empty string if port cannot be determined
            return string.Empty;
        }

        private async Task SaveUptimeRobotImportFileAsync(List<UptimeRobotMonitor> monitors, ExportSummary summary)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                var fileName = $"uptimerobot-import_{timestamp}.csv";
                var filePath = Path.Combine(_config.OutputDirectory, fileName);

                var csv = new StringBuilder();
                
                // Add header row exactly as specified by UptimeRobot
                csv.AppendLine("Type,\"Friendly Name\",URL/IP,Interval,\"Keyword Type\",\"Keyword Value\",Port");

                // Add data rows
                foreach (var monitor in monitors)
                {
                    var row = new[]
                    {
                        monitor.Type,
                        $"\"{monitor.FriendlyName.Replace("\"", "\"\"")}\"",
                        monitor.UrlIp,
                        monitor.Interval.ToString(),
                        monitor.KeywordType,
                        monitor.KeywordValue,
                        monitor.Port
                    };
                    
                    csv.AppendLine(string.Join(",", row));
                }

                await File.WriteAllTextAsync(filePath, csv.ToString());
                
                Console.WriteLine($"UptimeRobot import file saved: {fileName}");
                if (_config.VerboseMode)
                {
                    Console.WriteLine($"  File path: {filePath}");
                    Console.WriteLine($"  Monitors exported: {monitors.Count}");
                }
            }
            catch (Exception ex)
            {
                var error = $"Failed to save UptimeRobot import file: {ex.Message}";
                summary.Errors.Add(error);
                Console.WriteLine($"Error: {error}");
            }
        }

        private List<PingdomCheck> FilterChecksByStatus(List<PingdomCheck> checks)
        {
            if (_config.IncludeDisabledChecks)
            {
                // Include all checks
                return checks;
            }
            
            // Only include checks with status "up" (active checks)
            var filteredChecks = checks.Where(check => 
                check.Status.Equals("up", StringComparison.OrdinalIgnoreCase)).ToList();
                
            if (_config.VerboseMode)
            {
                var excludedCount = checks.Count - filteredChecks.Count;
                if (excludedCount > 0)
                {
                    Console.WriteLine($"Excluding {excludedCount} checks that are not 'up' (disabled/paused/down checks)");
                }
            }
            
            return filteredChecks;
        }
    }
}
