using Xunit;
using PingdomExporter.Models;

namespace PingdomExporter.Tests.Models
{
    public class UptimeRobotMonitorTests
    {
        [Fact]
        public void UptimeRobotMonitor_Defaults_AreSet()
        {
            var monitor = new UptimeRobotMonitor();
            Assert.Equal("HTTP", monitor.Type);
            Assert.Equal(string.Empty, monitor.FriendlyName);
            Assert.Equal(string.Empty, monitor.UrlIp);
            Assert.Equal(300, monitor.Interval);
            Assert.Equal(string.Empty, monitor.KeywordType);
            Assert.Equal(string.Empty, monitor.KeywordValue);
            Assert.Equal(string.Empty, monitor.Port);
        }
    }
}
