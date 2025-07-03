using Xunit;
using PingdomExporter.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PingdomExporter.Tests.Models
{
    public class CommonTests
    {
        #region PingdomApiError Tests

        [Fact]
        public void PingdomApiError_DefaultConstructor_InitializesErrorProperty()
        {
            // Act
            var error = new PingdomApiError();

            // Assert
            Assert.NotNull(error.Error);
            Assert.IsType<ErrorDetails>(error.Error);
        }

        [Fact]
        public void PingdomApiError_ErrorProperty_CanBeSet()
        {
            // Arrange
            var errorDetails = new ErrorDetails
            {
                StatusCode = 500,
                StatusDescription = "Internal Server Error",
                ErrorMessage = "Something went wrong"
            };

            // Act
            var apiError = new PingdomApiError { Error = errorDetails };

            // Assert
            Assert.Equal(errorDetails, apiError.Error);
            Assert.Equal(500, apiError.Error.StatusCode);
        }

        [Fact]
        public void PingdomApiError_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var apiError = new PingdomApiError
            {
                Error = new ErrorDetails
                {
                    StatusCode = 403,
                    StatusDescription = "Forbidden",
                    ErrorMessage = "Access denied"
                }
            };

            // Act
            var json = JsonConvert.SerializeObject(apiError);
            var deserialized = JsonConvert.DeserializeObject<PingdomApiError>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(403, deserialized.Error.StatusCode);
            Assert.Equal("Forbidden", deserialized.Error.StatusDescription);
            Assert.Equal("Access denied", deserialized.Error.ErrorMessage);
        }

        #endregion

        #region ErrorDetails Tests

        [Fact]
        public void ErrorDetails_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var details = new ErrorDetails();

            // Assert
            Assert.Equal(0, details.StatusCode);
            Assert.Equal(string.Empty, details.StatusDescription);
            Assert.Equal(string.Empty, details.ErrorMessage);
        }

        [Fact]
        public void ErrorDetails_Properties_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            var details = new ErrorDetails
            {
                StatusCode = 404,
                StatusDescription = "Not Found",
                ErrorMessage = "Resource not found"
            };

            // Assert
            Assert.Equal(404, details.StatusCode);
            Assert.Equal("Not Found", details.StatusDescription);
            Assert.Equal("Resource not found", details.ErrorMessage);
        }

        [Fact]
        public void ErrorDetails_JsonSerialization_UsesCorrectPropertyNames()
        {
            // Arrange
            var details = new ErrorDetails
            {
                StatusCode = 401,
                StatusDescription = "Unauthorized",
                ErrorMessage = "Invalid credentials"
            };

            // Act
            var json = JsonConvert.SerializeObject(details);

            // Assert
            Assert.Contains("\"statuscode\":401", json);
            Assert.Contains("\"statusdesc\":\"Unauthorized\"", json);
            Assert.Contains("\"errormessage\":\"Invalid credentials\"", json);
        }

        [Fact]
        public void ErrorDetails_JsonDeserialization_WorksWithApiFormat()
        {
            // Arrange
            var json = @"{
                ""statuscode"": 429,
                ""statusdesc"": ""Too Many Requests"",
                ""errormessage"": ""Rate limit exceeded""
            }";

            // Act
            var details = JsonConvert.DeserializeObject<ErrorDetails>(json);

            // Assert
            Assert.NotNull(details);
            Assert.Equal(429, details.StatusCode);
            Assert.Equal("Too Many Requests", details.StatusDescription);
            Assert.Equal("Rate limit exceeded", details.ErrorMessage);
        }

        #endregion

        #region ExportConfiguration Tests

        [Fact]
        public void ExportConfiguration_DefaultConstructor_SetsCorrectDefaults()
        {
            // Act
            var config = new ExportConfiguration();

            // Assert
            Assert.Equal(string.Empty, config.ApiToken);
            Assert.Equal("https://api.pingdom.com/api/3.1/", config.BaseUrl);
            Assert.Equal("exports", config.OutputDirectory);
            Assert.True(config.ExportUptimeChecks);
            Assert.True(config.ExportTransactionChecks);
            Assert.True(config.IncludeTags);
            Assert.True(config.IncludeTeams);
            Assert.Equal("json", config.OutputFormat);
            Assert.Equal("Summary", config.ExportMode);
            Assert.False(config.IncludeDisabledChecks);
            Assert.Equal(1000, config.RequestDelayMs);
            Assert.False(config.AutoMode);
            Assert.False(config.VerboseMode);
        }

        [Fact]
        public void ExportConfiguration_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            var config = new ExportConfiguration
            {
                ApiToken = "test-token-123",
                BaseUrl = "https://custom.api.com/v2/",
                OutputDirectory = "custom-exports",
                ExportUptimeChecks = false,
                ExportTransactionChecks = false,
                IncludeTags = false,
                IncludeTeams = false,
                OutputFormat = "csv",
                ExportMode = "Full",
                IncludeDisabledChecks = true,
                RequestDelayMs = 2000,
                AutoMode = true,
                VerboseMode = true
            };

            // Assert
            Assert.Equal("test-token-123", config.ApiToken);
            Assert.Equal("https://custom.api.com/v2/", config.BaseUrl);
            Assert.Equal("custom-exports", config.OutputDirectory);
            Assert.False(config.ExportUptimeChecks);
            Assert.False(config.ExportTransactionChecks);
            Assert.False(config.IncludeTags);
            Assert.False(config.IncludeTeams);
            Assert.Equal("csv", config.OutputFormat);
            Assert.Equal("Full", config.ExportMode);
            Assert.True(config.IncludeDisabledChecks);
            Assert.Equal(2000, config.RequestDelayMs);
            Assert.True(config.AutoMode);
            Assert.True(config.VerboseMode);
        }

        [Theory]
        [InlineData("json")]
        [InlineData("csv")]
        [InlineData("both")]
        public void ExportConfiguration_OutputFormat_AcceptsValidValues(string format)
        {
            // Arrange & Act
            var config = new ExportConfiguration { OutputFormat = format };

            // Assert
            Assert.Equal(format, config.OutputFormat);
        }

        [Theory]
        [InlineData("Full")]
        [InlineData("Summary")]
        [InlineData("UptimeRobot")]
        public void ExportConfiguration_ExportMode_AcceptsValidValues(string mode)
        {
            // Arrange & Act
            var config = new ExportConfiguration { ExportMode = mode };

            // Assert
            Assert.Equal(mode, config.ExportMode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(5000)]
        public void ExportConfiguration_RequestDelayMs_AcceptsValidValues(int delay)
        {
            // Arrange & Act
            var config = new ExportConfiguration { RequestDelayMs = delay };

            // Assert
            Assert.Equal(delay, config.RequestDelayMs);
        }

        #endregion

        #region UptimeRobotMonitor Tests

        [Fact]
        public void UptimeRobotMonitor_DefaultConstructor_SetsCorrectDefaults()
        {
            // Act
            var monitor = new UptimeRobotMonitor();

            // Assert
            Assert.Equal("HTTP", monitor.Type);
            Assert.Equal(string.Empty, monitor.FriendlyName);
            Assert.Equal(string.Empty, monitor.UrlIp);
            Assert.Equal(300, monitor.Interval);
            Assert.Equal(string.Empty, monitor.KeywordType);
            Assert.Equal(string.Empty, monitor.KeywordValue);
            Assert.Equal(string.Empty, monitor.Port);
        }

        [Fact]
        public void UptimeRobotMonitor_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange & Act
            var monitor = new UptimeRobotMonitor
            {
                Type = "HTTPS",
                FriendlyName = "Test Website",
                UrlIp = "https://example.com",
                Interval = 600,
                KeywordType = "exists",
                KeywordValue = "Welcome",
                Port = "443"
            };

            // Assert
            Assert.Equal("HTTPS", monitor.Type);
            Assert.Equal("Test Website", monitor.FriendlyName);
            Assert.Equal("https://example.com", monitor.UrlIp);
            Assert.Equal(600, monitor.Interval);
            Assert.Equal("exists", monitor.KeywordType);
            Assert.Equal("Welcome", monitor.KeywordValue);
            Assert.Equal("443", monitor.Port);
        }

        [Theory]
        [InlineData("HTTP")]
        [InlineData("HTTPS")]
        [InlineData("PING")]
        [InlineData("PORT")]
        public void UptimeRobotMonitor_Type_AcceptsValidValues(string type)
        {
            // Arrange & Act
            var monitor = new UptimeRobotMonitor { Type = type };

            // Assert
            Assert.Equal(type, monitor.Type);
        }

        [Theory]
        [InlineData(60)]    // 1 minute
        [InlineData(300)]   // 5 minutes
        [InlineData(600)]   // 10 minutes
        [InlineData(900)]   // 15 minutes
        [InlineData(1800)]  // 30 minutes
        public void UptimeRobotMonitor_Interval_AcceptsValidValues(int interval)
        {
            // Arrange & Act
            var monitor = new UptimeRobotMonitor { Interval = interval };

            // Assert
            Assert.Equal(interval, monitor.Interval);
        }

        [Theory]
        [InlineData("exists")]
        [InlineData("not exists")]
        public void UptimeRobotMonitor_KeywordType_AcceptsValidValues(string keywordType)
        {
            // Arrange & Act
            var monitor = new UptimeRobotMonitor { KeywordType = keywordType };

            // Assert
            Assert.Equal(keywordType, monitor.KeywordType);
        }

        #endregion

        #region ExportSummary Tests

        [Fact]
        public void ExportSummary_DefaultConstructor_InitializesCollections()
        {
            // Act
            var summary = new ExportSummary();

            // Assert
            Assert.NotNull(summary.Errors);
            Assert.NotNull(summary.Warnings);
            Assert.Empty(summary.Errors);
            Assert.Empty(summary.Warnings);
            Assert.Equal(default(DateTime), summary.ExportDate);
            Assert.Equal(0, summary.UptimeChecksExported);
            Assert.Equal(0, summary.TransactionChecksExported);
            Assert.Equal(0, summary.TotalChecksExported);
            Assert.Equal(default(TimeSpan), summary.Duration);
        }

        [Fact]
        public void ExportSummary_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var exportDate = DateTime.UtcNow;
            var duration = TimeSpan.FromMinutes(5);
            var errors = new List<string> { "Error 1", "Error 2" };
            var warnings = new List<string> { "Warning 1" };

            // Act
            var summary = new ExportSummary
            {
                ExportDate = exportDate,
                UptimeChecksExported = 10,
                TransactionChecksExported = 5,
                TotalChecksExported = 15,
                Duration = duration,
                Errors = errors,
                Warnings = warnings
            };

            // Assert
            Assert.Equal(exportDate, summary.ExportDate);
            Assert.Equal(10, summary.UptimeChecksExported);
            Assert.Equal(5, summary.TransactionChecksExported);
            Assert.Equal(15, summary.TotalChecksExported);
            Assert.Equal(duration, summary.Duration);
            Assert.Equal(errors, summary.Errors);
            Assert.Equal(warnings, summary.Warnings);
            Assert.Equal(2, summary.Errors.Count);
            Assert.Single(summary.Warnings);
        }

        [Fact]
        public void ExportSummary_ErrorsCollection_CanBeModified()
        {
            // Arrange
            var summary = new ExportSummary();

            // Act
            summary.Errors.Add("First error");
            summary.Errors.Add("Second error");

            // Assert
            Assert.Equal(2, summary.Errors.Count);
            Assert.Contains("First error", summary.Errors);
            Assert.Contains("Second error", summary.Errors);
        }

        [Fact]
        public void ExportSummary_WarningsCollection_CanBeModified()
        {
            // Arrange
            var summary = new ExportSummary();

            // Act
            summary.Warnings.Add("First warning");
            summary.Warnings.Add("Second warning");

            // Assert
            Assert.Equal(2, summary.Warnings.Count);
            Assert.Contains("First warning", summary.Warnings);
            Assert.Contains("Second warning", summary.Warnings);
        }

        [Fact]
        public void ExportSummary_TotalChecksExported_CanBeDifferentFromSum()
        {
            // This tests that TotalChecksExported is independent and not calculated
            // Arrange & Act
            var summary = new ExportSummary
            {
                UptimeChecksExported = 5,
                TransactionChecksExported = 3,
                TotalChecksExported = 10 // Different from 5+3=8
            };

            // Assert
            Assert.Equal(5, summary.UptimeChecksExported);
            Assert.Equal(3, summary.TransactionChecksExported);
            Assert.Equal(10, summary.TotalChecksExported);
        }

        [Fact]
        public void ExportSummary_Duration_AcceptsVariousTimeSpans()
        {
            // Arrange
            var summary = new ExportSummary();
            var testDurations = new[]
            {
                TimeSpan.FromMilliseconds(500),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMinutes(2),
                TimeSpan.FromHours(1)
            };

            foreach (var duration in testDurations)
            {
                // Act
                summary.Duration = duration;

                // Assert
                Assert.Equal(duration, summary.Duration);
            }
        }

        [Fact]
        public void ExportSummary_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var summary = new ExportSummary
            {
                ExportDate = new DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc),
                UptimeChecksExported = 10,
                TransactionChecksExported = 5,
                TotalChecksExported = 15,
                Duration = TimeSpan.FromMinutes(2),
                Errors = new List<string> { "Test error" },
                Warnings = new List<string> { "Test warning" }
            };

            // Act
            var json = JsonConvert.SerializeObject(summary);
            var deserialized = JsonConvert.DeserializeObject<ExportSummary>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(summary.ExportDate, deserialized.ExportDate);
            Assert.Equal(summary.UptimeChecksExported, deserialized.UptimeChecksExported);
            Assert.Equal(summary.TransactionChecksExported, deserialized.TransactionChecksExported);
            Assert.Equal(summary.TotalChecksExported, deserialized.TotalChecksExported);
            Assert.Equal(summary.Duration, deserialized.Duration);
            Assert.Equal(summary.Errors, deserialized.Errors);
            Assert.Equal(summary.Warnings, deserialized.Warnings);
        }

        #endregion
    }
}
