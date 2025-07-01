using Newtonsoft.Json;

namespace PingdomExporter.Models
{
    public class TmsCheck
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("active")]
        public bool Active { get; set; }

        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }

        [JsonProperty("modified_at")]
        public long ModifiedAt { get; set; }

        [JsonProperty("last_downtime_start")]
        public long? LastDowntimeStart { get; set; }

        [JsonProperty("last_downtime_end")]
        public long? LastDowntimeEnd { get; set; }

        [JsonProperty("custom_message")]
        public string CustomMessage { get; set; } = string.Empty;

        [JsonProperty("interval")]
        public int Interval { get; set; }

        [JsonProperty("region")]
        public string Region { get; set; } = string.Empty;

        [JsonProperty("send_notification_when_down")]
        public int SendNotificationWhenDown { get; set; }

        [JsonProperty("severity_level")]
        public string SeverityLevel { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("steps")]
        public List<TmsStep> Steps { get; set; } = new();

        [JsonProperty("team_ids")]
        public List<int> TeamIds { get; set; } = new();

        [JsonProperty("contact_ids")]
        public List<int> ContactIds { get; set; } = new();

        [JsonProperty("integration_ids")]
        public List<int> IntegrationIds { get; set; } = new();

        [JsonProperty("tags")]
        public List<string> Tags { get; set; } = new();

        [JsonProperty("type")]
        public List<string> Type { get; set; } = new();

        [JsonProperty("metadata")]
        public TmsMetadata? Metadata { get; set; }
    }

    public class TmsStep
    {
        [JsonProperty("fn")]
        public string Function { get; set; } = string.Empty;

        [JsonProperty("args")]
        public Dictionary<string, object> Args { get; set; } = new();

        [JsonProperty("guid")]
        public string? Guid { get; set; }

        [JsonProperty("contains_navigate")]
        public bool? ContainsNavigate { get; set; }
    }

    public class TmsMetadata
    {
        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("disableWebSecurity")]
        public bool DisableWebSecurity { get; set; }

        [JsonProperty("authentications")]
        public Dictionary<string, object>? Authentications { get; set; }
    }

    public class TmsChecksResponse
    {
        [JsonProperty("checks")]
        public List<TmsCheck> Checks { get; set; } = new();

        [JsonProperty("limit")]
        public int Limit { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }
    }
}
