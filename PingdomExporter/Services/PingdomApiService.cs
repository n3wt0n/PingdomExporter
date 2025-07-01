using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using PingdomExporter.Models;

namespace PingdomExporter.Services
{
    public interface IPingdomApiService
    {
        Task<PingdomChecksResponse> GetUptimeChecksAsync();
        Task<TmsChecksResponse> GetTransactionChecksAsync();
        Task<PingdomCheck> GetUptimeCheckDetailsAsync(int checkId);
        Task<TmsCheck> GetTransactionCheckDetailsAsync(int checkId);
    }

    public class PingdomApiService : IPingdomApiService
    {
        private readonly HttpClient _httpClient;
        private readonly ExportConfiguration _config;

        public PingdomApiService(HttpClient httpClient, ExportConfiguration config)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            // Configure HTTP client
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization = 
                new AuthenticationHeaderValue("Bearer", _config.ApiToken);
            _httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip");
            _httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            // Set a reasonable timeout
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
        }

        public async Task<PingdomChecksResponse> GetUptimeChecksAsync()
        {
            try
            {
                Console.WriteLine("Fetching uptime checks...");
                
                var queryParams = new List<string>
                {
                    "limit=25000" // Maximum allowed
                };

                if (_config.IncludeTags)
                {
                    queryParams.Add("include_tags=true");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"/checks?{queryString}");

                await EnsureSuccessStatusCodeAsync(response);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<PingdomChecksResponse>(content);
                
                Console.WriteLine($"Successfully fetched {result?.Checks.Count ?? 0} uptime checks");
                return result ?? new PingdomChecksResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching uptime checks: {ex.Message}");
                throw;
            }
        }

        public async Task<TmsChecksResponse> GetTransactionChecksAsync()
        {
            try
            {
                Console.WriteLine("Fetching transaction checks...");

                var queryParams = new List<string>
                {
                    "limit=1000"
                };

                if (_config.IncludeTags)
                {
                    queryParams.Add("extended_tags=true");
                }

                var queryString = string.Join("&", queryParams);
                var response = await _httpClient.GetAsync($"/tms/check?{queryString}");

                await EnsureSuccessStatusCodeAsync(response);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TmsChecksResponse>(content);
                
                Console.WriteLine($"Successfully fetched {result?.Checks.Count ?? 0} transaction checks");
                return result ?? new TmsChecksResponse();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching transaction checks: {ex.Message}");
                throw;
            }
        }

        public async Task<PingdomCheck> GetUptimeCheckDetailsAsync(int checkId)
        {
            try
            {
                Console.WriteLine($"Fetching details for uptime check {checkId}...");

                var queryParams = new List<string>();
                
                if (_config.IncludeTeams)
                {
                    queryParams.Add("include_teams=true");
                }

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"/checks/{checkId}{queryString}");

                await EnsureSuccessStatusCodeAsync(response);

                var content = await response.Content.ReadAsStringAsync();
                var checkResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
                
                if (checkResponse?.ContainsKey("check") == true)
                {
                    var checkJson = JsonConvert.SerializeObject(checkResponse["check"]);
                    var result = JsonConvert.DeserializeObject<PingdomCheck>(checkJson);
                    return result ?? new PingdomCheck();
                }

                return new PingdomCheck();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching uptime check {checkId} details: {ex.Message}");
                throw;
            }
        }

        public async Task<TmsCheck> GetTransactionCheckDetailsAsync(int checkId)
        {
            try
            {
                Console.WriteLine($"Fetching details for transaction check {checkId}...");

                var queryParams = new List<string>();
                
                if (_config.IncludeTags)
                {
                    queryParams.Add("extended_tags=true");
                }

                var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
                var response = await _httpClient.GetAsync($"/tms/check/{checkId}{queryString}");

                await EnsureSuccessStatusCodeAsync(response);

                var content = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<TmsCheck>(content);
                
                return result ?? new TmsCheck();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching transaction check {checkId} details: {ex.Message}");
                throw;
            }
        }

        private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                
                try
                {
                    var errorResponse = JsonConvert.DeserializeObject<PingdomApiError>(errorContent);
                    if (errorResponse?.Error != null)
                    {
                        throw new HttpRequestException(
                            $"Pingdom API error ({errorResponse.Error.StatusCode}): {errorResponse.Error.ErrorMessage}");
                    }
                }
                catch (JsonException)
                {
                    // Fall back to generic error if JSON parsing fails
                }

                throw new HttpRequestException(
                    $"HTTP {(int)response.StatusCode} {response.StatusCode}: {errorContent}");
            }
        }

        /// <summary>
        /// Implements rate limiting by adding delay between requests
        /// </summary>
        public async Task DelayForRateLimitAsync()
        {
            if (_config.RequestDelayMs > 0)
            {
                await Task.Delay(_config.RequestDelayMs);
            }
        }
    }
}
