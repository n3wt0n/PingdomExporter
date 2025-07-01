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
            
            // HttpClient is now configured via DI in Program.cs
            // No additional configuration needed here
        }

        /// <summary>
        /// Centralized HTTP GET method with authentication, rate limiting, and error handling
        /// </summary>
        private async Task<T> GetAsync<T>(string relativeUrl) where T : new()
        {
            try
            {
                // Apply rate limiting
                await DelayForRateLimitAsync();

                // Make the request using relative URL (BaseAddress + relativeUrl)
                var response = await _httpClient.GetAsync(relativeUrl);

                await EnsureSuccessStatusCodeAsync(response);

                var content = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<T>(content);
                
                return result ?? new T();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error making GET request to {relativeUrl}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Specialized method for getting raw response content with authentication and rate limiting
        /// </summary>
        private async Task<string> GetRawAsync(string relativeUrl)
        {
            try
            {
                // Apply rate limiting
                await DelayForRateLimitAsync();

                // Make the request using relative URL (BaseAddress + relativeUrl)
                var response = await _httpClient.GetAsync(relativeUrl);

                await EnsureSuccessStatusCodeAsync(response);

                var content = await response.Content.ReadAsStringAsync();
                
                return content;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error making GET request to {relativeUrl}: {ex.Message}");
                throw;
            }
        }

        public async Task<PingdomChecksResponse> GetUptimeChecksAsync()
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
            var relativeUrl = $"checks?{queryString}";
            
            var result = await GetAsync<PingdomChecksResponse>(relativeUrl);
            
            Console.WriteLine($"Successfully fetched {result?.Checks?.Count ?? 0} uptime checks");
            return result ?? new PingdomChecksResponse();
        }

        public async Task<TmsChecksResponse> GetTransactionChecksAsync()
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
            var relativeUrl = $"tms/check?{queryString}";
            
            var result = await GetAsync<TmsChecksResponse>(relativeUrl);
            
            Console.WriteLine($"Successfully fetched {result?.Checks?.Count ?? 0} transaction checks");
            return result ?? new TmsChecksResponse();
        }

        public async Task<PingdomCheck> GetUptimeCheckDetailsAsync(int checkId)
        {
            Console.WriteLine($"Fetching details for uptime check {checkId}...");

            var queryParams = new List<string>();
            
            if (_config.IncludeTeams)
            {
                queryParams.Add("include_teams=true");
            }

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var relativeUrl = $"checks/{checkId}{queryString}";
            
            var content = await GetRawAsync(relativeUrl);
            var checkResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(content);
            
            if (checkResponse?.ContainsKey("check") == true)
            {
                var checkJson = JsonConvert.SerializeObject(checkResponse["check"]);
                var result = JsonConvert.DeserializeObject<PingdomCheck>(checkJson);
                return result ?? new PingdomCheck();
            }

            return new PingdomCheck();
        }

        public async Task<TmsCheck> GetTransactionCheckDetailsAsync(int checkId)
        {
            Console.WriteLine($"Fetching details for transaction check {checkId}...");

            var queryParams = new List<string>();
            
            if (_config.IncludeTags)
            {
                queryParams.Add("extended_tags=true");
            }

            var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var relativeUrl = $"tms/check/{checkId}{queryString}";
            
            var result = await GetAsync<TmsCheck>(relativeUrl);
            
            return result ?? new TmsCheck();
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
