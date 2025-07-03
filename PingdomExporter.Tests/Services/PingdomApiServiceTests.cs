using Xunit;
using Moq;
using Moq.Protected;
using PingdomExporter.Services;
using PingdomExporter.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PingdomExporter.Tests.Services
{
    public class PingdomApiServiceTests : IDisposable
    {
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly ExportConfiguration _config;
        private readonly PingdomApiService _apiService;

        public PingdomApiServiceTests()
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://api.pingdom.com/api/3.1/")
            };
            
            _config = new ExportConfiguration
            {
                ApiToken = "test-token",
                BaseUrl = "https://api.pingdom.com/api/3.1/",
                RequestDelayMs = 0, // No delay for tests
                IncludeTags = true,
                IncludeTeams = true
            };

            _apiService = new PingdomApiService(_httpClient, _config);
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #region Constructor Tests

        [Fact]
        public void Constructor_NullHttpClient_Throws()
        {
            var config = new ExportConfiguration();
            Assert.Throws<ArgumentNullException>(() => new PingdomApiService(null!, config));
        }

        [Fact]
        public void Constructor_NullConfig_Throws()
        {
            var httpClient = new HttpClient();
            Assert.Throws<ArgumentNullException>(() => new PingdomApiService(httpClient, null!));
        }

        [Fact]
        public void Constructor_ValidParameters_CreatesInstance()
        {
            var httpClient = new HttpClient();
            var config = new ExportConfiguration();
            var service = new PingdomApiService(httpClient, config);
            Assert.NotNull(service);
        }

        #endregion

        #region GetUptimeChecksAsync Tests

        [Fact]
        public async Task GetUptimeChecksAsync_SuccessfulResponse_ReturnsChecks()
        {
            // Arrange
            var expectedChecks = CreateSampleUptimeChecks();
            var response = new PingdomChecksResponse { Checks = expectedChecks };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            var result = await _apiService.GetUptimeChecksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Checks.Count);
            Assert.Equal("Test Check 1", result.Checks[0].Name);
            Assert.Equal("example1.com", result.Checks[0].Hostname);

            VerifyHttpRequest("checks?limit=25000&include_tags=true");
        }

        [Fact]
        public async Task GetUptimeChecksAsync_WithoutTags_ExcludesTagsParameter()
        {
            // Arrange
            _config.IncludeTags = false;
            var response = new PingdomChecksResponse { Checks = new List<PingdomCheck>() };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            await _apiService.GetUptimeChecksAsync();

            // Assert
            VerifyHttpRequest("checks?limit=25000");
        }

        [Fact]
        public async Task GetUptimeChecksAsync_EmptyResponse_ReturnsEmptyList()
        {
            // Arrange
            var response = new PingdomChecksResponse { Checks = new List<PingdomCheck>() };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            var result = await _apiService.GetUptimeChecksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Checks);
        }

        [Fact]
        public async Task GetUptimeChecksAsync_NullResponse_ReturnsNewInstance()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.OK, "null");

            // Act
            var result = await _apiService.GetUptimeChecksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.Checks);
        }

        #endregion

        #region GetTransactionChecksAsync Tests

        [Fact]
        public async Task GetTransactionChecksAsync_SuccessfulResponse_ReturnsChecks()
        {
            // Arrange
            var expectedChecks = CreateSampleTransactionChecks();
            var response = new TmsChecksResponse { Checks = expectedChecks };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            var result = await _apiService.GetTransactionChecksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Checks.Count);
            Assert.Equal("Transaction Check 1", result.Checks[0].Name);
            Assert.True(result.Checks[0].Active);

            VerifyHttpRequest("tms/check?limit=1000&extended_tags=true");
        }

        [Fact]
        public async Task GetTransactionChecksAsync_WithoutTags_ExcludesTagsParameter()
        {
            // Arrange
            _config.IncludeTags = false;
            var response = new TmsChecksResponse { Checks = new List<TmsCheck>() };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            await _apiService.GetTransactionChecksAsync();

            // Assert
            VerifyHttpRequest("tms/check?limit=1000");
        }

        [Fact]
        public async Task GetTransactionChecksAsync_EmptyResponse_ReturnsEmptyList()
        {
            // Arrange
            var response = new TmsChecksResponse { Checks = new List<TmsCheck>() };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            var result = await _apiService.GetTransactionChecksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result.Checks);
        }

        #endregion

        #region GetUptimeCheckDetailsAsync Tests

        [Fact]
        public async Task GetUptimeCheckDetailsAsync_SuccessfulResponse_ReturnsCheckDetails()
        {
            // Arrange
            var checkDetails = CreateDetailedUptimeCheck();
            var response = new { check = checkDetails };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            var result = await _apiService.GetUptimeCheckDetailsAsync(123);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Detailed Check", result.Name);
            Assert.Equal("detailed.example.com", result.Hostname);
            Assert.Single(result.Tags);
            Assert.Equal("production", result.Tags[0].Name);

            VerifyHttpRequest("checks/123?include_teams=true");
        }

        [Fact]
        public async Task GetUptimeCheckDetailsAsync_WithoutTeams_ExcludesTeamsParameter()
        {
            // Arrange
            _config.IncludeTeams = false;
            var checkDetails = CreateDetailedUptimeCheck();
            var response = new { check = checkDetails };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            await _apiService.GetUptimeCheckDetailsAsync(123);

            // Assert
            VerifyHttpRequest("checks/123");
        }

        [Fact]
        public async Task GetUptimeCheckDetailsAsync_NoCheckInResponse_ReturnsEmptyCheck()
        {
            // Arrange
            var response = new { other_data = "value" };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            var result = await _apiService.GetUptimeCheckDetailsAsync(123);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id); // Default value
        }

        [Fact]
        public async Task GetUptimeCheckDetailsAsync_EmptyResponse_ReturnsEmptyCheck()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.OK, "{}");

            // Act
            var result = await _apiService.GetUptimeCheckDetailsAsync(123);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id); // Default value
        }

        #endregion

        #region GetTransactionCheckDetailsAsync Tests

        [Fact]
        public async Task GetTransactionCheckDetailsAsync_SuccessfulResponse_ReturnsCheckDetails()
        {
            // Arrange
            var checkDetails = CreateDetailedTransactionCheck();
            var jsonResponse = JsonConvert.SerializeObject(checkDetails);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            var result = await _apiService.GetTransactionCheckDetailsAsync(456);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Detailed Transaction Check", result.Name);
            Assert.True(result.Active);
            Assert.Equal(300, result.Interval);

            VerifyHttpRequest("tms/check/456?extended_tags=true");
        }

        [Fact]
        public async Task GetTransactionCheckDetailsAsync_WithoutTags_ExcludesTagsParameter()
        {
            // Arrange
            _config.IncludeTags = false;
            var checkDetails = CreateDetailedTransactionCheck();
            var jsonResponse = JsonConvert.SerializeObject(checkDetails);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            await _apiService.GetTransactionCheckDetailsAsync(456);

            // Assert
            VerifyHttpRequest("tms/check/456");
        }

        [Fact]
        public async Task GetTransactionCheckDetailsAsync_EmptyResponse_ReturnsEmptyCheck()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.OK, "{}");

            // Act
            var result = await _apiService.GetTransactionCheckDetailsAsync(456);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(0, result.Id); // Default value
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task GetUptimeChecksAsync_HttpError_ThrowsHttpRequestException()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.Unauthorized, "Unauthorized");

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => _apiService.GetUptimeChecksAsync());
            
            Assert.Contains("HTTP 401 Unauthorized", exception.Message);
        }

        [Fact]
        public async Task GetUptimeChecksAsync_PingdomApiError_ThrowsWithApiErrorMessage()
        {
            // Arrange
            var apiError = new PingdomApiError
            {
                Error = new ErrorDetails
                {
                    StatusCode = 403,
                    ErrorMessage = "Forbidden: Invalid API token"
                }
            };
            var jsonResponse = JsonConvert.SerializeObject(apiError);

            SetupHttpResponse(HttpStatusCode.Forbidden, jsonResponse);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => _apiService.GetUptimeChecksAsync());
            
            Assert.Contains("Pingdom API error (403): Forbidden: Invalid API token", exception.Message);
        }

        [Fact]
        public async Task GetTransactionChecksAsync_NetworkError_ThrowsHttpRequestException()
        {
            // Arrange
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(), 
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<HttpRequestException>(
                () => _apiService.GetTransactionChecksAsync());
            
            Assert.Contains("Network error", exception.Message);
        }

        [Fact]
        public async Task GetUptimeCheckDetailsAsync_InvalidJson_ThrowsJsonException()
        {
            // Arrange
            SetupHttpResponse(HttpStatusCode.OK, "invalid json {");

            // Act & Assert
            await Assert.ThrowsAsync<JsonReaderException>(
                () => _apiService.GetUptimeCheckDetailsAsync(123));
        }

        #endregion

        #region Rate Limiting Tests

        [Fact]
        public async Task DelayForRateLimitAsync_WithDelay_WaitsSpecifiedTime()
        {
            // Arrange
            _config.RequestDelayMs = 100;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await _apiService.DelayForRateLimitAsync();

            // Assert
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds >= 90); // Allow some tolerance
        }

        [Fact]
        public async Task DelayForRateLimitAsync_WithZeroDelay_DoesNotWait()
        {
            // Arrange
            _config.RequestDelayMs = 0;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await _apiService.DelayForRateLimitAsync();

            // Assert
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 50); // Should be very fast
        }

        [Fact]
        public async Task DelayForRateLimitAsync_WithNegativeDelay_DoesNotWait()
        {
            // Arrange
            _config.RequestDelayMs = -100;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await _apiService.DelayForRateLimitAsync();

            // Assert
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds < 50); // Should be very fast
        }

        [Fact]
        public async Task GetUptimeChecksAsync_WithRateLimit_AppliesDelay()
        {
            // Arrange
            _config.RequestDelayMs = 50;
            var response = new PingdomChecksResponse { Checks = new List<PingdomCheck>() };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Act
            await _apiService.GetUptimeChecksAsync();

            // Assert
            stopwatch.Stop();
            Assert.True(stopwatch.ElapsedMilliseconds >= 40); // Allow some tolerance
        }

        #endregion

        #region Query Parameter Tests

        [Fact]
        public async Task GetUptimeChecksAsync_AllConfigOptions_BuildsCorrectUrl()
        {
            // Arrange
            _config.IncludeTags = true;
            var response = new PingdomChecksResponse { Checks = new List<PingdomCheck>() };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            await _apiService.GetUptimeChecksAsync();

            // Assert
            VerifyHttpRequest("checks?limit=25000&include_tags=true");
        }

        [Fact]
        public async Task GetTransactionChecksAsync_AllConfigOptions_BuildsCorrectUrl()
        {
            // Arrange
            _config.IncludeTags = true;
            var response = new TmsChecksResponse { Checks = new List<TmsCheck>() };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            await _apiService.GetTransactionChecksAsync();

            // Assert
            VerifyHttpRequest("tms/check?limit=1000&extended_tags=true");
        }

        [Fact]
        public async Task GetUptimeCheckDetailsAsync_AllConfigOptions_BuildsCorrectUrl()
        {
            // Arrange
            _config.IncludeTeams = true;
            var checkDetails = CreateDetailedUptimeCheck();
            var response = new { check = checkDetails };
            var jsonResponse = JsonConvert.SerializeObject(response);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            await _apiService.GetUptimeCheckDetailsAsync(789);

            // Assert
            VerifyHttpRequest("checks/789?include_teams=true");
        }

        [Fact]
        public async Task GetTransactionCheckDetailsAsync_AllConfigOptions_BuildsCorrectUrl()
        {
            // Arrange
            _config.IncludeTags = true;
            var checkDetails = CreateDetailedTransactionCheck();
            var jsonResponse = JsonConvert.SerializeObject(checkDetails);

            SetupHttpResponse(HttpStatusCode.OK, jsonResponse);

            // Act
            await _apiService.GetTransactionCheckDetailsAsync(789);

            // Assert
            VerifyHttpRequest("tms/check/789?extended_tags=true");
        }

        #endregion

        #region Helper Methods

        private void SetupHttpResponse(HttpStatusCode statusCode, string content)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(content, Encoding.UTF8, "application/json")
            };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>("SendAsync", 
                    ItExpr.IsAny<HttpRequestMessage>(), 
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);
        }

        private void VerifyHttpRequest(string expectedRelativeUrl)
        {
            _mockHttpMessageHandler.Protected()
                .Verify("SendAsync", Times.Once(),
                    ItExpr.Is<HttpRequestMessage>(req => 
                        req.RequestUri!.ToString().EndsWith(expectedRelativeUrl)),
                    ItExpr.IsAny<CancellationToken>());
        }

        private List<PingdomCheck> CreateSampleUptimeChecks()
        {
            return new List<PingdomCheck>
            {
                new PingdomCheck
                {
                    Id = 1,
                    Name = "Test Check 1",
                    Hostname = "example1.com",
                    Status = "up",
                    Type = "http",
                    Resolution = 5,
                    LastTestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Created = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds()
                },
                new PingdomCheck
                {
                    Id = 2,
                    Name = "Test Check 2",
                    Hostname = "example2.com",
                    Status = "up",
                    Type = "ping",
                    Resolution = 1,
                    LastTestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    Created = DateTimeOffset.UtcNow.AddDays(-15).ToUnixTimeSeconds()
                }
            };
        }

        private List<TmsCheck> CreateSampleTransactionChecks()
        {
            return new List<TmsCheck>
            {
                new TmsCheck
                {
                    Id = 101,
                    Name = "Transaction Check 1",
                    Active = true,
                    Status = "up",
                    Interval = 300,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-20).ToUnixTimeSeconds()
                },
                new TmsCheck
                {
                    Id = 102,
                    Name = "Transaction Check 2",
                    Active = true,
                    Status = "up",
                    Interval = 600,
                    CreatedAt = DateTimeOffset.UtcNow.AddDays(-10).ToUnixTimeSeconds()
                }
            };
        }

        private PingdomCheck CreateDetailedUptimeCheck()
        {
            return new PingdomCheck
            {
                Id = 123,
                Name = "Detailed Check",
                Hostname = "detailed.example.com",
                Status = "up",
                Type = "http",
                Resolution = 5,
                LastTestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                Created = DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeSeconds(),
                Tags = new List<Tag>
                {
                    new Tag { Name = "production", Type = "auto" }
                },
                TeamIds = new List<int> { 1, 2 },
                UserIds = new List<int> { 10, 20 }
            };
        }

        private TmsCheck CreateDetailedTransactionCheck()
        {
            return new TmsCheck
            {
                Id = 456,
                Name = "Detailed Transaction Check",
                Active = true,
                Status = "up",
                Interval = 300,
                CreatedAt = DateTimeOffset.UtcNow.AddDays(-20).ToUnixTimeSeconds()
            };
        }

        #endregion
    }
}
