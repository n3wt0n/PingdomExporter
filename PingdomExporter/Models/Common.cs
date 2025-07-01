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
        public string BaseUrl { get; set; } = "https://api.pingdom.com/api/3.1";
        public string OutputDirectory { get; set; } = "exports";
        public bool ExportUptimeChecks { get; set; } = true;
        public bool ExportTransactionChecks { get; set; } = true;
        public bool IncludeTags { get; set; } = true;
        public bool IncludeTeams { get; set; } = true;
        public string OutputFormat { get; set; } = "json"; // json, csv, both
        public int RequestDelayMs { get; set; } = 1000; // Delay between API requests to respect rate limits
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
