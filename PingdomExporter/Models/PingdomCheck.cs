using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace PingdomExporter.Models
{
    /// <summary>
    /// Custom JSON converter to handle the type field which can be either a string or an object
    /// depending on whether it's from the list API or details API
    /// </summary>
    public class FlexibleTypeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType) =>
            objectType == typeof(object) || objectType == typeof(string) || objectType == typeof(JObject);

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            
            if (token.Type == JTokenType.String)
                // Simple string type (from list API)
                return token.ToString();
            else if (token.Type == JTokenType.Object)
                // Complex object type (from details API) - keep as JObject for flexibility
                return token as JObject;
            
            // Fallback
            return string.Empty;
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value is string stringValue)
                writer.WriteValue(stringValue);
            else
                serializer.Serialize(writer, value);
        }
    }

    public class PingdomCheck
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("hostname")]
        public string Hostname { get; set; } = string.Empty;

        [JsonProperty("status")]
        public string Status { get; set; } = string.Empty;

        [JsonProperty("resolution")]
        public int Resolution { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(FlexibleTypeConverter))]
        public object Type { get; set; } = string.Empty;

        [JsonProperty("lasttesttime")]
        public long LastTestTime { get; set; }

        [JsonProperty("lastresponsetime")]
        public int LastResponseTime { get; set; }

        [JsonProperty("created")]
        public long Created { get; set; }

        [JsonProperty("tags")]
        public List<Tag> Tags { get; set; } = new();

        [JsonProperty("probe_filters")]
        public List<string> ProbeFilters { get; set; } = new();

        [JsonProperty("ipv6")]
        public bool Ipv6 { get; set; }

        [JsonProperty("responsetime_threshold")]
        public int ResponseTimeThreshold { get; set; }

        [JsonProperty("custom_message")]
        public string CustomMessage { get; set; } = string.Empty;

        [JsonProperty("integrationids")]
        public List<int> IntegrationIds { get; set; } = new();

        [JsonProperty("sendnotificationwhendown")]
        public int SendNotificationWhenDown { get; set; }

        [JsonProperty("notifyagainevery")]
        public int NotifyAgainEvery { get; set; }

        [JsonProperty("notifywhenbackup")]
        public bool NotifyWhenBackup { get; set; }

        [JsonProperty("userids")]
        public List<int> UserIds { get; set; } = new();

        [JsonProperty("teamids")]
        public List<int> TeamIds { get; set; } = new();

        [JsonProperty("paused")]
        public bool Paused { get; set; }
    }

    public class CheckType
    {
        [JsonProperty("http")]
        public object? Http { get; set; }

        [JsonProperty("tcp")]
        public object? Tcp { get; set; }

        [JsonProperty("ping")]
        public object? Ping { get; set; }

        [JsonProperty("dns")]
        public object? Dns { get; set; }
    }

    public class HttpCheckDetails
    {
        [JsonProperty("url")]
        public string Url { get; set; } = string.Empty;

        [JsonProperty("encryption")]
        public bool Encryption { get; set; }

        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("auth")]
        public string? Auth { get; set; }

        [JsonProperty("shouldcontain")]
        public string? ShouldContain { get; set; }

        [JsonProperty("shouldnotcontain")]
        public string? ShouldNotContain { get; set; }

        [JsonProperty("postdata")]
        public string? PostData { get; set; }

        [JsonProperty("requestheaders")]
        public Dictionary<string, string> RequestHeaders { get; set; } = new();

        [JsonProperty("verify_certificate")]
        public bool VerifyCertificate { get; set; }

        [JsonProperty("ssl_down_days_before")]
        public int SslDownDaysBefore { get; set; }
    }

    public class TcpCheckDetails
    {
        [JsonProperty("port")]
        public int Port { get; set; }

        [JsonProperty("stringtosend")]
        public string? StringToSend { get; set; }

        [JsonProperty("stringtoexpect")]
        public string? StringToExpect { get; set; }
    }

    public class PingCheckDetails
    {
        // Ping checks typically don't have additional parameters beyond basic check settings
    }

    public class DnsCheckDetails
    {
        [JsonProperty("expectedip")]
        public string ExpectedIp { get; set; } = string.Empty;

        [JsonProperty("nameserver")]
        public string NameServer { get; set; } = string.Empty;
    }

    public class Tag
    {
        [JsonProperty("name")]
        public string Name { get; set; } = string.Empty;

        [JsonProperty("type")]
        public string Type { get; set; } = string.Empty;

        [JsonProperty("count")]
        public int Count { get; set; }
    }

    public class PingdomChecksResponse
    {
        [JsonProperty("checks")]
        public List<PingdomCheck> Checks { get; set; } = new();

        [JsonProperty("counts")]
        public CheckCounts? Counts { get; set; }
    }

    public class CheckCounts
    {
        [JsonProperty("total")]
        public int Total { get; set; }

        [JsonProperty("limited")]
        public int Limited { get; set; }

        [JsonProperty("filtered")]
        public int Filtered { get; set; }
    }
}
