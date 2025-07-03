using Xunit;
using PingdomExporter.Services;
using PingdomExporter.Models;
using System.Threading.Tasks;

namespace PingdomExporter.Tests.Services
{
    public class CliHandlerTests
    {
        #region Help and Version Tests

        [Fact]
        public async Task ParseAsync_HelpFlag_ReturnsShouldExit()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--help" });
            Assert.True(result.ShouldExit);
            Assert.Equal(0, result.ExitCode);
        }

        [Fact]
        public async Task ParseAsync_HelpShortFlag_ReturnsShouldExit()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "-h" });
            Assert.True(result.ShouldExit);
            Assert.Equal(0, result.ExitCode);
        }

        [Fact]
        public async Task ParseAsync_VersionFlag_ReturnsShouldExit()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--version" });
            Assert.True(result.ShouldExit);
            Assert.Equal(0, result.ExitCode);
        }

        #endregion

        #region Error Handling Tests

        [Fact]
        public async Task ParseAsync_InvalidFlag_ReturnsShouldExitWithError()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--notarealflag" });
            Assert.True(result.ShouldExit);
            Assert.Equal(1, result.ExitCode);
        }

        [Fact]
        public async Task ParseAsync_InvalidValue_ReturnsShouldExitWithError()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--delay", "invalid" });
            Assert.True(result.ShouldExit);
            Assert.Equal(1, result.ExitCode);
        }

        #endregion

        #region API Configuration Tests

        [Fact]
        public async Task ParseAsync_ApiTokenLongFlag_SetsApiToken()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--api-token", "test-token" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal("test-token", result.Configuration.ApiToken);
        }

        [Fact]
        public async Task ParseAsync_ApiTokenShortFlag_SetsApiToken()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "-t", "test-token" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal("test-token", result.Configuration.ApiToken);
        }

        [Fact]
        public async Task ParseAsync_BaseUrlLongFlag_SetsBaseUrl()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--base-url", "https://custom.api.com" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal("https://custom.api.com", result.Configuration.BaseUrl);
        }

        [Fact]
        public async Task ParseAsync_BaseUrlShortFlag_SetsBaseUrl()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "-u", "https://custom.api.com" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal("https://custom.api.com", result.Configuration.BaseUrl);
        }

        #endregion

        #region Output Configuration Tests

        [Fact]
        public async Task ParseAsync_OutputDirLongFlag_SetsOutputDirectory()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--output-dir", "/custom/path" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal("/custom/path", result.Configuration.OutputDirectory);
        }

        [Fact]
        public async Task ParseAsync_OutputDirShortFlag_SetsOutputDirectory()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "-o", "/custom/path" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal("/custom/path", result.Configuration.OutputDirectory);
        }

        [Fact]
        public async Task ParseAsync_FormatLongFlag_SetsOutputFormat()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--format", "csv" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal("csv", result.Configuration.OutputFormat);
        }

        [Fact]
        public async Task ParseAsync_FormatShortFlag_SetsOutputFormat()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "-f", "both" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal("both", result.Configuration.OutputFormat);
        }

        [Fact]
        public async Task ParseAsync_ExportModeLongFlag_SetsExportMode()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--export-mode", "Full" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal("Full", result.Configuration.ExportMode);
        }

        [Fact]
        public async Task ParseAsync_ExportModeShortFlag_SetsExportMode()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "-m", "UptimeRobot" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal("UptimeRobot", result.Configuration.ExportMode);
        }

        #endregion

        #region Export Options Tests (Negative Flags)

        [Fact]
        public async Task ParseAsync_NoUptimeFlag_DisablesUptimeChecks()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--no-uptime" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.False(result.Configuration.ExportUptimeChecks);
        }

        [Fact]
        public async Task ParseAsync_NoTransactionFlag_DisablesTransactionChecks()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--no-transaction" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.False(result.Configuration.ExportTransactionChecks);
        }

        [Fact]
        public async Task ParseAsync_NoTagsFlag_DisablesTags()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--no-tags" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.False(result.Configuration.IncludeTags);
        }

        [Fact]
        public async Task ParseAsync_NoTeamsFlag_DisablesTeams()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--no-teams" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.False(result.Configuration.IncludeTeams);
        }

        [Fact]
        public async Task ParseAsync_IncludeDisabledFlag_EnablesDisabledChecks()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--include-disabled" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.True(result.Configuration.IncludeDisabledChecks);
        }

        #endregion

        #region Performance Options Tests

        [Fact]
        public async Task ParseAsync_DelayLongFlag_SetsRequestDelay()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--delay", "2000" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal(2000, result.Configuration.RequestDelayMs);
        }

        [Fact]
        public async Task ParseAsync_DelayShortFlag_SetsRequestDelay()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "-d", "500" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal(500, result.Configuration.RequestDelayMs);
        }

        [Fact]
        public async Task ParseAsync_DelayZero_SetsRequestDelayToZero()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--delay", "0" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal(0, result.Configuration.RequestDelayMs);
        }

        #endregion

        #region Execution Options Tests

        [Fact]
        public async Task ParseAsync_AutoLongFlag_EnablesAutoMode()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--auto" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.True(result.Configuration.AutoMode);
        }

        [Fact]
        public async Task ParseAsync_AutoShortFlag_EnablesAutoMode()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "-y" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.True(result.Configuration.AutoMode);
        }

        [Fact]
        public async Task ParseAsync_VerboseLongFlag_EnablesVerboseMode()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--verbose" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.True(result.Configuration.VerboseMode);
        }

        [Fact]
        public async Task ParseAsync_VerboseShortFlag_EnablesVerboseMode()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "-v" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.True(result.Configuration.VerboseMode);
        }

        #endregion

        #region Default Values Tests

        [Fact]
        public async Task ParseAsync_NoArguments_ReturnsDefaultConfiguration()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new string[0]);
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            
            // Check that CLI-provided values are empty (will be filled from config file or defaults later)
            Assert.Equal(string.Empty, result.Configuration.ApiToken);
            Assert.Equal(string.Empty, result.Configuration.BaseUrl);
            Assert.Equal(string.Empty, result.Configuration.OutputDirectory);
            Assert.Equal(string.Empty, result.Configuration.OutputFormat);
            Assert.Equal(string.Empty, result.Configuration.ExportMode);
            
            // Check boolean defaults (these should remain as model defaults since no negative flags were used)
            Assert.True(result.Configuration.ExportUptimeChecks);
            Assert.True(result.Configuration.ExportTransactionChecks);
            Assert.True(result.Configuration.IncludeTags);
            Assert.True(result.Configuration.IncludeTeams);
            Assert.False(result.Configuration.IncludeDisabledChecks);
            
            // Check CLI-specific defaults
            Assert.False(result.Configuration.AutoMode);
            Assert.False(result.Configuration.VerboseMode);
            
            // Check delay default
            Assert.Equal(1000, result.Configuration.RequestDelayMs);
        }

        #endregion

        #region Multiple Arguments Tests

        [Fact]
        public async Task ParseAsync_MultipleArguments_SetsAllValues()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { 
                "--api-token", "test-token",
                "--base-url", "https://custom.api.com",
                "--output-dir", "/custom/path",
                "--format", "csv",
                "--export-mode", "Full",
                "--delay", "2000",
                "--auto",
                "--verbose",
                "--no-uptime",
                "--include-disabled"
            });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            
            Assert.Equal("test-token", result.Configuration.ApiToken);
            Assert.Equal("https://custom.api.com", result.Configuration.BaseUrl);
            Assert.Equal("/custom/path", result.Configuration.OutputDirectory);
            Assert.Equal("csv", result.Configuration.OutputFormat);
            Assert.Equal("Full", result.Configuration.ExportMode);
            Assert.Equal(2000, result.Configuration.RequestDelayMs);
            Assert.True(result.Configuration.AutoMode);
            Assert.True(result.Configuration.VerboseMode);
            Assert.False(result.Configuration.ExportUptimeChecks);
            Assert.True(result.Configuration.IncludeDisabledChecks);
        }

        [Fact]
        public async Task ParseAsync_MixedShortAndLongFlags_SetsAllValues()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { 
                "-t", "test-token",
                "--base-url", "https://custom.api.com",
                "-o", "/custom/path",
                "--format", "both",
                "-m", "Summary",
                "-d", "500",
                "-y",
                "--verbose"
            });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            
            Assert.Equal("test-token", result.Configuration.ApiToken);
            Assert.Equal("https://custom.api.com", result.Configuration.BaseUrl);
            Assert.Equal("/custom/path", result.Configuration.OutputDirectory);
            Assert.Equal("both", result.Configuration.OutputFormat);
            Assert.Equal("Summary", result.Configuration.ExportMode);
            Assert.Equal(500, result.Configuration.RequestDelayMs);
            Assert.True(result.Configuration.AutoMode);
            Assert.True(result.Configuration.VerboseMode);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public async Task ParseAsync_EmptyStringValues_SetsEmptyStrings()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { 
                "--api-token", "",
                "--base-url", "",
                "--output-dir", ""
            });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal("", result.Configuration.ApiToken);
            Assert.Equal("", result.Configuration.BaseUrl);
            Assert.Equal("", result.Configuration.OutputDirectory);
        }

        [Fact]
        public async Task ParseAsync_NegativeDelay_SetsNegativeValue()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--delay", "-100" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal(-100, result.Configuration.RequestDelayMs);
        }

        [Fact]
        public async Task ParseAsync_AllNegativeFlags_DisablesAllFeatures()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { 
                "--no-uptime",
                "--no-transaction",
                "--no-tags",
                "--no-teams"
            });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.False(result.Configuration.ExportUptimeChecks);
            Assert.False(result.Configuration.ExportTransactionChecks);
            Assert.False(result.Configuration.IncludeTags);
            Assert.False(result.Configuration.IncludeTeams);
        }

        #endregion

        #region Configuration Preservation Tests

        [Fact]
        public async Task ParseAsync_OnlySpecifiedValuesAreSet_OthersRemainEmpty()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--api-token", "test-token" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            
            // Only API token should be set
            Assert.Equal("test-token", result.Configuration.ApiToken);
            
            // Others should remain empty (to be filled from config file later)
            Assert.Equal(string.Empty, result.Configuration.BaseUrl);
            Assert.Equal(string.Empty, result.Configuration.OutputDirectory);
            Assert.Equal(string.Empty, result.Configuration.OutputFormat);
            Assert.Equal(string.Empty, result.Configuration.ExportMode);
        }

        [Fact]
        public async Task ParseAsync_DelayNotSpecified_UsesDefault()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--api-token", "test-token" });
            
            Assert.False(result.ShouldExit);
            Assert.NotNull(result.Configuration);
            Assert.Equal(1000, result.Configuration.RequestDelayMs); // Default value
        }

        #endregion
    }
}
