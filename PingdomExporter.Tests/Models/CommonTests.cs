using Xunit;
using PingdomExporter.Models;

namespace PingdomExporter.Tests.Models
{
    public class CommonTests
    {
        [Fact]
        public void PingdomApiError_Defaults_AreSet()
        {
            var error = new PingdomApiError();
            Assert.NotNull(error.Error);
        }

        [Fact]
        public void ErrorDetails_Properties_Work()
        {
            var details = new ErrorDetails
            {
                StatusCode = 404,
                StatusDescription = "Not Found",
                ErrorMessage = "Resource not found"
            };
            Assert.Equal(404, details.StatusCode);
            Assert.Equal("Not Found", details.StatusDescription);
            Assert.Equal("Resource not found", details.ErrorMessage);
        }

        [Fact]
        public void ExportConfiguration_Defaults_AreSet()
        {
            var config = new ExportConfiguration();
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
    }
}
