using Newtonsoft.Json;

namespace PingdomExporter.Models
{
    public class PingdomApiError
    {
        [JsonProperty("error")]
        public ErrorDetails Error { get; set; } = new();
    }

    public class ErrorDetails
    {
        [JsonProperty("statuscode")]
        public int StatusCode { get; set; }

        [JsonProperty("statusdesc")]
        public string StatusDescription { get; set; } = string.Empty;

        [JsonProperty("errormessage")]
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class ExportConfiguration
    {
        public string ApiToken { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = "https://api.pingdom.com/api/3.1/";
        public string OutputDirectory { get; set; } = "exports";
        public bool ExportUptimeChecks { get; set; } = true;
        public bool ExportTransactionChecks { get; set; } = true;
        public bool IncludeTags { get; set; } = true;
        public bool IncludeTeams { get; set; } = true;
        public string OutputFormat { get; set; } = "json"; // json, csv, both
        public string ExportMode { get; set; } = "Summary"; // Full, Summary, UptimeRobot
        public bool IncludeDisabledChecks { get; set; } = false; // Only export checks with status "up" by default
        public int RequestDelayMs { get; set; } = 1000; // Delay between API requests to respect rate limits
        
        // CLI-specific properties (not in config file)
        public bool AutoMode { get; set; } = false;
        public bool VerboseMode { get; set; } = false;
    }

    public class UptimeRobotMonitor
    {
        public string Type { get; set; } = "HTTP";
        public string FriendlyName { get; set; } = string.Empty;
        public string UrlIp { get; set; } = string.Empty;
        public int Interval { get; set; } = 300; // 5 minutes default
        public string KeywordType { get; set; } = string.Empty;
        public string KeywordValue { get; set; } = string.Empty;
        public string Port { get; set; } = string.Empty;
    }

    public class ExportSummary
    {
        public DateTime ExportDate { get; set; }
        public int UptimeChecksExported { get; set; }
        public int TransactionChecksExported { get; set; }
        public int TotalChecksExported { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }
}
