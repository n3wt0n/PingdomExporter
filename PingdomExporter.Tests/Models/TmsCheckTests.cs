using Xunit;
using PingdomExporter.Models;
using System.Collections.Generic;

namespace PingdomExporter.Tests.Models
{
    public class TmsCheckTests
    {
        [Fact]
        public void TmsCheck_Defaults_AreSet()
        {
            var check = new TmsCheck();
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
            Assert.NotNull(check.TeamIds);
        }
    }
}
