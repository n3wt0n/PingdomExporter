using Xunit;
using PingdomExporter.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace PingdomExporter.Tests.Models
{
    public class PingdomCheckTests
    {
        #region PingdomCheck Tests

        [Fact]
        public void PingdomCheck_DefaultConstructor_SetsCorrectDefaults()
        {
            // Act
            var check = new PingdomCheck();

            // Assert
            Assert.Equal(0, check.Id);
            Assert.Equal(string.Empty, check.Name);
            Assert.Equal(string.Empty, check.Hostname);
            Assert.Equal(string.Empty, check.Status);
            Assert.Equal(0, check.Resolution);
            Assert.Equal(string.Empty, check.Type);
            Assert.Equal(0, check.LastTestTime);
            Assert.Equal(0, check.LastResponseTime);
            Assert.Equal(0, check.Created);
            Assert.NotNull(check.Tags);
            Assert.Empty(check.Tags);
            Assert.NotNull(check.ProbeFilters);
            Assert.Empty(check.ProbeFilters);
            Assert.False(check.Ipv6);
            Assert.Equal(0, check.ResponseTimeThreshold);
            Assert.Equal(string.Empty, check.CustomMessage);
            Assert.NotNull(check.IntegrationIds);
            Assert.Empty(check.IntegrationIds);
            Assert.Equal(0, check.SendNotificationWhenDown);
            Assert.Equal(0, check.NotifyAgainEvery);
            Assert.False(check.NotifyWhenBackup);
            Assert.NotNull(check.UserIds);
            Assert.Empty(check.UserIds);
            Assert.NotNull(check.TeamIds);
            Assert.Empty(check.TeamIds);
            Assert.False(check.Paused);
        }

        [Fact]
        public void PingdomCheck_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var tags = new List<Tag> { new Tag { Name = "production", Type = "auto" } };
            var probeFilters = new List<string> { "NA", "EU" };
            var integrationIds = new List<int> { 1, 2, 3 };
            var userIds = new List<int> { 10, 20 };
            var teamIds = new List<int> { 100, 200 };

            // Act
            var check = new PingdomCheck
            {
                Id = 12345,
                Name = "Test Website",
                Hostname = "example.com",
                Status = "up",
                Resolution = 5,
                Type = "http",
                LastTestTime = 1640995200,
                LastResponseTime = 250,
                Created = 1640908800,
                Tags = tags,
                ProbeFilters = probeFilters,
                Ipv6 = true,
                ResponseTimeThreshold = 5000,
                CustomMessage = "Custom alert message",
                IntegrationIds = integrationIds,
                SendNotificationWhenDown = 2,
                NotifyAgainEvery = 5,
                NotifyWhenBackup = true,
                UserIds = userIds,
                TeamIds = teamIds,
                Paused = true
            };

            // Assert
            Assert.Equal(12345, check.Id);
            Assert.Equal("Test Website", check.Name);
            Assert.Equal("example.com", check.Hostname);
            Assert.Equal("up", check.Status);
            Assert.Equal(5, check.Resolution);
            Assert.Equal("http", check.Type);
            Assert.Equal(1640995200, check.LastTestTime);
            Assert.Equal(250, check.LastResponseTime);
            Assert.Equal(1640908800, check.Created);
            Assert.Equal(tags, check.Tags);
            Assert.Equal(probeFilters, check.ProbeFilters);
            Assert.True(check.Ipv6);
            Assert.Equal(5000, check.ResponseTimeThreshold);
            Assert.Equal("Custom alert message", check.CustomMessage);
            Assert.Equal(integrationIds, check.IntegrationIds);
            Assert.Equal(2, check.SendNotificationWhenDown);
            Assert.Equal(5, check.NotifyAgainEvery);
            Assert.True(check.NotifyWhenBackup);
            Assert.Equal(userIds, check.UserIds);
            Assert.Equal(teamIds, check.TeamIds);
            Assert.True(check.Paused);
        }

        [Fact]
        public void PingdomCheck_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var check = new PingdomCheck
            {
                Id = 123,
                Name = "Test Check",
                Hostname = "test.com",
                Status = "up",
                Type = "http"
            };

            // Act
            var json = JsonConvert.SerializeObject(check);
            var deserialized = JsonConvert.DeserializeObject<PingdomCheck>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(check.Id, deserialized.Id);
            Assert.Equal(check.Name, deserialized.Name);
            Assert.Equal(check.Hostname, deserialized.Hostname);
            Assert.Equal(check.Status, deserialized.Status);
        }

        [Fact]
        public void PingdomCheck_JsonDeserialization_WithApiFormat_WorksCorrectly()
        {
            // Arrange
            var json = @"{
                ""id"": 456,
                ""name"": ""API Test"",
                ""hostname"": ""api.example.com"",
                ""status"": ""up"",
                ""resolution"": 1,
                ""type"": ""http"",
                ""lasttesttime"": 1640995200,
                ""lastresponsetime"": 150,
                ""created"": 1640908800,
                ""tags"": [
                    {""name"": ""production"", ""type"": ""auto"", ""count"": 5}
                ],
                ""ipv6"": true,
                ""paused"": false
            }";

            // Act
            var check = JsonConvert.DeserializeObject<PingdomCheck>(json);

            // Assert
            Assert.NotNull(check);
            Assert.Equal(456, check.Id);
            Assert.Equal("API Test", check.Name);
            Assert.Equal("api.example.com", check.Hostname);
            Assert.Equal("up", check.Status);
            Assert.Equal(1, check.Resolution);
            Assert.Equal(1640995200, check.LastTestTime);
            Assert.Equal(150, check.LastResponseTime);
            Assert.Equal(1640908800, check.Created);
            Assert.True(check.Ipv6);
            Assert.False(check.Paused);
            Assert.Single(check.Tags);
            Assert.Equal("production", check.Tags[0].Name);
        }

        #endregion

        #region FlexibleTypeConverter Tests

        [Fact]
        public void FlexibleTypeConverter_CanConvert_ReturnsTrue()
        {
            // Arrange
            var converter = new FlexibleTypeConverter();

            // Act & Assert
            Assert.True(converter.CanConvert(typeof(object)));
            Assert.True(converter.CanConvert(typeof(string)));
            Assert.True(converter.CanConvert(typeof(JObject)));
        }

        [Fact]
        public void FlexibleTypeConverter_CanConvert_ReturnsFalseForOtherTypes()
        {
            // Arrange
            var converter = new FlexibleTypeConverter();

            // Act & Assert
            Assert.False(converter.CanConvert(typeof(int)));
            Assert.False(converter.CanConvert(typeof(bool)));
            Assert.False(converter.CanConvert(typeof(DateTime)));
        }

        [Fact]
        public void FlexibleTypeConverter_ReadJson_String_ReturnsString()
        {
            // Arrange
            var converter = new FlexibleTypeConverter();
            var json = "\"http\"";
            var reader = new JsonTextReader(new StringReader(json));
            reader.Read();

            // Act
            var result = converter.ReadJson(reader, typeof(object), null, new JsonSerializer());

            // Assert
            Assert.Equal("http", result);
        }

        [Fact]
        public void FlexibleTypeConverter_ReadJson_Object_ReturnsJObject()
        {
            // Arrange
            var converter = new FlexibleTypeConverter();
            var json = "{\"http\":{\"url\":\"https://example.com\"}}";
            var reader = new JsonTextReader(new StringReader(json));
            reader.Read();

            // Act
            var result = converter.ReadJson(reader, typeof(object), null, new JsonSerializer());

            // Assert
            Assert.IsType<JObject>(result);
            var jObject = result as JObject;
            Assert.NotNull(jObject);
            Assert.True(jObject.ContainsKey("http"));
        }

        [Fact]
        public void FlexibleTypeConverter_ReadJson_EmptyString_ReturnsEmptyString()
        {
            // Arrange
            var converter = new FlexibleTypeConverter();
            var json = "\"\"";
            var reader = new JsonTextReader(new StringReader(json));
            reader.Read();

            // Act
            var result = converter.ReadJson(reader, typeof(object), null, new JsonSerializer());

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void FlexibleTypeConverter_ReadJson_Null_ReturnsEmptyString()
        {
            // Arrange
            var converter = new FlexibleTypeConverter();
            var json = "null";
            var reader = new JsonTextReader(new StringReader(json));
            reader.Read();

            // Act
            var result = converter.ReadJson(reader, typeof(object), null, new JsonSerializer());

            // Assert
            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void FlexibleTypeConverter_WriteJson_String_WritesString()
        {
            // Arrange
            var converter = new FlexibleTypeConverter();
            var stringWriter = new StringWriter();
            var writer = new JsonTextWriter(stringWriter);
            var serializer = new JsonSerializer();

            // Act
            converter.WriteJson(writer, "http", serializer);

            // Assert
            Assert.Equal("\"http\"", stringWriter.ToString());
        }

        [Fact]
        public void FlexibleTypeConverter_WriteJson_Object_SerializesObject()
        {
            // Arrange
            var converter = new FlexibleTypeConverter();
            var stringWriter = new StringWriter();
            var writer = new JsonTextWriter(stringWriter);
            var serializer = new JsonSerializer();
            var obj = new { type = "http", url = "https://example.com" };

            // Act
            converter.WriteJson(writer, obj, serializer);

            // Assert
            var result = stringWriter.ToString();
            Assert.Contains("\"type\":\"http\"", result);
            Assert.Contains("\"url\":\"https://example.com\"", result);
        }

        #endregion

        #region Tag Tests

        [Fact]
        public void Tag_DefaultConstructor_SetsCorrectDefaults()
        {
            // Act
            var tag = new Tag();

            // Assert
            Assert.Equal(string.Empty, tag.Name);
            Assert.Equal(string.Empty, tag.Type);
            Assert.Equal(0, tag.Count);
        }

        [Fact]
        public void Tag_AllProperties_CanBeSetAndRetrieved()
        {
            // Act
            var tag = new Tag
            {
                Name = "production",
                Type = "auto",
                Count = 5
            };

            // Assert
            Assert.Equal("production", tag.Name);
            Assert.Equal("auto", tag.Type);
            Assert.Equal(5, tag.Count);
        }

        [Fact]
        public void Tag_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var tag = new Tag
            {
                Name = "staging",
                Type = "manual",
                Count = 3
            };

            // Act
            var json = JsonConvert.SerializeObject(tag);
            var deserialized = JsonConvert.DeserializeObject<Tag>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(tag.Name, deserialized.Name);
            Assert.Equal(tag.Type, deserialized.Type);
            Assert.Equal(tag.Count, deserialized.Count);
        }

        #endregion

        #region CheckType Tests

        [Fact]
        public void CheckType_DefaultConstructor_SetsNullValues()
        {
            // Act
            var checkType = new CheckType();

            // Assert
            Assert.Null(checkType.Http);
            Assert.Null(checkType.Tcp);
            Assert.Null(checkType.Ping);
            Assert.Null(checkType.Dns);
        }

        [Fact]
        public void CheckType_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var httpDetails = new { url = "https://example.com" };
            var tcpDetails = new { port = 80 };
            var pingDetails = new { };
            var dnsDetails = new { expectedip = "1.2.3.4" };

            // Act
            var checkType = new CheckType
            {
                Http = httpDetails,
                Tcp = tcpDetails,
                Ping = pingDetails,
                Dns = dnsDetails
            };

            // Assert
            Assert.Equal(httpDetails, checkType.Http);
            Assert.Equal(tcpDetails, checkType.Tcp);
            Assert.Equal(pingDetails, checkType.Ping);
            Assert.Equal(dnsDetails, checkType.Dns);
        }

        #endregion

        #region HttpCheckDetails Tests

        [Fact]
        public void HttpCheckDetails_DefaultConstructor_SetsCorrectDefaults()
        {
            // Act
            var details = new HttpCheckDetails();

            // Assert
            Assert.Equal(string.Empty, details.Url);
            Assert.False(details.Encryption);
            Assert.Equal(0, details.Port);
            Assert.Null(details.Auth);
            Assert.Null(details.ShouldContain);
            Assert.Null(details.ShouldNotContain);
            Assert.Null(details.PostData);
            Assert.NotNull(details.RequestHeaders);
            Assert.Empty(details.RequestHeaders);
            Assert.False(details.VerifyCertificate);
            Assert.Equal(0, details.SslDownDaysBefore);
        }

        [Fact]
        public void HttpCheckDetails_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var headers = new Dictionary<string, string> { { "User-Agent", "Test" } };

            // Act
            var details = new HttpCheckDetails
            {
                Url = "https://api.example.com/health",
                Encryption = true,
                Port = 443,
                Auth = "user:pass",
                ShouldContain = "OK",
                ShouldNotContain = "ERROR",
                PostData = "{\"test\":true}",
                RequestHeaders = headers,
                VerifyCertificate = true,
                SslDownDaysBefore = 7
            };

            // Assert
            Assert.Equal("https://api.example.com/health", details.Url);
            Assert.True(details.Encryption);
            Assert.Equal(443, details.Port);
            Assert.Equal("user:pass", details.Auth);
            Assert.Equal("OK", details.ShouldContain);
            Assert.Equal("ERROR", details.ShouldNotContain);
            Assert.Equal("{\"test\":true}", details.PostData);
            Assert.Equal(headers, details.RequestHeaders);
            Assert.True(details.VerifyCertificate);
            Assert.Equal(7, details.SslDownDaysBefore);
        }

        #endregion

        #region TcpCheckDetails Tests

        [Fact]
        public void TcpCheckDetails_DefaultConstructor_SetsCorrectDefaults()
        {
            // Act
            var details = new TcpCheckDetails();

            // Assert
            Assert.Equal(0, details.Port);
            Assert.Null(details.StringToSend);
            Assert.Null(details.StringToExpect);
        }

        [Fact]
        public void TcpCheckDetails_AllProperties_CanBeSetAndRetrieved()
        {
            // Act
            var details = new TcpCheckDetails
            {
                Port = 25,
                StringToSend = "HELO test",
                StringToExpect = "220"
            };

            // Assert
            Assert.Equal(25, details.Port);
            Assert.Equal("HELO test", details.StringToSend);
            Assert.Equal("220", details.StringToExpect);
        }

        #endregion

        #region DnsCheckDetails Tests

        [Fact]
        public void DnsCheckDetails_DefaultConstructor_SetsCorrectDefaults()
        {
            // Act
            var details = new DnsCheckDetails();

            // Assert
            Assert.Equal(string.Empty, details.ExpectedIp);
            Assert.Equal(string.Empty, details.NameServer);
        }

        [Fact]
        public void DnsCheckDetails_AllProperties_CanBeSetAndRetrieved()
        {
            // Act
            var details = new DnsCheckDetails
            {
                ExpectedIp = "192.168.1.1",
                NameServer = "8.8.8.8"
            };

            // Assert
            Assert.Equal("192.168.1.1", details.ExpectedIp);
            Assert.Equal("8.8.8.8", details.NameServer);
        }

        #endregion

        #region PingdomChecksResponse Tests

        [Fact]
        public void PingdomChecksResponse_DefaultConstructor_InitializesCollections()
        {
            // Act
            var response = new PingdomChecksResponse();

            // Assert
            Assert.NotNull(response.Checks);
            Assert.Empty(response.Checks);
            Assert.Null(response.Counts);
        }

        [Fact]
        public void PingdomChecksResponse_AllProperties_CanBeSetAndRetrieved()
        {
            // Arrange
            var checks = new List<PingdomCheck> { new PingdomCheck { Id = 1, Name = "Test" } };
            var counts = new CheckCounts { Total = 1, Limited = 0, Filtered = 0 };

            // Act
            var response = new PingdomChecksResponse
            {
                Checks = checks,
                Counts = counts
            };

            // Assert
            Assert.Equal(checks, response.Checks);
            Assert.Equal(counts, response.Counts);
        }

        [Fact]
        public void PingdomChecksResponse_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var response = new PingdomChecksResponse
            {
                Checks = new List<PingdomCheck> { new PingdomCheck { Id = 123, Name = "Test Check" } },
                Counts = new CheckCounts { Total = 1, Limited = 0, Filtered = 0 }
            };

            // Act
            var json = JsonConvert.SerializeObject(response);
            var deserialized = JsonConvert.DeserializeObject<PingdomChecksResponse>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Single(deserialized.Checks);
            Assert.Equal(123, deserialized.Checks[0].Id);
            Assert.Equal("Test Check", deserialized.Checks[0].Name);
            Assert.NotNull(deserialized.Counts);
            Assert.Equal(1, deserialized.Counts.Total);
        }

        #endregion

        #region CheckCounts Tests

        [Fact]
        public void CheckCounts_DefaultConstructor_SetsZeroValues()
        {
            // Act
            var counts = new CheckCounts();

            // Assert
            Assert.Equal(0, counts.Total);
            Assert.Equal(0, counts.Limited);
            Assert.Equal(0, counts.Filtered);
        }

        [Fact]
        public void CheckCounts_AllProperties_CanBeSetAndRetrieved()
        {
            // Act
            var counts = new CheckCounts
            {
                Total = 100,
                Limited = 50,
                Filtered = 25
            };

            // Assert
            Assert.Equal(100, counts.Total);
            Assert.Equal(50, counts.Limited);
            Assert.Equal(25, counts.Filtered);
        }

        [Fact]
        public void CheckCounts_JsonSerialization_WorksCorrectly()
        {
            // Arrange
            var counts = new CheckCounts
            {
                Total = 75,
                Limited = 25,
                Filtered = 10
            };

            // Act
            var json = JsonConvert.SerializeObject(counts);
            var deserialized = JsonConvert.DeserializeObject<CheckCounts>(json);

            // Assert
            Assert.NotNull(deserialized);
            Assert.Equal(75, deserialized.Total);
            Assert.Equal(25, deserialized.Limited);
            Assert.Equal(10, deserialized.Filtered);
        }

        #endregion
    }
}
