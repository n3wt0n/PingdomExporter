using Xunit;
using PingdomExporter.Models;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace PingdomExporter.Tests.Models
{
    public class TmsCheckTests
    {
        #region TmsCheck Tests

        [Fact]
        public void TmsCheck_DefaultConstructor_SetsCorrectDefaults()
        {
            // Act
            var check = new TmsCheck();

            // Assert
            Assert.Equal(0, check.Id);
            Assert.Equal(string.Empty, check.Name);
            Assert.False(check.Active);
            Assert.Equal(0, check.CreatedAt);
            Assert.Equal(0, check.ModifiedAt);
            Assert.Null(check.LastDowntimeStart);
            Assert.Null(check.LastDowntimeEnd);
            Assert.Equal(string.Empty, check.CustomMessage);
            Assert.Equal(0, check.Interval);
            Assert.Equal(string.Empty, check.Region);
            Assert.Equal(0, check.SendNotificationWhenDown);
            Assert.Equal(string.Empty, check.SeverityLevel);
            Assert.Equal(string.Empty, check.Status);
            Assert.NotNull(check.Steps);
            Assert.Empty(check.Steps);
            Assert.NotNull(check.TeamIds);
            Assert.Empty(check.TeamIds);
            Assert.NotNull(check.ContactIds);
            Assert.Empty(check.ContactIds);
            Assert.NotNull(check.IntegrationIds);
            Assert.Empty(check.IntegrationIds);
            Assert.NotNull(check.Tags);
            Assert.Empty(check.Tags);
            Assert.NotNull(check.Type);
            Assert.Empty(check.Type);
            Assert.Null(check.Metadata);
        }

        [Fact]
        public void TmsCheck_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var steps = new List<TmsStep> { new TmsStep { Function = "navigate", Args = new Dictionary<string, object> { { "url", "https://example.com" } } } };
            var teamIds = new List<int> { 1, 2, 3 };
            var contactIds = new List<int> { 10, 20 };
            var integrationIds = new List<int> { 100, 200 };
            var tags = new List<string> { "production", "api" };
            var type = new List<string> { "real_browser" };
            var metadata = new TmsMetadata { Width = 1920, Height = 1080, DisableWebSecurity = false };

            // Act
            var check = new TmsCheck
            {
                Id = 12345,
                Name = "Transaction Test",
                Active = true,
                CreatedAt = 1640995200,
                ModifiedAt = 1641081600,
                LastDowntimeStart = 1641000000,
                LastDowntimeEnd = 1641003600,
                CustomMessage = "Custom transaction message",
                Interval = 300,
                Region = "us-east-1",
                SendNotificationWhenDown = 2,
                SeverityLevel = "high",
                Status = "up",
                Steps = steps,
                TeamIds = teamIds,
                ContactIds = contactIds,
                IntegrationIds = integrationIds,
                Tags = tags,
                Type = type,
                Metadata = metadata
            };

            // Assert
            Assert.Equal(12345, check.Id);
            Assert.Equal("Transaction Test", check.Name);
            Assert.True(check.Active);
            Assert.Equal(1640995200, check.CreatedAt);
            Assert.Equal(1641081600, check.ModifiedAt);
            Assert.Equal(1641000000, check.LastDowntimeStart);
            Assert.Equal(1641003600, check.LastDowntimeEnd);
            Assert.Equal("Custom transaction message", check.CustomMessage);
            Assert.Equal(300, check.Interval);
            Assert.Equal("us-east-1", check.Region);
            Assert.Equal(2, check.SendNotificationWhenDown);
            Assert.Equal("high", check.SeverityLevel);
            Assert.Equal("up", check.Status);
            Assert.Equal(steps, check.Steps);
            Assert.Equal(teamIds, check.TeamIds);
            Assert.Equal(contactIds, check.ContactIds);
            Assert.Equal(integrationIds, check.IntegrationIds);
            Assert.Equal(tags, check.Tags);
            Assert.Equal(type, check.Type);
            Assert.Equal(metadata, check.Metadata);
        }

        [Fact]
        public void TmsCheck_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var check = new TmsCheck
            {
                Id = 456,
                Name = "API Transaction",
                Active = true,
                Interval = 600,
                Region = "eu-west-1"
            };

            // Act
            var json = JsonConvert.SerializeObject(check);
            var deserialized = JsonConvert.DeserializeObject<TmsCheck>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(check.Id, deserialized.Id);
            Assert.Equal(check.Name, deserialized.Name);
            Assert.Equal(check.Active, deserialized.Active);
            Assert.Equal(check.Interval, deserialized.Interval);
            Assert.Equal(check.Region, deserialized.Region);
        }

        [Fact]
        public void TmsCheck_JsonDeserialization_WithApiFormat_WorksCorrectly()
        {
            // Arrange
            var json = @"{
                ""id"": 789,
                ""name"": ""E-commerce Flow"",
                ""active"": true,
                ""created_at"": 1640995200,
                ""modified_at"": 1641081600,
                ""custom_message"": ""E-commerce transaction test"",
                ""interval"": 900,
                ""region"": ""us-west-2"",
                ""send_notification_when_down"": 1,
                ""severity_level"": ""medium"",
                ""status"": ""up"",
                ""steps"": [
                    {""fn"": ""navigate"", ""args"": {""url"": ""https://shop.example.com""}}
                ],
                ""team_ids"": [1, 2],
                ""tags"": [""ecommerce"", ""critical""]
            }";

            // Act
            var check = JsonConvert.DeserializeObject<TmsCheck>(json);

            // Assert
            Assert.NotNull(check);
            Assert.Equal(789, check.Id);
            Assert.Equal("E-commerce Flow", check.Name);
            Assert.True(check.Active);
            Assert.Equal(1640995200, check.CreatedAt);
            Assert.Equal(1641081600, check.ModifiedAt);
            Assert.Equal("E-commerce transaction test", check.CustomMessage);
            Assert.Equal(900, check.Interval);
            Assert.Equal("us-west-2", check.Region);
            Assert.Equal(1, check.SendNotificationWhenDown);
            Assert.Equal("medium", check.SeverityLevel);
            Assert.Equal("up", check.Status);
            Assert.Single(check.Steps);
            Assert.Equal("navigate", check.Steps[0].Function);
            Assert.Equal(2, check.TeamIds.Count);
            Assert.Contains(1, check.TeamIds);
            Assert.Contains(2, check.TeamIds);
            Assert.Equal(2, check.Tags.Count);
            Assert.Contains("ecommerce", check.Tags);
            Assert.Contains("critical", check.Tags);
        }

        [Theory]
        [InlineData(60)]
        [InlineData(300)]
        [InlineData(600)]
        [InlineData(900)]
        [InlineData(1800)]
        public void TmsCheck_Interval_AcceptsValidValues(int interval)
        {
            // Act
            var check = new TmsCheck { Interval = interval };

            // Assert
            Assert.Equal(interval, check.Interval);
        }

        [Theory]
        [InlineData("low")]
        [InlineData("medium")]
        [InlineData("high")]
        [InlineData("critical")]
        public void TmsCheck_SeverityLevel_AcceptsValidValues(string severityLevel)
        {
            // Act
            var check = new TmsCheck { SeverityLevel = severityLevel };

            // Assert
            Assert.Equal(severityLevel, check.SeverityLevel);
        }

        [Theory]
        [InlineData("up")]
        [InlineData("down")]
        [InlineData("paused")]
        [InlineData("unknown")]
        public void TmsCheck_Status_AcceptsValidValues(string status)
        {
            // Act
            var check = new TmsCheck { Status = status };

            // Assert
            Assert.Equal(status, check.Status);
        }

        [Theory]
        [InlineData("us-east-1")]
        [InlineData("us-west-2")]
        [InlineData("eu-west-1")]
        [InlineData("ap-southeast-1")]
        public void TmsCheck_Region_AcceptsValidValues(string region)
        {
            // Act
            var check = new TmsCheck { Region = region };

            // Assert
            Assert.Equal(region, check.Region);
        }

        #endregion

        #region TmsStep Tests

        [Fact]
        public void TmsStep_DefaultConstructor_SetsCorrectDefaults()
        {
            // Act
            var step = new TmsStep();

            // Assert
            Assert.Equal(string.Empty, step.Function);
            Assert.NotNull(step.Args);
            Assert.Empty(step.Args);
            Assert.Null(step.Guid);
            Assert.Null(step.ContainsNavigate);
        }

        [Fact]
        public void TmsStep_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var args = new Dictionary<string, object>
            {
                { "url", "https://example.com" },
                { "timeout", 30000 },
                { "waitForElement", ".login-button" }
            };

            // Act
            var step = new TmsStep
            {
                Function = "navigate",
                Args = args,
                Guid = "12345678-1234-1234-1234-123456789abc",
                ContainsNavigate = true
            };

            // Assert
            Assert.Equal("navigate", step.Function);
            Assert.Equal(args, step.Args);
            Assert.Equal("12345678-1234-1234-1234-123456789abc", step.Guid);
            Assert.True(step.ContainsNavigate);
        }

        [Fact]
        public void TmsStep_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var step = new TmsStep
            {
                Function = "click",
                Args = new Dictionary<string, object> { { "selector", "#submit-button" } },
                Guid = "test-guid-123"
            };

            // Act
            var json = JsonConvert.SerializeObject(step);
            var deserialized = JsonConvert.DeserializeObject<TmsStep>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(step.Function, deserialized.Function);
            Assert.Equal(step.Guid, deserialized.Guid);
            Assert.True(deserialized.Args.ContainsKey("selector"));
        }

        [Theory]
        [InlineData("navigate")]
        [InlineData("click")]
        [InlineData("type")]
        [InlineData("wait")]
        [InlineData("assert")]
        public void TmsStep_Function_AcceptsValidValues(string function)
        {
            // Act
            var step = new TmsStep { Function = function };

            // Assert
            Assert.Equal(function, step.Function);
        }

        [Fact]
        public void TmsStep_Args_CanContainVariousTypes()
        {
            // Arrange
            var args = new Dictionary<string, object>
            {
                { "stringValue", "test" },
                { "intValue", 42 },
                { "boolValue", true },
                { "doubleValue", 3.14 }
            };

            // Act
            var step = new TmsStep { Args = args };

            // Assert
            Assert.Equal("test", step.Args["stringValue"]);
            Assert.Equal(42, step.Args["intValue"]);
            Assert.Equal(true, step.Args["boolValue"]);
            Assert.Equal(3.14, step.Args["doubleValue"]);
        }

        #endregion

        #region TmsMetadata Tests

        [Fact]
        public void TmsMetadata_DefaultConstructor_SetsCorrectDefaults()
        {
            // Act
            var metadata = new TmsMetadata();

            // Assert
            Assert.Equal(0, metadata.Width);
            Assert.Equal(0, metadata.Height);
            Assert.False(metadata.DisableWebSecurity);
            Assert.Null(metadata.Authentications);
        }

        [Fact]
        public void TmsMetadata_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var authentications = new Dictionary<string, object>
            {
                { "basic", new { username = "user", password = "pass" } },
                { "oauth", new { token = "abc123" } }
            };

            // Act
            var metadata = new TmsMetadata
            {
                Width = 1920,
                Height = 1080,
                DisableWebSecurity = true,
                Authentications = authentications
            };

            // Assert
            Assert.Equal(1920, metadata.Width);
            Assert.Equal(1080, metadata.Height);
            Assert.True(metadata.DisableWebSecurity);
            Assert.Equal(authentications, metadata.Authentications);
        }

        [Fact]
        public void TmsMetadata_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var metadata = new TmsMetadata
            {
                Width = 1366,
                Height = 768,
                DisableWebSecurity = false
            };

            // Act
            var json = JsonConvert.SerializeObject(metadata);
            var deserialized = JsonConvert.DeserializeObject<TmsMetadata>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(metadata.Width, deserialized.Width);
            Assert.Equal(metadata.Height, deserialized.Height);
            Assert.Equal(metadata.DisableWebSecurity, deserialized.DisableWebSecurity);
        }

        [Theory]
        [InlineData(1024, 768)]
        [InlineData(1366, 768)]
        [InlineData(1920, 1080)]
        [InlineData(2560, 1440)]
        public void TmsMetadata_Resolution_AcceptsValidValues(int width, int height)
        {
            // Act
            var metadata = new TmsMetadata
            {
                Width = width,
                Height = height
            };

            // Assert
            Assert.Equal(width, metadata.Width);
            Assert.Equal(height, metadata.Height);
        }

        #endregion

        #region TmsChecksResponse Tests

        [Fact]
        public void TmsChecksResponse_DefaultConstructor_InitializesCollections()
        {
            // Act
            var response = new TmsChecksResponse();

            // Assert
            Assert.NotNull(response.Checks);
            Assert.Empty(response.Checks);
            Assert.Equal(0, response.Limit);
            Assert.Equal(0, response.Offset);
        }

        [Fact]
        public void TmsChecksResponse_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var checks = new List<TmsCheck>
            {
                new TmsCheck { Id = 1, Name = "Test 1" },
                new TmsCheck { Id = 2, Name = "Test 2" }
            };

            // Act
            var response = new TmsChecksResponse
            {
                Checks = checks,
                Limit = 100,
                Offset = 0
            };

            // Assert
            Assert.Equal(checks, response.Checks);
            Assert.Equal(100, response.Limit);
            Assert.Equal(0, response.Offset);
        }

        [Fact]
        public void TmsChecksResponse_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var response = new TmsChecksResponse
            {
                Checks = new List<TmsCheck> { new TmsCheck { Id = 123, Name = "Test Transaction" } },
                Limit = 50,
                Offset = 10
            };

            // Act
            var json = JsonConvert.SerializeObject(response);
            var deserialized = JsonConvert.DeserializeObject<TmsChecksResponse>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Single(deserialized.Checks);
            Assert.Equal(123, deserialized.Checks[0].Id);
            Assert.Equal("Test Transaction", deserialized.Checks[0].Name);
            Assert.Equal(50, deserialized.Limit);
            Assert.Equal(10, deserialized.Offset);
        }

        [Fact]
        public void TmsChecksResponse_JsonDeserialization_WithApiFormat_WorksCorrectly()
        {
            // Arrange
            var json = @"{
                ""checks"": [
                    {
                        ""id"": 456,
                        ""name"": ""Login Flow"",
                        ""active"": true,
                        ""interval"": 300
                    }
                ],
                ""limit"": 1000,
                ""offset"": 0
            }";

            // Act
            var response = JsonConvert.DeserializeObject<TmsChecksResponse>(json);

            // Assert
            Assert.NotNull(response);
            Assert.Single(response.Checks);
            Assert.Equal(456, response.Checks[0].Id);
            Assert.Equal("Login Flow", response.Checks[0].Name);
            Assert.True(response.Checks[0].Active);
            Assert.Equal(300, response.Checks[0].Interval);
            Assert.Equal(1000, response.Limit);
            Assert.Equal(0, response.Offset);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        [InlineData(1000)]
        public void TmsChecksResponse_Limit_AcceptsValidValues(int limit)
        {
            // Act
            var response = new TmsChecksResponse { Limit = limit };

            // Assert
            Assert.Equal(limit, response.Limit);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(10)]
        [InlineData(50)]
        [InlineData(100)]
        public void TmsChecksResponse_Offset_AcceptsValidValues(int offset)
        {
            // Act
            var response = new TmsChecksResponse { Offset = offset };

            // Assert
            Assert.Equal(offset, response.Offset);
        }

        #endregion
    }
}
