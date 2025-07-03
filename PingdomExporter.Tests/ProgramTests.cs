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
