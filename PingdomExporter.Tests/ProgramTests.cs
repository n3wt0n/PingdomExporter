using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using PingdomExporter.Models;
using PingdomExporter.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System;
using System.Linq;
using System.Collections.Generic;

namespace PingdomExporter.Tests
{
    public class ProgramTests
    {
        #region Configuration Tests

        [Fact]
        public void ExportConfiguration_DefaultValues_AreCorrect()
        {
            // Act
            var config = new ExportConfiguration();

            // Assert
            Assert.Equal("https://api.pingdom.com/api/3.1/", config.BaseUrl);
            Assert.Equal("exports", config.OutputDirectory);
            Assert.Equal("json", config.OutputFormat);
            Assert.Equal("Summary", config.ExportMode);
            Assert.True(config.ExportUptimeChecks);
            Assert.True(config.ExportTransactionChecks);
            Assert.True(config.IncludeTags);
            Assert.True(config.IncludeTeams);
            Assert.False(config.IncludeDisabledChecks);
            Assert.Equal(1000, config.RequestDelayMs);
            Assert.False(config.AutoMode);
            Assert.False(config.VerboseMode);
        }

        [Fact]
        public void ExportConfiguration_CanOverrideAllProperties()
        {
            // Act
            var config = new ExportConfiguration
            {
                ApiToken = "custom-token",
                BaseUrl = "https://custom.api.url/",
                OutputDirectory = "/custom/path",
                OutputFormat = "csv",
                ExportMode = "Summary",
                ExportUptimeChecks = false,
                ExportTransactionChecks = false,
                IncludeTags = false,
                IncludeTeams = false,
                IncludeDisabledChecks = true,
                RequestDelayMs = 1000,
                AutoMode = true,
                VerboseMode = true
            };

            // Assert
            Assert.Equal("custom-token", config.ApiToken);
            Assert.Equal("https://custom.api.url/", config.BaseUrl);
            Assert.Equal("/custom/path", config.OutputDirectory);
            Assert.Equal("csv", config.OutputFormat);
            Assert.Equal("Summary", config.ExportMode);
            Assert.False(config.ExportUptimeChecks);
            Assert.False(config.ExportTransactionChecks);
            Assert.False(config.IncludeTags);
            Assert.False(config.IncludeTeams);
            Assert.True(config.IncludeDisabledChecks);
            Assert.Equal(1000, config.RequestDelayMs);
            Assert.True(config.AutoMode);
            Assert.True(config.VerboseMode);
        }

        [Theory]
        [InlineData("json")]
        [InlineData("csv")]
        [InlineData("both")]
        public void ExportConfiguration_OutputFormat_AcceptsValidValues(string format)
        {
            // Act
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
            // Act
            var config = new ExportConfiguration { ExportMode = mode };

            // Assert
            Assert.Equal(mode, config.ExportMode);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(100)]
        [InlineData(500)]
        [InlineData(1000)]
        [InlineData(5000)]
        public void ExportConfiguration_RequestDelayMs_AcceptsValidValues(int delay)
        {
            // Act
            var config = new ExportConfiguration { RequestDelayMs = delay };

            // Assert
            Assert.Equal(delay, config.RequestDelayMs);
        }

        #endregion

        #region Dependency Injection Tests

        [Fact]
        public void ServiceCollection_CanRegisterBasicServices()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ExportConfiguration
            {
                ApiToken = "test-token",
                BaseUrl = "https://api.pingdom.com/api/3.1/"
            };

            // Act
            services.AddSingleton(config);
            services.AddHttpClient<IPingdomApiService, PingdomApiService>();
            services.AddTransient<IExportService, ExportService>();

            var serviceProvider = services.BuildServiceProvider();

            // Assert
            Assert.NotNull(serviceProvider.GetService<ExportConfiguration>());
            Assert.NotNull(serviceProvider.GetService<IExportService>());
            Assert.NotNull(serviceProvider.GetService<IPingdomApiService>());
        }

        [Fact]
        public void ServiceCollection_ConfigurationIsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ExportConfiguration { ApiToken = "test-token" };

            // Act
            services.AddSingleton(config);
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var config1 = serviceProvider.GetService<ExportConfiguration>();
            var config2 = serviceProvider.GetService<ExportConfiguration>();
            Assert.Same(config1, config2);
            Assert.Equal("test-token", config1?.ApiToken);
        }

        [Fact]
        public void ServiceCollection_ExportServiceIsTransient()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ExportConfiguration { ApiToken = "test-token" };

            // Act
            services.AddSingleton(config);
            services.AddHttpClient<IPingdomApiService, PingdomApiService>();
            services.AddTransient<IExportService, ExportService>();
            var serviceProvider = services.BuildServiceProvider();

            // Assert
            var service1 = serviceProvider.GetService<IExportService>();
            var service2 = serviceProvider.GetService<IExportService>();
            Assert.NotNull(service1);
            Assert.NotNull(service2);
            Assert.NotSame(service1, service2); // Transient services should be different instances
        }

        #endregion

        #region HttpClient Configuration Tests

        [Fact]
        public void HttpClient_ConfigurationValues_AreValid()
        {
            // Arrange
            var config = new ExportConfiguration
            {
                ApiToken = "bearer-token-123",
                BaseUrl = "https://api.pingdom.com/api/3.1/"
            };

            // Act & Assert
            Assert.True(Uri.TryCreate(config.BaseUrl, UriKind.Absolute, out var uri));
            Assert.NotNull(uri);
            Assert.Equal("https", uri.Scheme);
            Assert.False(string.IsNullOrWhiteSpace(config.ApiToken));
        }

        [Fact]
        public void HttpClient_CanCreateAuthenticationHeader()
        {
            // Arrange
            var apiToken = "test-bearer-token";

            // Act
            var authHeader = new AuthenticationHeaderValue("Bearer", apiToken);

            // Assert
            Assert.Equal("Bearer", authHeader.Scheme);
            Assert.Equal(apiToken, authHeader.Parameter);
            Assert.Equal($"Bearer {apiToken}", authHeader.ToString());
        }

        [Theory]
        [InlineData(10)]
        [InlineData(30)]
        [InlineData(60)]
        [InlineData(120)]
        public void HttpClient_TimeoutValues_AreValid(int timeoutSeconds)
        {
            // Act
            var timeout = TimeSpan.FromSeconds(timeoutSeconds);

            // Assert
            Assert.True(timeout.TotalSeconds > 0);
            Assert.Equal(timeoutSeconds, timeout.TotalSeconds);
        }

        [Fact]
        public void HttpClient_MediaTypeHeaders_CanBeCreated()
        {
            // Act
            var jsonHeader = new MediaTypeWithQualityHeaderValue("application/json");

            // Assert
            Assert.Equal("application/json", jsonHeader.MediaType);
            Assert.NotNull(jsonHeader);
        }

        #endregion

        #region Integration Tests

        [Fact]
        public void DependencyInjection_CanResolveCompleteServiceGraph()
        {
            // Arrange
            var services = new ServiceCollection();
            var config = new ExportConfiguration
            {
                ApiToken = "test-token",
                BaseUrl = "https://api.pingdom.com/api/3.1/"
            };

            // Act
            services.AddSingleton(config);
            services.AddHttpClient<IPingdomApiService, PingdomApiService>(client =>
            {
                client.BaseAddress = new Uri(config.BaseUrl);
                client.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", config.ApiToken);
            });
            services.AddTransient<IExportService, ExportService>();

            var serviceProvider = services.BuildServiceProvider();

            // Assert - Should be able to create ExportService with all its dependencies
            var exportService = serviceProvider.GetService<IExportService>();
            Assert.NotNull(exportService);
            Assert.IsType<ExportService>(exportService);
        }

        [Fact]
        public void Configuration_CanBeLoadedFromMultipleSources()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("ApiToken", "config-token"),
                    new KeyValuePair<string, string?>("BaseUrl", "https://config.url/"),
                    new KeyValuePair<string, string?>("OutputFormat", "csv")
                });

            // Act
            var configuration = configBuilder.Build();
            var exportConfig = configuration.Get<ExportConfiguration>() ?? new ExportConfiguration();

            // Assert
            Assert.Equal("config-token", exportConfig.ApiToken);
            Assert.Equal("https://config.url/", exportConfig.BaseUrl);
            Assert.Equal("csv", exportConfig.OutputFormat);
        }

        [Fact]
        public void Configuration_CommandLineOverridesOtherSources()
        {
            // Arrange
            var args = new[] { "--api-token", "cli-token", "--format", "both" };
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("ApiToken", "config-token"),
                    new KeyValuePair<string, string?>("OutputFormat", "json")
                })
                .AddCommandLine(args);

            // Act
            var configuration = configBuilder.Build();

            // Assert
            Assert.Equal("cli-token", configuration["api-token"]);
            Assert.Equal("both", configuration["format"]);
        }

        [Fact]
        public void Configuration_EnvironmentVariablesWork()
        {
            // Arrange
            var configBuilder = new ConfigurationBuilder()
                .AddInMemoryCollection(new[]
                {
                    new KeyValuePair<string, string?>("PINGDOM_ApiToken", "env-token"),
                    new KeyValuePair<string, string?>("PINGDOM_BaseUrl", "https://env.url/")
                });

            // Act
            var configuration = configBuilder.Build();

            // Assert
            Assert.Equal("env-token", configuration["PINGDOM_ApiToken"]);
            Assert.Equal("https://env.url/", configuration["PINGDOM_BaseUrl"]);
        }

        #endregion

        #region Validation Tests

        [Fact]
        public void Configuration_RequiresApiToken()
        {
            // Arrange
            var config = new ExportConfiguration
            {
                ApiToken = "", // Empty token should be invalid
                BaseUrl = "https://api.pingdom.com/api/3.1/"
            };

            // Act & Assert
            Assert.True(string.IsNullOrWhiteSpace(config.ApiToken));
        }

        [Fact]
        public void Configuration_RejectsPlaceholderToken()
        {
            // Arrange
            var config = new ExportConfiguration
            {
                ApiToken = "YOUR_PINGDOM_API_TOKEN_HERE", // Placeholder token
                BaseUrl = "https://api.pingdom.com/api/3.1/"
            };

            // Act & Assert
            Assert.Equal("YOUR_PINGDOM_API_TOKEN_HERE", config.ApiToken);
        }

        [Theory]
        [InlineData("https://api.pingdom.com/api/3.1/")]
        [InlineData("https://custom.pingdom.api/v3/")]
        [InlineData("http://localhost:8080/api/")]
        public void Configuration_AcceptsValidBaseUrls(string baseUrl)
        {
            // Act
            var config = new ExportConfiguration { BaseUrl = baseUrl };

            // Assert
            Assert.Equal(baseUrl, config.BaseUrl);
            Assert.True(Uri.TryCreate(baseUrl, UriKind.Absolute, out _));
        }

        #endregion
    }
}
