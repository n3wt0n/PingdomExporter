using Xunit;
using PingdomExporter.Services;
using PingdomExporter.Models;
using System.CommandLine.Parsing;
using System.Threading.Tasks;

namespace PingdomExporter.Tests.Services
{
    public class CliHandlerTests
    {
        [Fact]
        public async Task ParseAsync_HelpFlag_ReturnsShouldExit()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--help" });
            Assert.True(result.ShouldExit);
            Assert.Equal(0, result.ExitCode);
        }

        [Fact]
        public async Task ParseAsync_InvalidFlag_ReturnsShouldExitWithError()
        {
            var handler = new CliHandler();
            var result = await handler.ParseAsync(new[] { "--notarealflag" });
            Assert.True(result.ShouldExit);
            Assert.Equal(1, result.ExitCode);
        }
    }
}
